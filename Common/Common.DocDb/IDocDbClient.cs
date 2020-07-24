// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDocumentDbClient.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.DocDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///     Represents a client to interact with a specific collection in a specific DocumentDb store
    /// </summary>
    public interface IDocDbClient : IDisposable
    {
        DocumentCollection Collection { get; }
        DocumentClient Client { get; }

        /// <summary>
        ///     switch context to different collection, create new partition if not exist
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="partitionKeyPaths"></param>
        /// <returns></returns>
        Task SwitchCollection(string collectionName, params string[] partitionKeyPaths);

        /// <summary>
        ///     count records across partitions
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync(string whereClause, CancellationToken cancel = default);

        /// <summary>
        ///     Update (if exists) or insert (if it doesn't exist) an object to the store.
        ///     New objects will automatically receive a system-generated id.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="@object">The object being stored</param>
        /// <param name="object"></param>
        /// <param name="requestOptions"></param>
        /// <param name="cancel"></param>
        /// <returns>The system generated id for this object</returns>
        /// <exception cref="ArgumentNullException">if @object is null</exception>
        Task<string> UpsertObject<T>(T @object, RequestOptions requestOptions = null,
            CancellationToken cancel = default);

        /// <summary>
        ///     using bulk executor to upsert list of objects
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<int> UpsertObjects(List<JObject> list, CancellationToken cancel = default);

        /// <summary>
        ///     Execute a SQL query to retrieve matching strongly-typed objects from the store
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="querySpec"></param>
        /// <param name="feedOptions"></param>
        /// <param name="cancel"></param>
        /// <returns>A collection of stored objects--if no objects match, the collection will be empty</returns>
        /// <exception cref="ArgumentNullException">if querySpec is null</exception>
        Task<IEnumerable<T>> Query<T>(SqlQuerySpec querySpec, FeedOptions feedOptions = null,
            CancellationToken cancel = default);

        /// <summary>
        ///     use continuation token to query in batches, batch size is stored in <see cref="FeedOptions" />
        /// </summary>
        /// <param name="querySpec"></param>
        /// <param name="feedOptions"></param>
        /// <param name="cancel"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<FeedResponse<T>> QueryInBatches<T>(SqlQuerySpec querySpec, FeedOptions feedOptions = null,
            CancellationToken cancel = default);

        /// <summary>
        ///     Delete an object within the store.
        /// </summary>
        /// <param name="id">The identifier of the object in the store</param>
        /// <param name="cancel"></param>
        /// <exception cref="ArgumentException">if id is trivial or null</exception>
        Task DeleteObject(string id, CancellationToken cancel = default);

        /// <summary>
        ///     delete objects returned from query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<int> DeleteByQuery(string query, CancellationToken cancel = default);

        /// <summary>
        ///     Read a strongly-typed object from the store.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="id">The identifier of the object in the store</param>
        /// <param name="cancel"></param>
        /// <returns>
        ///     The stored object. Will throw
        ///     <see>
        ///         <cref>DocumentDbException</cref>
        ///     </see>
        ///     if the document doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">if id is trivial or null</exception>
        Task<T> ReadObject<T>(string id, CancellationToken cancel = default);

        /// <summary>
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<int> ClearAll(CancellationToken cancel = default);

        Task<T> ExecuteStoredProcedure<T>(string storedProcName, CancellationToken cancel, params object[] paramValues);

        Task<DateTime> GetLastModificationTime(string query, CancellationToken cancel);

        Task<IEnumerable<string>> GetCountsByField(string fieldName, string query = null, CancellationToken cancel = default);

        Task<long> CountBy(string fieldName, string query = null, CancellationToken cancel = default);

        Task<Dictionary<string, string>> GetIdMappings(string fieldName, CancellationToken cancel);

        Task<T> ExecuteScalar<T>(string query, string fieldName, CancellationToken cancel);

        Task ExecuteQuery(
            Type entityType,
            string query,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            int batchSize = 100,
            CancellationToken cancel = default);

        Task ExecuteQuery<T>(
            string query,
            Func<IList<T>, CancellationToken, Task> onBatchReceived,
            int batchSize = 100,
            CancellationToken cancel = default);

        IQueryable<T> GetDocuments<T>();
    }
}