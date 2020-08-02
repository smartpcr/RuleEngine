// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICacheProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICacheProvider
    {
        Task<T> GetOrUpdateAsync<T>(
            string key,
            Func<Task<DateTimeOffset>> getLastModificationTime,
            Func<Task<T>> getItem,
            CancellationToken cancel) where T : class, new();

        T GetOrUpdate<T>(
            string key,
            Func<DateTimeOffset> getLastModificationTime,
            Func<T> getItem,
            CancellationToken cancel = default) where T : class, new();

        Task Set<T>(string key, T item, CancellationToken cancel) where T : class, new();

        Task ClearAsync(string key, CancellationToken cancel);

        Task ClearAll(CancellationToken cancel);
    }
}