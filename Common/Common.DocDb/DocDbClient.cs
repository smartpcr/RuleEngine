// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentDbClient.cs" company="Microsoft">
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
    using Config;
    using EnsureThat;
    using KeyVault;
    using Microsoft.Azure.CosmosDB.BulkExecutor;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public sealed class DocDbClient : IDocDbClient
    {
        private readonly FeedOptions _feedOptions;
        private readonly ILogger<DocDbClient> _logger;
        private readonly DocDbSettings _settings;

        public DocDbClient(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IOptions<DocDbSettings> docDbSettings)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _settings = docDbSettings.Value ?? configuration.GetConfiguredSettings<DocDbSettings>();
            _logger = loggerFactory.CreateLogger<DocDbClient>();
            var vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
            var kvClient = serviceProvider.GetRequiredService<IKeyVaultClient>();

            _logger.LogInformation(
                $"Retrieving auth key '{_settings.AuthKeySecret}' from vault '{vaultSettings.VaultName}'");
            var authKey = kvClient.GetSecretAsync(
                vaultSettings.VaultUrl,
                _settings.AuthKeySecret).GetAwaiter().GetResult();
            Client = new DocumentClient(
                _settings.AccountUri,
                authKey.Value,
                desiredConsistencyLevel: ConsistencyLevel.Session,
                serializerSettings: new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            var database = Client.CreateDatabaseQuery().Where(db => db.Id == _settings.Db).AsEnumerable().First();
            Collection = Client.CreateDocumentCollectionQuery(database.SelfLink)
                .Where(c => c.Id == _settings.Collection).AsEnumerable().First();
            _feedOptions = new FeedOptions
            {
                PopulateQueryMetrics = _settings.CollectMetrics,
                EnableCrossPartitionQuery = true
            };

            _logger.LogInformation($"Connected to doc db '{Collection.SelfLink}'");
        }

        public DocumentCollection Collection { get; private set; }
        public DocumentClient Client { get; }

        public async Task SwitchCollection(string collectionName, params string[] partitionKeyPaths)
        {
            if (Collection.Id == collectionName) return;

            var database = Client.CreateDatabaseQuery().Where(db => db.Id == _settings.Db).AsEnumerable().First();
            Collection = Client.CreateDocumentCollectionQuery(database.SelfLink).Where(c => c.Id == collectionName)
                .AsEnumerable().FirstOrDefault();
            if (Collection == null)
            {
                var partition = new PartitionKeyDefinition();
                if (partitionKeyPaths?.Any() == true)
                    foreach (var keyPath in partitionKeyPaths)
                        partition.Paths.Add(keyPath);
                else
                    partition.Paths.Add("/id");

                try
                {
                    await Client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection
                    {
                        Id = collectionName,
                        PartitionKey = partition
                    }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create collection");
                    throw;
                }

                Collection = Client.CreateDocumentCollectionQuery(database.SelfLink).Where(c => c.Id == collectionName)
                    .AsEnumerable().FirstOrDefault();
                _logger.LogInformation("Created collection {0} in {1}/{2}", collectionName, _settings.Account,
                    _settings.Db);
            }

            _logger.LogInformation("Switched to collection {0}", collectionName);
        }

        public async Task<int> CountAsync(string whereClause, CancellationToken cancel = default)
        {
            var countQuery = @"SELECT VALUE COUNT(1) FROM c";
            if (!string.IsNullOrEmpty(whereClause)) countQuery += $" where {whereClause}";

            var result = Client.CreateDocumentQuery<int>(
                Collection.DocumentsLink,
                new SqlQuerySpec
                {
                    QueryText = countQuery,
                    Parameters = new SqlParameterCollection()
                },
                new FeedOptions
                {
                    EnableCrossPartitionQuery = true
                }).AsDocumentQuery();

            var count = 0;
            while (result.HasMoreResults)
            {
                var batchSize = await result.ExecuteNextAsync<int>(cancel);
                count += batchSize.First();
            }

            return count;
        }

        public async Task DeleteObject(string id, CancellationToken cancel = default)
        {
            Ensure.That(id).IsNotNullOrWhiteSpace();

            try
            {
                if (Collection.PartitionKey?.Paths?.Any() == true)
                {
                    var doc = await ReadObject<Document>(id, cancel);
                    if (doc != null)
                        await Client.DeleteDocumentAsync(doc.SelfLink, GetCrossPartitionOptions(doc), cancel);
                    else
                        throw new Exception($"Unable to find doucment with id '{id}'");
                }
                else
                {
                    var docUri = UriFactory.CreateDocumentUri(_settings.Db, _settings.Collection, id);
                    await Client.DeleteDocumentAsync(docUri, cancellationToken: cancel,
                        options: GetCrossPartitionOptions());
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Unable to Delete document. DatabaseName={_settings.Db}, CollectionName={_settings.Collection}, DocumentId={id}, Exception={e.Message}");
                throw;
            }
        }

        public async Task<int> DeleteByQuery(string query, CancellationToken cancel = default)
        {
            var docQuery = Client
                .CreateDocumentQuery(Collection.SelfLink, new SqlQuerySpec(query),
                    new FeedOptions {EnableCrossPartitionQuery = true})
                .AsDocumentQuery();

            var ids = new List<string>();
            while (docQuery.HasMoreResults)
            {
                var response = await docQuery.ExecuteNextAsync<Document>(cancel);
                ids.AddRange(response.Select(d => d.Id));
            }

            foreach (var id in ids) await DeleteObject(id, cancel);

            return ids.Count;
        }

        public async Task<int> UpsertObjects(List<JObject> list, CancellationToken cancel = default)
        {
            Client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            Client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;
            IBulkExecutor bulkExecutor = new BulkExecutor(Client, Collection);
            await bulkExecutor.InitializeAsync();
            Client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            Client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;

            var response = await bulkExecutor.BulkImportAsync(
                list,
                true,
                false,
                cancellationToken: cancel);

            _logger.LogTrace($"Wrote {response.NumberOfDocumentsImported} documents");

            _logger.LogTrace(
                $"Total of {response.NumberOfDocumentsImported} documents written to {Collection.Id}.");

            return (int) response.NumberOfDocumentsImported;
        }

        public async Task<IEnumerable<T>> Query<T>(SqlQuerySpec querySpec, FeedOptions feedOptions = null,
            CancellationToken cancel = default)
        {
            Ensure.That(querySpec).IsNotNull();

            try
            {
                var output = new List<T>();
                feedOptions ??= _feedOptions;
                var query = Client
                    .CreateDocumentQuery<T>(Collection.SelfLink, querySpec, feedOptions)
                    .AsDocumentQuery();

                while (query.HasMoreResults)
                {
                    var response = await query.ExecuteNextAsync<T>(cancel);
                    output.AddRange(response);

                    // if (_settings.CollectMetrics)
                    // {
                    //     var queryMetrics = response.QueryMetrics;
                    //     foreach (var label in queryMetrics.Keys)
                    //     {
                    //         var queryMetric = queryMetrics[label];
                    //     }
                    // }
                }
                //_logger.LogInformation("Total RU for {0}: {1}", nameof(Query), totalRequestUnits);

                return output;
            }
            catch (DocumentClientException e)
            {
                _logger.LogError(
                    e,
                    $"Unable to Query. DatabaseName={_settings.Db}, CollectionName={_settings.Collection}, Query={querySpec}, FeedOptions={feedOptions}");

                throw;
            }
        }

        public async Task<FeedResponse<T>> QueryInBatches<T>(SqlQuerySpec querySpec, FeedOptions feedOptions = null,
            CancellationToken cancel = default)
        {
            Ensure.That(querySpec).IsNotNull();

            try
            {
                feedOptions = feedOptions ?? _feedOptions;
                var query = Client
                    .CreateDocumentQuery<T>(Collection.SelfLink, querySpec, feedOptions)
                    .AsDocumentQuery();

                if (query.HasMoreResults)
                {
                    var response = await query.ExecuteNextAsync<T>(cancel);
                    return response;
                }

                return null;
            }
            catch (DocumentClientException e)
            {
                _logger.LogError(
                    e,
                    $"Unable to Query. DatabaseName={_settings.Db}, CollectionName={_settings.Collection}, Query={querySpec}, FeedOptions={feedOptions}");

                throw;
            }
        }

        public async Task<T> ReadObject<T>(string id, CancellationToken cancel = default)
        {
            try
            {
                if (Collection.PartitionKey?.Paths?.Any() == true)
                {
                    var query = Client.CreateDocumentQuery<Document>(Collection.SelfLink,
                            new FeedOptions {EnableCrossPartitionQuery = true})
                        .Where(d => d.Id == id).AsDocumentQuery();
                    if (query.HasMoreResults)
                    {
                        var docs = await query.ExecuteNextAsync<T>(cancel);
                        return docs.FirstOrDefault();
                    }

                    return default;
                }

                var docUri = UriFactory.CreateDocumentUri(_settings.Db, _settings.Collection, id);
                var response = await Client.ReadDocumentAsync<T>(docUri, GetCrossPartitionOptions(), cancel);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Unable to Read document. DatabaseName={0}, CollectionName={1}, DocumentId={2}, Exception={3}",
                    _settings.Db, _settings.Collection, id, e.Message);
                throw;
            }
        }

        public async Task<string> UpsertObject<T>(T @object, RequestOptions requestOptions = null,
            CancellationToken cancel = default)
        {
            try
            {
                var response = await Client.UpsertDocumentAsync(Collection.SelfLink, @object, requestOptions,
                    cancellationToken: cancel);

                return response.Resource.Id;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to Upsert object. CollectionUrl={0}",
                    Collection.SelfLink);
                throw;
            }
        }

        public async Task<int> ClearAll(CancellationToken cancel = default)
        {
            var feedOptions = new FeedOptions {EnableCrossPartitionQuery = true};
            var bulkDeleteSp = Client.CreateStoredProcedureQuery(Collection.SelfLink)
                .AsEnumerable().FirstOrDefault(sp => sp.Id == "bulkDelete");
            if (bulkDeleteSp == null)
                throw new Exception("stored procedure with name 'bulkDelete' need to be deployed");

            var totalDeleted = 0;

            try
            {
                // check if partition key exists, assumes single partition key
                var partitionKey = Collection.PartitionKey?.Paths.FirstOrDefault();
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    partitionKey = partitionKey.TrimStart(new[] {'/'}).Trim();
                    _logger.LogInformation($"collection has partition key: {partitionKey}");

                    var getPartitionKeysQuery = Client.CreateDocumentQuery<string>(
                            Collection.SelfLink,
                            new SqlQuerySpec($"select distinct value(c.{partitionKey}) from c"),
                            feedOptions)
                        .AsDocumentQuery();
                    var partitionKeyValues = new List<string>();
                    while (getPartitionKeysQuery.HasMoreResults)
                    {
                        var partitionKeyResponse = await getPartitionKeysQuery.ExecuteNextAsync<string>(cancel);
                        partitionKeyValues.AddRange(partitionKeyResponse);
                    }

                    _logger.LogInformation(
                        $"total of {partitionKeyValues.Count} partition key values found in collection");

                    foreach (var partitionKeyValue in partitionKeyValues)
                    {
                        _logger.LogInformation($"deleting documents from partition: {partitionKeyValue}");

                        var response = await Client.ExecuteStoredProcedureAsync<string>(
                            bulkDeleteSp.SelfLink,
                            new RequestOptions {PartitionKey = new PartitionKey(partitionKeyValue)},
                            cancel,
                            new {query = $"select * from c where c.{partitionKey} = '{partitionKeyValue}'"});
                        var jsonObj = JObject.Parse(response);
                        var deleted = jsonObj.Value<int>("deleted");
                        totalDeleted += deleted;
                        var continueation = jsonObj.Value<bool>("continuation");
                        while (continueation)
                        {
                            _logger.LogInformation($"deleting...{totalDeleted}");

                            response = await Client.ExecuteStoredProcedureAsync<string>(
                                bulkDeleteSp.SelfLink,
                                new RequestOptions {PartitionKey = new PartitionKey(partitionKeyValue)},
                                cancel,
                                new {query = $"select * from c where c.{partitionKey} = '{partitionKeyValue}'"});
                            jsonObj = JObject.Parse(response);
                            deleted = jsonObj.Value<int>("deleted");
                            totalDeleted += deleted;
                            continueation = jsonObj.Value<bool>("continuation");
                        }
                    }

                    // delete records with null partition value
                    try
                    {
                        var response = await Client.ExecuteStoredProcedureAsync<string>(
                            bulkDeleteSp.SelfLink,
                            new RequestOptions {PartitionKey = new PartitionKey(Undefined.Value) },
                            cancel,
                            new {query = $"select * from c"});
                        var jsonObj = JObject.Parse(response);
                        var deleted = jsonObj.Value<int>("deleted");
                        totalDeleted += deleted;
                        var continueation = jsonObj.Value<bool>("continuation");
                        while (continueation)
                        {
                            _logger.LogInformation($"deleting...{totalDeleted}");

                            response = await Client.ExecuteStoredProcedureAsync<string>(
                                bulkDeleteSp.SelfLink,
                                new RequestOptions {PartitionKey = new PartitionKey(Undefined.Value) },
                                cancel,
                                new {query = $"select * from c"});
                            jsonObj = JObject.Parse(response);
                            deleted = jsonObj.Value<int>("deleted");
                            totalDeleted += deleted;
                            continueation = jsonObj.Value<bool>("continuation");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"failed to clear records with empty partition key value");
                    }
                }
                else
                {
                    var response = await Client.ExecuteStoredProcedureAsync<string>(
                        bulkDeleteSp.SelfLink,
                        new RequestOptions {PartitionKey = new PartitionKey(Undefined.Value)},
                        cancel,
                        new {query = "select * from c"});

                    var jsonObj = JObject.Parse(response);
                    var deleted = jsonObj.Value<int>("deleted");
                    totalDeleted += deleted;
                    var continueation = jsonObj.Value<bool>("continuation");
                    while (continueation)
                    {
                        _logger.LogInformation($"deleting...{totalDeleted}");

                        response = await Client.ExecuteStoredProcedureAsync<string>(
                            bulkDeleteSp.SelfLink,
                            new RequestOptions {PartitionKey = new PartitionKey(Undefined.Value)},
                            cancel);
                        jsonObj = JObject.Parse(response);
                        deleted = jsonObj.Value<int>("deleted");
                        totalDeleted += deleted;
                        continueation = jsonObj.Value<bool>("continuation");
                    }
                }


                _logger.LogInformation($"total of {totalDeleted} records are deleted from collection: {Collection.Id}");
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    $"Unable to clear collection. DatabaseName={_settings.Db}, CollectionName={_settings.Collection}");

                throw;
            }

            return totalDeleted;
        }

        public async Task<T> ExecuteStoredProcedure<T>(string storedProcName, CancellationToken cancel,
            params object[] paramValues)
        {
            var sp = Client.CreateStoredProcedureQuery(Collection.SelfLink)
                .AsEnumerable().FirstOrDefault(storedProc => storedProc.Id == storedProcName);
            if (sp == null) throw new Exception($"stored procedure with name '{storedProcName}' need to be deployed");

            _logger.LogInformation($"Executing stored procedure {storedProcName}...");
            var resp = await Client.ExecuteStoredProcedureAsync<string>(
                sp.SelfLink,
                GetCrossPartitionOptions(),
                cancel,
                paramValues);
            _logger.LogInformation($"results from stored procedure: {resp.Response}");
            return JsonConvert.DeserializeObject<T>(resp.Response);
        }

        public async Task<DateTime> GetLastModificationTime(string query, CancellationToken cancel)
        {
            var sql = $"select top 1 value(c._ts) from c order by c._ts desc";
            if (!string.IsNullOrEmpty(query))
                sql = $"select top 1 value(c._ts) from c where {query} order by c._ts desc";

            var timestamps = await Query<long>(
                new SqlQuerySpec(sql),
                new FeedOptions {EnableCrossPartitionQuery = true},
                cancel);
            var ts = timestamps.FirstOrDefault();
            var timestamp = ts != 0 ? DateTimeOffset.FromUnixTimeSeconds(ts) : default;
            _logger.LogInformation($"last modification time: {timestamp}");

            return timestamp.DateTime;
        }

        public async Task<IEnumerable<string>> GetCountsByField(string fieldName, string query = null, CancellationToken cancel = default)
        {
            var sql = $"select distinct c.{fieldName} from c";
            if (!string.IsNullOrEmpty(query))
            {
                sql = $"select distinct c.{fieldName} from c where {query}";
            }
            _logger.LogInformation(sql);

            var docQuery = Client.CreateDocumentQuery(
                Collection.DocumentsLink,
                new SqlQuerySpec
                {
                    QueryText = sql,
                    Parameters = new SqlParameterCollection()
                },
                new FeedOptions()
                {
                    EnableCrossPartitionQuery = true

                }).AsDocumentQuery();

            var result = new List<string>();
            while (docQuery.HasMoreResults)
            {
                var docs = await docQuery.ExecuteNextAsync(cancel);

                foreach (var doc in docs)
                {
                    var token = JToken.Parse(doc.ToString());
                    result.Add(token.Value<string>(fieldName));
                }
            }

            return result;
        }

        public async Task<long> CountBy(string fieldName, string query = null, CancellationToken cancel = default)
        {
            var usePartitionCount = false;
            var partitionKey = Collection.PartitionKey?.Paths.FirstOrDefault();
            if (!string.IsNullOrEmpty(partitionKey))
            {
                partitionKey = partitionKey.TrimStart(new[] {'/'}).Trim();
                if (partitionKey == fieldName)
                {
                    usePartitionCount = true;
                }
            }

            if (usePartitionCount)
            {
                var partitionKeyValues = await GetPartitionKeyValues(cancel);
                return partitionKeyValues.Count;
            }
            else
            {
                var uniqueFields = await GetCountsByField(fieldName, query, cancel);
                return uniqueFields.LongCount();
            }
        }

        public async Task<Dictionary<string, string>> GetIdMappings(string fieldName, CancellationToken cancel)
        {
            var sql = $"select distinct c.id, c.{fieldName} from c";
            _logger.LogInformation(sql);

            var docQuery = Client.CreateDocumentQuery(
                Collection.DocumentsLink,
                new SqlQuerySpec
                {
                    QueryText = sql,
                    Parameters = new SqlParameterCollection()
                },
                new FeedOptions()
                {
                    EnableCrossPartitionQuery = true

                }).AsDocumentQuery();

            var result = new Dictionary<string, string>();
            while (docQuery.HasMoreResults)
            {
                var docs = await docQuery.ExecuteNextAsync(cancel);

                foreach (var doc in docs)
                {
                    var token = JToken.Parse(doc.ToString());
                    var id = token.Value<string>("id");
                    var value = token.Value<string>(fieldName);
                    if (value == null || id == null) continue;
                    if (!result.ContainsKey(value))
                    {
                        result.Add(value, id);
                    }
                }
            }

            return result;
        }

        public async Task<T> ExecuteScalar<T>(string query, string fieldName, CancellationToken cancel)
        {
            _logger.LogInformation(query);
            var docQuery = Client.CreateDocumentQuery(
                Collection.DocumentsLink,
                new SqlQuerySpec
                {
                    QueryText = query,
                    Parameters = new SqlParameterCollection()
                },
                new FeedOptions()
                {
                    EnableCrossPartitionQuery = true
                }).AsDocumentQuery();

            if (docQuery.HasMoreResults)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    var values = await docQuery.ExecuteNextAsync<T>(cancel);
                    return values.FirstOrDefault();
                }

                var docs = await docQuery.ExecuteNextAsync(cancel);
                var doc = docs.FirstOrDefault();
                if (doc != null)
                {
                    var token = JToken.Parse(doc.ToString());
                    return token.Value<T>(fieldName);
                }
            }

            return default(T);
        }

        public async Task ExecuteQuery(
            Type entityType,
            string query,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            int batchSize = 100,
            CancellationToken cancel = default)
        {
            try
            {
                var docQuery = Client
                    .CreateDocumentQuery(Collection.SelfLink, new SqlQuerySpec(query), _feedOptions)
                    .AsDocumentQuery();
                var output = new List<object>();

                while (docQuery.HasMoreResults)
                {
                    var response = await docQuery.ExecuteNextAsync(cancel);
                    output.AddRange(response);
                    if (output.Count >= batchSize)
                    {
                        await onBatchReceived(output, cancel);
                        output= new List<object>();
                    }
                }

                if (output.Count > 0)
                {
                    await onBatchReceived(output, cancel);
                    output.Clear();
                }
            }
            catch (DocumentClientException e)
            {
                _logger.LogError(
                    e,
                    $"Unable to Query. DatabaseName={_settings.Db}, CollectionName={_settings.Collection}, Query={query}");

                throw;
            }
        }

        public async Task ExecuteQuery<T>(string query, Func<IList<T>, CancellationToken, Task> onBatchReceived, int batchSize = 100, CancellationToken cancel = default)
        {
            try
            {
                var docQuery = Client
                    .CreateDocumentQuery<T>(Collection.SelfLink, new SqlQuerySpec(query), _feedOptions)
                    .AsDocumentQuery();
                var output = new List<T>();

                while (docQuery.HasMoreResults)
                {
                    var response = await docQuery.ExecuteNextAsync<T>(cancel);
                    output.AddRange(response);
                    if (output.Count >= batchSize)
                    {
                        await onBatchReceived(output, cancel);
                        output= new List<T>();
                    }
                }

                if (output.Count > 0)
                {
                    await onBatchReceived(output, cancel);
                    output.Clear();
                }
            }
            catch (DocumentClientException e)
            {
                _logger.LogError(
                    e,
                    $"Unable to Query. DatabaseName={_settings.Db}, CollectionName={_settings.Collection}, Query={query}");

                throw;
            }
        }

        public IQueryable<T> GetDocuments<T>()
        {
            var docQuery = Client.CreateDocumentQuery<T>(
                Collection.DocumentsLink,
                new FeedOptions()
                {
                    EnableCrossPartitionQuery = true
                });
            return docQuery;
        }

        private RequestOptions GetCrossPartitionOptions(Document doc = null)
        {
            if (doc != null && Collection.PartitionKey?.Paths.Any() == true)
            {
                var partitionKeyField = Collection.PartitionKey.Paths.First().TrimStart(new[] {'/'}).Trim();
                var partitionKeyValue = doc.GetPropertyValue<string>(partitionKeyField);
                if (partitionKeyValue != null)
                    return new RequestOptions
                    {
                        PartitionKey = new PartitionKey(partitionKeyValue)
                    };
            }

            return new RequestOptions
            {
                PartitionKey = PartitionKey.None
            };
        }

        private async Task<List<string>> GetPartitionKeyValues(CancellationToken cancel)
        {
            var partitionKeyValues = new List<string>();
            var feedOptions = new FeedOptions { EnableCrossPartitionQuery = true };
            var partitionKey = Collection.PartitionKey?.Paths.FirstOrDefault();
            if (!string.IsNullOrEmpty(partitionKey))
            {
                partitionKey = partitionKey.TrimStart(new[] {'/'}).Trim();
                _logger.LogInformation($"collection has partition key: {partitionKey}");

                var getPartitionKeysQuery = Client.CreateDocumentQuery<string>(
                        Collection.SelfLink,
                        new SqlQuerySpec($"select distinct value(c.{partitionKey}) from c"),
                        feedOptions)
                    .AsDocumentQuery();
                
                while (getPartitionKeysQuery.HasMoreResults)
                {
                    var partitionKeyResponse = await getPartitionKeysQuery.ExecuteNextAsync<string>(cancel);
                    partitionKeyValues.AddRange(partitionKeyResponse);
                }

                _logger.LogInformation(
                    $"total of {partitionKeyValues.Count} partition key values found in collection");
            }

            return partitionKeyValues;
        }

        #region IDisposable Support

        private bool isDisposed; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing) Client.Dispose();

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DocDb() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}