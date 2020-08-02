// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGraphDbClient.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.GraphDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IGraphDbClient<V, E>: IDisposable
        where V : class, IGremlinVertex, new()
        where E : class, IGremlinEdge, new()
    {
        Task<IEnumerable<V>> Query(V fromVertex, string query, CancellationToken cancel);
        Task<IEnumerable<E>> GetPath(V fromVertex, V toVertex, CancellationToken cancel);
        Task BulkInsertVertices(IEnumerable<V> vertices, string partition, CancellationToken cancel);
        Task BulkInsertEdges(IEnumerable<E> edges, string partition, CancellationToken cancel);
    }
}