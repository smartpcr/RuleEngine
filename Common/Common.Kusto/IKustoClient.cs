// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IKustoClient.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IKustoClient : IDisposable
    {
        Task<IEnumerable<T>> ExecuteQuery<T>(string query, TimeSpan timeout = default, CancellationToken cancellationToken = default);

        Task<(int Total, T LastRecord)> ExecuteQuery<T>(
            string query,
            Func<IList<T>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken = default,
            int batchSize = 100);

        Task<(int Total, object LastRecord)> ExecuteQuery(
            Type entityType,
            string query,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken = default,
            int batchSize = 100);

        Task<IEnumerable<T>> ExecuteFunction<T>(string functionName, CancellationToken cancellationToken,
            params (string name, string value)[] parameters);

        Task<IDataReader> ExecuteReader(string query);

        Task ExecuteFunction<T>(
            string functionName,
            (string name, string value)[] parameters,
            Func<IList<T>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken = default,
            int batchSize = 100);

        Task<int> BulkInsert<T>(string tableName, IList<T> items, IngestMode ingestMode, string idPropName, CancellationToken cancellationToken);

        Task<T> ExecuteScalar<T>(string query, string fieldName, CancellationToken cancel);

        Task DropTable(string tableName, CancellationToken cancel);

        #region schema

        Task<IEnumerable<KustoTable>> ListTables();

        Task<IEnumerable<KustoFunction>> ListFunctions();

        #endregion
    }

    public enum IngestMode
    {
        AppendOnly,
        InsertNew,
        Refresh
    }
}