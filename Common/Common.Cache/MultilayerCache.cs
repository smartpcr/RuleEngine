// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultilayerCache.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;

    public class MultilayerCache : IDistributedCache
    {
        private readonly IList<(IDistributedCache Cache, DistributedCacheEntryOptions Options)> _caches;

        /// <summary>
        ///     Creates a new <see cref="MultilayerCache" /> instance.
        /// </summary>
        /// <param name="innerLayerCache"></param>
        /// <param name="innerLayerCacheOptions">The options of the cache.</param>
        public MultilayerCache(IDistributedCache innerLayerCache,
            DistributedCacheEntryOptions innerLayerCacheOptions = null)
        {
            if (innerLayerCache == null)
                throw new ArgumentException("Initial cache can not be null.", nameof(innerLayerCache));

            if (innerLayerCacheOptions == null) innerLayerCacheOptions = new DistributedCacheEntryOptions();
            _caches = new List<(IDistributedCache Cache, DistributedCacheEntryOptions Options)>();
            _caches.Add((innerLayerCache, innerLayerCacheOptions));
        }

        /// <summary>
        ///     Gets or sets whether lower layers should be populated on cache hit.
        /// </summary>
        public bool PopulateLayersOnGet { get; set; }

        /// <summary>
        ///     Gets a list of underlying caches.
        /// </summary>
        public IEnumerable<IDistributedCache> Caches => _caches.Select(c => c.Cache);

        public byte[] Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            // TODO: Optimize by using ObjectPool
            var emptyCaches = new List<(IDistributedCache Cache, DistributedCacheEntryOptions Options)>();
            foreach (var layer in _caches)
            {
                var value = await layer.Cache.GetAsync(key, token);
                if (value != null)
                {
                    if (PopulateLayersOnGet && emptyCaches.Any())
                        await Task.WhenAll(emptyCaches.Select(l => l.Cache.SetAsync(key, value, l.Options, token)));

                    return value;
                }

                emptyCaches.Add(layer);
            }

            return null;
        }

        public void Refresh(string key)
        {
            foreach (var layer in _caches) layer.Cache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.WhenAll(_caches.Select(l => l.Cache.RefreshAsync(key, token)));
        }

        public void Remove(string key)
        {
            foreach (var layer in _caches) layer.Cache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.WhenAll(_caches.Select(l => l.Cache.RemoveAsync(key, token)));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            foreach (var layer in _caches) layer.Cache.Set(key, value, layer.Options.PatchOptions(options));
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            return Task.WhenAll(
                _caches.Select(l => l.Cache.SetAsync(key, value, l.Options.PatchOptions(options), token)));
        }

        public MultilayerCache AppendLayer(IDistributedCache cache, DistributedCacheEntryOptions cacheOptions = null)
        {
            if (cache == null) throw new ArgumentException("Cache can not be null.", nameof(cache));

            if (cacheOptions == null) cacheOptions = new DistributedCacheEntryOptions();

            _caches.Add((cache, cacheOptions));
            return this;
        }
    }
}