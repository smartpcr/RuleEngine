// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpillableMemoryCache.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Config;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Telemetry;

    /// <summary>
    ///     do not do file lock when performing read/write, log exception whenever file is locked
    /// </summary>
    public class SpillableMemoryCache : IDistributedCache
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly string cacheFolder;
        private readonly CacheSettings cacheSettings;
        private readonly ILogger<SpillableMemoryCache> logger;
        private readonly MemoryCache memoryCache;

        public SpillableMemoryCache(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<SpillableMemoryCache>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var configration = serviceProvider.GetRequiredService<IConfiguration>();
            cacheSettings = configration.GetConfiguredSettings<CacheSettings>();
            var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (binFolder == null) throw new InvalidOperationException("invalid bin folder");
            cacheFolder = Path.Combine(binFolder, cacheSettings.FileCache.CacheFolder);
            logger.LogInformation($"set cache folder to: {cacheFolder}");
            if (!Directory.Exists(cacheFolder))
            {
                logger.LogInformation($"create cache folder: {cacheFolder}");
                Directory.CreateDirectory(cacheFolder);
            }

            var cacheOptions = new MemoryCacheOptions
            {
                Clock = new SystemClock(),
                CompactionPercentage = cacheSettings.MemoryCache.CompactionPercentage,
                SizeLimit = cacheSettings.MemoryCache.SizeLimit,
                ExpirationScanFrequency = cacheSettings.TimeToLive
            };
            memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(cacheOptions));
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            logger.LogInformation($"get spillable memory cache: {key}");
            try
            {
                if (memoryCache.TryGetValue(key, out var value) && value is byte[] cachedValue)
                {
                    appTelemetry.RecordMetric("memory-cache-hit", 1, ("key", key));
                    return cachedValue;
                }

                appTelemetry.RecordMetric("memory-cache-miss", 1, ("key", key));

                var cacheFile = Path.Combine(cacheFolder, key);
                if (File.Exists(cacheFile))
                {
                    if (File.GetCreationTimeUtc(cacheFile).Add(cacheSettings.TimeToLive) < DateTimeOffset.UtcNow)
                    {
                        appTelemetry.RecordMetric("file-cache-expired", 1, ("key", key));
                        return null;
                    }
                }
                else
                {
                    appTelemetry.RecordMetric("file-cache-miss", 1, ("key", key));
                    return null;
                }

                var fileContent = await File.ReadAllBytesAsync(cacheFile, token);
                appTelemetry.RecordMetric("file-cache-hit", 1, ("key", key));
                var size = (int) Math.Ceiling((double) fileContent.Length / 1000000); // MB
                var entryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size)
                    .SetSlidingExpiration(cacheSettings.TimeToLive);
                memoryCache.Set(key, fileContent, entryOptions);

                return fileContent;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"unable to get cache: {key}");
                return null;
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            logger.LogInformation($"set spillable cache: key={key}, length={value.Length}");
            try
            {
                var size = (int) Math.Ceiling((double) value.Length / 1000000); // MB
                var entryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size)
                    .SetSlidingExpiration(cacheSettings.TimeToLive);
                memoryCache.Set(key, value, entryOptions);
                var cacheFile = Path.Combine(cacheFolder, key);
                if (File.Exists(cacheFile)) File.Delete(cacheFile);

                await File.WriteAllBytesAsync(cacheFile, value, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed set cache, key={key}, length={value.Length}");
            }
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            logger.LogInformation($"removing cache, {key}");
            try
            {
                memoryCache.Remove(key);
                var cacheFile = Path.Combine(cacheFolder, key);
                if (File.Exists(cacheFile)) File.Delete(cacheFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to remove cache: {key}");
            }

            return Task.CompletedTask;
        }
    }
}