// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheExtensions.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using Microsoft.Extensions.Caching.Distributed;

    public static class CacheExtensions
    {
        public static DistributedCacheEntryOptions PatchOptions(this DistributedCacheEntryOptions entry,
            DistributedCacheEntryOptions options)
        {
            if (entry != null && options != null)
            {
                entry.AbsoluteExpiration = options.AbsoluteExpiration ?? entry.AbsoluteExpiration;
                entry.AbsoluteExpirationRelativeToNow =
                    options.AbsoluteExpirationRelativeToNow ?? entry.AbsoluteExpirationRelativeToNow;
                entry.SlidingExpiration = options.SlidingExpiration ?? entry.SlidingExpiration;
            }

            return entry;
        }
    }
}