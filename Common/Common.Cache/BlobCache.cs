// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Microsoft Corporation">
// //   Copyright (c) 2017 Microsoft Corporation.  All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Config;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Storage;
    using Telemetry;

    public class BlobCache : IDistributedCache
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly IBlobClient blobClient;
        private readonly CacheSettings cacheSettings;
        private readonly ILogger<BlobCache> logger;
        private readonly string tempFolder = Path.GetTempPath();

        public BlobCache(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<BlobCache>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            cacheSettings = configuration.GetConfiguredSettings<CacheSettings>();
            blobClient = new BlobClient(configuration, loggerFactory,
                new OptionsWrapper<BlobStorageSettings>(cacheSettings.BlobCache));
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            logger.LogInformation($"get blob cache: key={key}");
            var tokenInfo = await blobClient.GetBlobInfo(key, token);
            if (tokenInfo == null)
            {
                appTelemetry.RecordMetric("blob-cache-miss", 1, ("key", key));
                return null;
            }

            if (tokenInfo.CreatedOn.Add(cacheSettings.TimeToLive) < DateTimeOffset.UtcNow)
            {
                appTelemetry.RecordMetric("blob-cache-expired", 1, ("key", key));
                return null;
            }

            await blobClient.DownloadAsync(null, key, tempFolder, token);
            var downloadedBlogFile = Path.Combine(tempFolder, key);
            if (!File.Exists(downloadedBlogFile))
                throw new InvalidOperationException($"blob download file not found: {downloadedBlogFile}");

            var bytes = await File.ReadAllBytesAsync(downloadedBlogFile, token);
            File.Delete(downloadedBlogFile);
            return bytes;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            await blobClient.Upsert(key, value, token);
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

        public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            await blobClient.Delete(key, token);
        }
    }
}