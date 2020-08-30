// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Config;
    using Common.DocDb;
    using DataCenterHealth.Repositories;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class DocDbRepository<T>: IDocDbRepository<T>
        where T : BaseEntity, new()
    {
        private readonly IServiceProvider serviceProvider;
        private readonly EntityStoreSettings _dataSettings;
        private readonly IDocDbClient _docDbClient;
        private readonly JsonSerializer _jsonSerializer;
        private readonly ILogger<DocDbRepository<T>> _logger;

        public DocDbRepository(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            _logger = loggerFactory.CreateLogger<DocDbRepository<T>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var docDbSettings = configuration.GetConfiguredSettings<DocDbSettings>();
            var backendSettings = configuration.GetConfiguredSettings<BackendData>();
            var prop = backendSettings.GetType().GetProperties()
                .FirstOrDefault(p =>
                {
                    var customAttr = p.GetCustomAttribute<MappedModelAttribute>();
                    if (customAttr != null && customAttr.ModelType == typeof(T)) return true;

                    return false;
                });
            if (prop == null) throw new Exception($"Missing backend mapping for model: {typeof(T).Name}");

            _dataSettings = prop.GetValue(backendSettings) as EntityStoreSettings;
            if (_dataSettings == null)
            {
                throw new Exception($"Missing backend mapping for model: {typeof(T).Name}");
            }

            docDbSettings.Db = _dataSettings.Db;
            docDbSettings.Collection = _dataSettings.Collection;
            _docDbClient = new DocDbClient(
                serviceProvider,
                loggerFactory,
                new OptionsWrapper<DocDbSettings>(docDbSettings));

            _jsonSerializer = new JsonSerializer();
            _jsonSerializer.Converters.Add(new StringEnumConverter());
            _jsonSerializer.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };
        }

        public IDocDbClient Client => _docDbClient;

        public async Task<IEnumerable<T>> GetAll()
        {
            var query = _dataSettings.BuildQuery();
            return await ExecuteQuery(query);
        }

        public async Task<int> Count(string whereSql)
        {
            var total = await _docDbClient.CountAsync(whereSql);
            return total;
        }

        public async Task<IEnumerable<T>> Query(string whereSql)
        {
            var query = _dataSettings.BuildQuery(whereSql);
            return await ExecuteQuery(query);
        }

        public async Task<IEnumerable<T>> Query(string whereSqlTemplate, IList<string> ids)
        {
            var pos = 0;
            const int batchSize = 100;
            var output = new List<T>();
            while (pos < ids.Count)
            {
                var batchedIds = ids.Skip(pos).Take(batchSize).ToList();
                var idQuery = string.Join(",", batchedIds.Select(id => "\"" + id + "\""));
                var query = string.Format(whereSqlTemplate, idQuery);
                var batchedResult = await Query(query);
                output.AddRange(batchedResult);
                pos += batchedIds.Count;
            }

            return output;
        }

        public async Task<PagedResult<T>> QueryPaged(string whereSql, string orderByField, bool isDescending = false,
            int skip = 0, int take = 10)
        {
            var query = _dataSettings.BuildQuery(whereSql, orderByField, isDescending, skip, take);
            var items = await ExecuteQuery(query);
            var total = await _docDbClient.CountAsync(whereSql);
            return new PagedResult<T>
            {
                Total = total,
                Items = items.ToList()
            };
        }

        public async Task<T> GetById(string id)
        {
            var query = _dataSettings.BuildQuery("c.id=@id");
            _logger.LogInformation($"getting entity {typeof(T).Name} by @id: {id} and query '{query}'...");
            var results = await _docDbClient.Query<T>(
                new SqlQuerySpec(query, new SqlParameterCollection
                {
                    new SqlParameter("@id", id)
                }),
                new FeedOptions {EnableCrossPartitionQuery = true});
            return results?.FirstOrDefault();
        }

        public async Task<T> Create(T newInstance)
        {
            _logger.LogInformation($"creating new entity: {typeof(T).Name}...");
            var result = await _docDbClient.UpsertObject(newInstance);
            newInstance.Id = result;
            _logger.LogInformation($"created new entity: {typeof(T).Name}, id={newInstance.Id}.");
            return newInstance;
        }

        public async Task Update(T instance)
        {
            _logger.LogInformation($"updating entity: {typeof(T).Name}");
            await _docDbClient.UpsertObject(instance);
        }

        public async Task Delete(string id)
        {
            _logger.LogInformation($"deleting entity: {typeof(T).Name}, id={id}.");
            await _docDbClient.DeleteObject(id);
        }

        public async Task<int> DeleteByQuery(string query)
        {
            _logger.LogInformation($"deleting entity: {typeof(T).Name}, query={query}.");
            return await _docDbClient.DeleteByQuery(query);
        }

        public async Task BulkUpsert(IList<T> batch, CancellationToken cancellationToken)
        {
            _logger.LogTrace($"bulk upsert {batch.Count} documents...");
            // bulk executor don't follow json converter settings without customized serilizer
            var docs = batch.Select(e => JObject.FromObject(e, _jsonSerializer)).ToList();
            await _docDbClient.UpsertObjects(docs, cancellationToken);
        }

        public async Task<IEnumerable<T>> ObtainLease(CancellationToken cancel)
        {
            _logger.LogInformation("trying to obtain lease...");
            var leasedDocs = (await _docDbClient.ExecuteStoredProcedure<IEnumerable<T>>("acquireLease", cancel)).ToList();
            _logger.LogInformation($"obtained {leasedDocs.Count} leased docs");
            return leasedDocs;
        }

        public async Task ReleaseLease(string id, CancellationToken cancel)
        {
            _logger.LogInformation($"releasing lease to doc {id}");
            var releasedDoc = await _docDbClient.ExecuteStoredProcedure<ReleasedDoc>("releaseLease", cancel);
            _logger.LogInformation($"lease on doc {releasedDoc.Id} is released");
        }

        public async Task<DateTime> GetLastModificationTime(string query, CancellationToken cancel)
        {
            _logger.LogInformation($"get last modification time: {query}");
            var timestamp = await _docDbClient.GetLastModificationTime(query, cancel);
            return timestamp;
        }

        public async Task<IEnumerable<string>> GetCountsByField(string fieldName, string query = null, CancellationToken cancel = default)
        {
            _logger.LogInformation($"executing count by {fieldName} with filter {query}");
            var countsByField = (await _docDbClient.GetCountsByField(fieldName, query, cancel)).ToList();
            _logger.LogInformation($"total of {countsByField.Count} distinct values found for {fieldName}");
            return countsByField;
        }

        private async Task<IEnumerable<T>> ExecuteQuery(string query)
        {
            _logger.LogInformation($"query all entities for {typeof(T).Name} with query '{query}'..");
            return await _docDbClient.Query<T>(
                new SqlQuerySpec(query),
                new FeedOptions {EnableCrossPartitionQuery = true});
        }

        class ReleasedDoc
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("released")]
            public bool Released { get; set; }
        }
    }
}