// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Config;
    using KeyVault;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Redis;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Telemetry;

    public class CacheProvider : ICacheProvider
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly DistributedCacheEntryOptions cacheEntryOptions;
        private readonly ILogger<CacheProvider> logger;
        private readonly MultilayerCache multilayerCache;
        private readonly CacheSettings settings;

        public CacheProvider(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<CacheProvider>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<CacheSettings>();
            if (settings.MemoryCache == null) throw new InvalidOperationException("Memory cache is not configured");

            cacheEntryOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = settings.TimeToLive
            };

            multilayerCache = new MultilayerCache(new SpillableMemoryCache(serviceProvider, loggerFactory))
            {
                PopulateLayersOnGet = true
            };
            if (settings.BlobCache != null) multilayerCache.AppendLayer(new BlobCache(serviceProvider, loggerFactory));

            if (settings.RedisCache != null)
            {
                var kvClient = serviceProvider.GetRequiredService<IKeyVaultClient>();
                var vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
                var accessKey = kvClient.GetSecretAsync(vaultSettings.VaultUrl, settings.RedisCache.AccessKeySecretName)
                    .GetAwaiter().GetResult();
                var connStr = $"{settings.RedisCache.HostName},password={accessKey.Value},ssl=True,abortConnect=False";
                var redisCacheOptions = new RedisCacheOptions
                {
                    Configuration = connStr
                };
                var redisCache = new RedisCache(new OptionsWrapper<RedisCacheOptions>(redisCacheOptions));
                multilayerCache.AppendLayer(redisCache);
            }
        }

        public async Task<T> GetOrUpdateAsync<T>(
            string key,
            Func<Task<DateTimeOffset>> getLastModificationTime,
            Func<Task<T>> getItem,
            CancellationToken cancel) where T : class, new()
        {
            logger.LogInformation($"get cached item, key={key}");
            var value = await multilayerCache.GetAsync(key, cancel);
            CachedItem<T> cachedItem;

            async Task<T> RefreshItem()
            {
                var item = await getItem();
                cachedItem = new CachedItem<T>(item);
                string serializeObject = Serialize(cachedItem);
                value = Encoding.UTF8.GetBytes(serializeObject);
#pragma warning disable 4014
                multilayerCache.SetAsync(key, value, cacheEntryOptions, cancel);
#pragma warning restore 4014
                return item;
            }

            if (value == null)
            {
                appTelemetry.RecordMetric("cache-miss", 1, ("key", key));
                return await RefreshItem();
            }

            var json = Encoding.UTF8.GetString(value);
            cachedItem = JsonConvert.DeserializeObject<CachedItem<T>>(json);
            var needRefresh = false;
            if (cachedItem.CreatedOn.Add(settings.TimeToLive) < DateTimeOffset.UtcNow)
            {
                appTelemetry.RecordMetric("cache-expired", 1, ("key", key));
                needRefresh = true;
            }
            else
            {
                var lastUpdateTime = await getLastModificationTime();
                if (lastUpdateTime != default && lastUpdateTime > cachedItem.CreatedOn) needRefresh = true;
            }

            if (!needRefresh) return cachedItem.Value;

            return await RefreshItem();
        }

        public T GetOrUpdate<T>(string key, Func<DateTimeOffset> getLastModificationTime, Func<T> getItem, CancellationToken cancel = default) where T : class, new()
        {
            logger.LogInformation($"get cached item, key={key}");
            var value = multilayerCache.Get(key);
            CachedItem<T> cachedItem;

            T RefreshItem()
            {
                var item = getItem();
                cachedItem = new CachedItem<T>(item);
                string serializeObject = Serialize(cachedItem);
                value = Encoding.UTF8.GetBytes(serializeObject);
                multilayerCache.SetAsync(key, value, cacheEntryOptions, cancel); // donot wait
                return item;
            }

            if (value == null)
            {
                appTelemetry.RecordMetric("cache-miss", 1, ("key", key));
                return RefreshItem();
            }

            var json = Encoding.UTF8.GetString(value);
            cachedItem = JsonConvert.DeserializeObject<CachedItem<T>>(json);
            var needRefresh = false;
            if (cachedItem.CreatedOn.Add(settings.TimeToLive) < DateTimeOffset.UtcNow)
            {
                appTelemetry.RecordMetric("cache-expired", 1, ("key", key));
                needRefresh = true;
            }
            else
            {
                var lastUpdateTime = getLastModificationTime();
                if (lastUpdateTime != default && lastUpdateTime > cachedItem.CreatedOn) needRefresh = true;
            }

            if (!needRefresh) return cachedItem.Value;

            return RefreshItem();
        }

        public async Task Set<T>(string key, T item, CancellationToken cancel) where T : class, new()
        {
            var cachedItem = new CachedItem<T>(item);
            string serializeObject = Serialize(cachedItem);
            await multilayerCache.SetAsync(
                key,
                Encoding.UTF8.GetBytes(serializeObject),
                cancel);
        }

        public Task ClearAll()
        {
            throw new NotImplementedException();
        }

        private string Serialize<T>(CachedItem<T> cachedItem) where T : class, new()
        {
            var tempFile = Path.GetTempFileName();
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            using (var stream = File.OpenWrite(tempFile))
            {
                using (var writer = new StreamWriter(stream))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        var serializer = new JsonSerializer
                        {
                            Formatting = Formatting.None,
                            MaxDepth = 3,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        serializer.Serialize(jsonWriter, cachedItem);
                        jsonWriter.Flush();
                    }
                }
            }

            var serializedString = File.ReadAllText(tempFile);
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            return serializedString;
        }
    }
}