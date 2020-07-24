// // --------------------------------------------------------------------------------------------------------------------
// // <copyright company="Microsoft Corporation">
// //   Copyright (c) 2017 Microsoft Corporation.  All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using Storage;

    public class CacheSettings
    {
        public RedisSettings RedisCache { get; set; }
        public BlobStorageSettings BlobCache { get; set; }
        public FileCacheSettings FileCache { get; set; }
        public MemoryCacheSettings MemoryCache { get; set; }

        /// <summary>
        ///     updated item still triggers cache invalidation within TTL
        /// </summary>
        public TimeSpan TimeToLive { get; set; } = TimeSpan.FromDays(15);
    }

    public class FileCacheSettings
    {
        public string CacheFolder { get; set; } = "cache";
    }

    public class MemoryCacheSettings
    {
        public double CompactionPercentage { get; set; } = 0.1;

        /// <summary>
        ///     max memory size in MB
        /// </summary>
        public int SizeLimit { get; set; } = 1024;
    }
}