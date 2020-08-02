//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="IKustoRepo.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Kusto;

    public interface IKustoRepo<T> where T : class, new()
    {
        Task<IList<T>> Query([NotNull]string query, CancellationToken cancellationToken);
        Task<IList<TRecord>> ExecuteQuery<TRecord>([NotNull]string query, Func<IDataReader, TRecord> read, CancellationToken cancel);
        Task<DateTime> GetLastModificationTime([NotNull]string query, CancellationToken cancel);
        Task<int> Ingest(List<T> payload, IngestMode ingestMode, CancellationToken cancel);
    }
}