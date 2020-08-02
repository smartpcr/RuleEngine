// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphDbClient.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.GraphDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Config;
    using KeyVault;
    using Microsoft.Azure.CosmosDB.BulkExecutor;
    using Microsoft.Azure.CosmosDB.BulkExecutor.Graph;
    using Microsoft.Azure.CosmosDB.BulkExecutor.Graph.Element;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class GraphDbClient<V, E> : IGraphDbClient<V, E>
       where V : class, IGremlinVertex, new()
       where E : class, IGremlinEdge, new()
    {
        private readonly ILogger<GraphDbClient<V, E>> logger;
        private IBulkExecutor bulkExecutor;
        private readonly List<PropertyInfo> vertexProps;

        public DocumentCollection Collection { get; }
        public DocumentClient Client { get; }

        public GraphDbClient(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<GraphDbSettings> cosmosDbSettings)
        {
            logger = loggerFactory.CreateLogger<GraphDbClient<V, E>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var settings = cosmosDbSettings.Value ?? configuration.GetConfiguredSettings<GraphDbSettings>();

            var kvClient = serviceProvider.GetRequiredService<IKeyVaultClient>();
            var vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
            logger.LogInformation(
                $"Retrieving auth key '{settings.AuthKeySecret}' from vault '{vaultSettings.VaultName}'");
            var authKey = kvClient.GetSecretAsync(
                vaultSettings.VaultUrl,
                settings.AuthKeySecret).GetAwaiter().GetResult();
            Client = new DocumentClient(
                settings.AccountUri,
                authKey.Value,
                desiredConsistencyLevel: ConsistencyLevel.Session,
                serializerSettings: new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            var database = Client.CreateDatabaseQuery().Where(db => db.Id == settings.Db).AsEnumerable().First();
            Collection = Client.CreateDocumentCollectionQuery(database.SelfLink)
                .Where(c => c.Id == settings.Collection).AsEnumerable().First();
            logger.LogInformation($"Connected to graph db '{Collection.SelfLink}'");

            var vertexInstance = Activator.CreateInstance<V>();
            vertexProps = vertexInstance.GetFlattenedProperties();
        }

        public Task<IEnumerable<V>> Query(V fromVertex, string query, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<E>> GetPath(V fromVertex, V toVertex, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        public async Task BulkInsertVertices(IEnumerable<V> vertices, string partition, CancellationToken cancel)
        {
            await InitBulkExecutor();
            var objs = vertices.Select(ToVertex).ToList();
            logger.LogInformation($"Adding vertices to partition {partition}...{objs.Count}");
            int pos = 0;
            int totalInserted = 0;
            while (pos < objs.Count)
            {
                var batch = objs.Skip(pos).Take(100);
                var retryCount = 0;
                var succeed = false;
                while (!succeed && retryCount < 3)
                {
                    try
                    {
                        await bulkExecutor.BulkImportAsync(batch, true, true, null, null, cancel);
                        succeed = true;
                    }
                    catch (DocumentClientException ex) when (ex.StatusCode == (HttpStatusCode)429)
                    {
                        retryCount++;
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }

                totalInserted += batch.Count();
                logger.LogInformation($"executing batch...{totalInserted}");
                pos += batch.Count();

            }
        }

        public async Task BulkInsertEdges(IEnumerable<E> edges, string partition, CancellationToken cancel)
        {
            await InitBulkExecutor();
            var objs = edges.Select(ToEdge).ToList();
            logger.LogInformation($"Adding edges to partition {partition}...{objs.Count}");
            await bulkExecutor.BulkImportAsync(objs, true, true, null, null, cancel);
            int pos = 0;
            int totalInserted = 0;
            while (pos < objs.Count)
            {
                var batch = objs.Skip(pos).Take(100);
                var retryCount = 0;
                var succeed = false;
                while (!succeed && retryCount < 3)
                {
                    try
                    {
                        await bulkExecutor.BulkImportAsync(batch, true, true, null, null, cancel);
                        succeed = true;
                    }
                    catch (DocumentClientException ex) when (ex.StatusCode == (HttpStatusCode)429)
                    {
                        retryCount++;
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                totalInserted += batch.Count();
                logger.LogInformation($"executing batch...{totalInserted}");
                pos += batch.Count();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        private GremlinVertex ToVertex(V v)
        {
            var vertex = new GremlinVertex(v.Id, v.Label);
            // make sure partition key is added, i.e. if partition key is dcName, vertex.AddProperty("dcName", "***");
            var propValues = v.GetPropertyValues(vertexProps);
            if (propValues != null && propValues.Count > 0)
            {
                foreach (var key in propValues.Keys)
                {
                    vertex.AddProperty(key, propValues[key]);
                }
            }

            return vertex;
        }

        private GremlinEdge ToEdge(E e)
        {
            var outV = e.GetOutVertex();
            var inV = e.GetInVertex();

            var edge = new GremlinEdge(
                e.GetId(),
                e.GetLabel(),
                e.GetOutVertexId(),
                e.GetInVertexId(),
                outV.Label,
                inV.Label,
                outV.PartitionKey,
                inV.PartitionKey);

            return edge;
        }

        private async Task InitBulkExecutor()
        {
            Client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            Client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;
            bulkExecutor = new GraphBulkExecutor(Client, Collection);
            await bulkExecutor.InitializeAsync();

            Client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            Client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}