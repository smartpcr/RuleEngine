// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KustoClient.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Config;
    using global::Kusto.Data.Common;
    using global::Kusto.Ingest;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class KustoClient : IKustoClient
    {
        private readonly ICslAdminProvider adminClient;
        private readonly IKustoIngestClient ingestClient;
        private readonly KustoSettings kustoSettings;
        private readonly ILogger<KustoClient> logger;
        private readonly ICslQueryProvider queryClient;

        public KustoClient(IServiceProvider serviceProvider, ILoggerFactory loggerFactory,
            KustoSettings kustoSettings = null)
        {
            logger = loggerFactory.CreateLogger<KustoClient>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            this.kustoSettings = kustoSettings ?? configuration.GetConfiguredSettings<KustoSettings>();
            var clientFactory = new KustoClientFactory(serviceProvider, kustoSettings);
            queryClient = clientFactory.QueryQueryClient;
            adminClient = clientFactory.AdminClient;
            ingestClient = clientFactory.IngestClient;
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"kusto query:\n{query}");
            var stopWatch = Stopwatch.StartNew();
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                GetClientRequestProps(timeout));
            var records = Read<T>(reader, cancellationToken);
            stopWatch.Stop();
            logger.LogInformation($"it took {stopWatch.Elapsed} to query {records.Count()} records from kusto");
            return records;
        }

        public async Task<(int Total, T LastRecord)> ExecuteQuery<T>(
            string query,
            Func<IList<T>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken = default,
            int batchSize = 100)
        {
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                GetClientRequestProps());
            return await Read(reader, onBatchReceived, cancellationToken, batchSize);
        }

        public async Task<(int Total, object LastRecord)> ExecuteQuery(
            Type entityType,
            string query,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken = default,
            int batchSize = 100)
        {
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                GetClientRequestProps());
            return await Read(entityType, reader, onBatchReceived, cancellationToken, batchSize);
        }

        public async Task<IEnumerable<T>> ExecuteFunction<T>(
            string functionName,
            CancellationToken cancellationToken,
            params (string name, string value)[] parameters)
        {
            var functionParameters = parameters.Select(p => new KeyValuePair<string, string>(p.name, p.value));
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                functionName,
                GetClientRequestProps());
            return Read<T>(reader, cancellationToken);
        }

        public async Task ExecuteFunction<T>(string functionName, (string name, string value)[] parameters,
            Func<IList<T>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken = default, int batchSize = 100)
        {
            var functionParameters = parameters.Select(p => new KeyValuePair<string, string>(p.name, p.value));
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                functionName,
                GetClientRequestProps());
            await Read(reader, onBatchReceived, cancellationToken, batchSize);
        }

        public async Task<int> BulkInsert<T>(
            string tableName,
            IList<T> items,
            IngestMode ingestMode,
            string idPropName,
            CancellationToken cancellationToken)
        {
            await EnsureTable<T>(tableName);
            var columnMappings = typeof(T).GetKustoColumnMappings();
            var props = new KustoIngestionProperties(kustoSettings.DbName, tableName)
            {
                DropByTags = new List<string> {DateTime.Today.ToString("MM/dd/yyyy")},
                IngestByTags = new List<string> {new Guid().ToString()},
                Format = DataSourceFormat.json,
                JsonMapping = columnMappings.Select(p => p.mapping)
            };

            long totalSize = 0;
            int itemChanged = 0;
            if (ingestMode == IngestMode.InsertNew)
            {
                var upserts = await CheckExistingRecords(tableName, items.ToList(), idPropName);
                await using var memoryStream = new MemoryStream();
                await using var writer = new StreamWriter(memoryStream);
                if (upserts.inserts.Count > 0)
                {
                    itemChanged = upserts.inserts.Count;
                    foreach (var item in upserts.inserts)
                        await writer.WriteLineAsync(JsonConvert.SerializeObject(item));
                    await writer.FlushAsync();
                    totalSize = memoryStream.Length;
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await ingestClient.IngestFromStreamAsync(memoryStream, props);
                }
            }
            else
            {
                if (ingestMode == IngestMode.Refresh)
                {
                    await DropTable(tableName, cancellationToken);
                    await EnsureTable<T>(tableName);
                }

                itemChanged = items.Count;
                await using var memoryStream = new MemoryStream();
                await using var writer = new StreamWriter(memoryStream);
                foreach (var item in items) await writer.WriteLineAsync(JsonConvert.SerializeObject(item));
                await writer.FlushAsync();
                totalSize = memoryStream.Length;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await ingestClient.IngestFromStreamAsync(memoryStream, props);
            }

            logger.LogInformation($"ingested {itemChanged} records to kuso table {tableName}, size: {totalSize} bytes");
            return itemChanged;
        }

        public async Task<T> ExecuteScalar<T>(string query, string fieldName, CancellationToken cancel)
        {
            logger.LogInformation($"kusto query:\n{query}");
            try
            {
                var reader = await queryClient.ExecuteQueryAsync(
                    kustoSettings.DbName,
                    query,
                    GetClientRequestProps());
                if (reader.Read())
                {
                    return reader[fieldName] == DBNull.Value ? default : (T) reader[fieldName];
                }
                reader.Dispose();
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get last ingestion time: {ex.Message}");
                return default;
            }
        }

        public async Task<IDataReader> ExecuteReader(string query)
        {
            logger.LogInformation($"kusto query:\n{query}");
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                GetClientRequestProps());
            return reader;
        }

        public async Task DropTable(string tableName, CancellationToken cancel)
        {
            logger.LogInformation($"dropping kusto table: {tableName}...");
            var showTableCmd = CslCommandGenerator.GenerateTableDropCommand(tableName, true);
            await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showTableCmd);
            logger.LogInformation($"kusto table: {tableName} is dropped");
        }

        #region schema
        public async Task<IEnumerable<KustoTable>> ListTables()
        {
            var showTablesCmd = CslCommandGenerator.GenerateTablesShowCommand();
            var reader = await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showTablesCmd);
            var tableNames = new List<string>();
            while (reader.Read())
            {
                tableNames.Add(reader.GetString(0));
            }
            reader.Close();

            var tables = new List<KustoTable>();
            foreach (var tableName in tableNames)
            {
                logger.LogInformation($"reading schema for table {tableName}");
                var showTblCmd = string.Format(".show table {0} schema as json", tableName);
                reader = await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showTblCmd);
                if (reader.Read())
                {
                    var schemaJson = reader.GetString(1);
                    var schema = JObject.Parse(schemaJson);
                    var columns = new List<KustoColumn>();
                    foreach (var column in schema.Value<JArray>("OrderedColumns"))
                    {
                        var columnName = column.Value<string>("Name");
                        var columnType = Type.GetType(column.Value<string>("Type"));
                        var cslType = column.Value<string>("CslType");
                        columns.Add(new KustoColumn()
                        {
                            Name = columnName,
                            Type = columnType,
                            CslType = cslType
                        });
                    }

                    tables.Add(new KustoTable()
                    {
                        Name = tableName,
                        Columns = columns
                    });
                }
                reader.Close();
            }
            logger.LogInformation($"total of {tables.Count} tables found");

            return tables;
        }

        public async Task<IEnumerable<KustoFunction>> ListFunctions()
        {
            var showFunctionsCmd = CslCommandGenerator.GenerateFunctionsShowCommand();
            var reader = await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showFunctionsCmd);
            var functionNames = new List<string>();
            while (reader.Read())
            {
                functionNames.Add(reader.GetString(0));
            }
            reader.Close();

            var output = new List<KustoFunction>();
            foreach (var funcName in functionNames)
            {
                logger.LogInformation($"reading schema for function {funcName}");
                var showFunctionCmd = CslCommandGenerator.GenerateFunctionShowCommand(funcName);
                reader = await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showFunctionCmd);
                if (reader.Read())
                {
                    var function = new KustoFunction()
                    {
                        Name = reader.GetString(0),
                        Parameters = reader[1] == DBNull.Value ? null : reader.GetString(1),
                        Body = reader[2] == DBNull.Value ? null : reader.GetString(2),
                        Folder = reader[3] == DBNull.Value ? null : reader.GetString(3),
                        DocString = reader[4] == DBNull.Value ? null : reader.GetString(4)
                    };
                    output.Add(function);
                }
                reader.Close();
            }
            logger.LogInformation($"total of {output.Count} functions found");

            return output;
        }
        #endregion

        public void Dispose()
        {
            adminClient?.Dispose();
            queryClient?.Dispose();
        }

        private IEnumerable<T> Read<T>(IDataReader reader, CancellationToken cancellationToken)
        {
            var propMappings = BuildFieldMapping<T>(reader);
            var output = new List<T>();
            var total = 0;

            while (reader.Read() && !cancellationToken.IsCancellationRequested)
            {
                var instance = Create<T>(reader, propMappings);
                output.Add(instance);
                total++;
                if (total % 100 == 0) logger.LogTrace($"reading {total} records from kusto...");
            }

            reader.Dispose();

            logger.LogInformation($"total of {output.Count} records retrieved from kusto");

            return output;
        }

        private async Task<(int Total, T LastRecord)> Read<T>(
            IDataReader reader,
            Func<IList<T>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken,
            int batchSize)
        {
            var propMappings = BuildFieldMapping<T>(reader);

            var output = new List<T>();
            var batchCount = 0;
            var total = 0;
            T lastRecord = default(T);
            while (reader.Read() && !cancellationToken.IsCancellationRequested)
            {
                var instance = Create<T>(reader, propMappings);
                output.Add(instance);
                if (output.Count >= batchSize)
                {
                    batchCount++;
                    total += output.Count;
                    await onBatchReceived(output, cancellationToken);
                    logger.LogInformation($"sending batch #{batchCount}, total: {total} records");
                    lastRecord = output[output.Count - 1];
                    output = new List<T>();
                }
            }

            reader.Dispose();

            if (output.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                batchCount++;
                total += output.Count;
                logger.LogInformation($"sending batch #{batchCount}, count: {total} records");
                await onBatchReceived(output, cancellationToken);
                lastRecord = output[output.Count - 1];
                output.Clear();
            }

            if (cancellationToken.IsCancellationRequested) logger.LogInformation("kusto query is cancelled");

            logger.LogInformation($"total of {output.Count} records retrieved from kusto");

            return (total, lastRecord);
        }

        private async Task<(int Total, object LastRecord)> Read(
            Type entityType,
            IDataReader reader,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            CancellationToken cancellationToken,
            int batchSize)
        {
            var propMappings = BuildFieldMapping(entityType, reader);

            var output = new List<object>();
            int batchCount = 0;
            int total = 0;
            object lastRecord = null;
            while (reader.Read() && !cancellationToken.IsCancellationRequested)
            {
                var instance = Create(entityType, reader, propMappings);
                output.Add(instance);
                if (output.Count >= batchSize)
                {
                    batchCount++;
                    total += output.Count;
                    await onBatchReceived(output, cancellationToken);
                    logger.LogInformation($"sending batch #{batchCount}, total: {total} records");
                    lastRecord = output[output.Count - 1];
                    output = new List<object>();
                }
            }
            reader?.Dispose();

            if (output.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                batchCount++;
                total += output.Count;
                logger.LogInformation($"sending batch #{batchCount}, count: {total} records");
                await onBatchReceived(output, cancellationToken);
                lastRecord = output[output.Count - 1];
                output.Clear();
            }

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("kusto query is cancelled");
            }

            logger.LogInformation($"total of {output.Count} records retrieved from kusto");

            return (total, lastRecord);
        }

        private async Task EnsureTable<T>(string tableName)
        {
            var tableExists = false;
            try
            {
                var showTableCmd = CslCommandGenerator.GenerateShowTableAdminsCommand(tableName);
                var tableCheck = await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showTableCmd);
                if (tableCheck.RecordsAffected >= 0 && tableCheck.FieldCount > 0) tableExists = true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                logger.LogInformation($"table doesn't exists, creating table {tableName}...");
            }

            if (!tableExists)
            {
                var columnMappings = typeof(T).GetKustoColumnMappings();
                var createTableCmd = CslCommandGenerator.GenerateTableCreateCommand(tableName,
                    columnMappings.Select(cm =>
                    {
                        Type columnType = cm.fieldType;
                        if (columnType.IsEnum)
                        {
                            columnType = typeof(string);
                        }

                        if (columnType == typeof(byte))
                        {
                            columnType = typeof(int);
                        }

                        return new Tuple<string, Type>(cm.mapping.ColumnName, columnType);
                    }));
                logger.LogInformation($"creating kusto table {tableName} with {columnMappings.Count} columns");
                await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, createTableCmd);
            }
        }

        private async Task<(List<T> updates, List<T> inserts)> CheckExistingRecords<T>(string tableName, List<T> items, string idPropName)
        {
            var updates = new List<T>();
            var inserts = new List<T>();

            var idProp = typeof(T).GetProperties()
                .FirstOrDefault(p =>
                {
                    var jsonProp = p.GetCustomAttribute<JsonPropertyAttribute>();
                    var propName = jsonProp?.PropertyName ?? p.Name;
                    return propName.Equals(idPropName, StringComparison.OrdinalIgnoreCase);
                });
            if (idProp == null) return (updates, items);
            var idPropName1 = idProp.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ? "['id']" : idProp.Name;

            var existingIds = new HashSet<string>();

            string lastId = null;
            int throttleSize = 500000;
            while (true)
            {
                var batchRead = 0;
                var idQuery = $"{tableName} \n| order by {idPropName1} asc \n| project {idPropName1} \n| take {throttleSize}";
                if (lastId != null)
                {
                    idQuery = $"{tableName} \n| where strcmp({idPropName1},'{lastId}')>0 \n| order by {idPropName1} asc \n| project {idPropName1} \n| take {throttleSize}";
                }
                var reader = await queryClient.ExecuteQueryAsync(
                    kustoSettings.DbName,
                    idQuery,
                    GetClientRequestProps());
                while (reader.Read())
                {
                    lastId = reader.GetString(0);
                    existingIds.Add(lastId);
                    batchRead++;
                }
                reader.Close();

                if (batchRead < throttleSize)
                {
                    break;
                }
            }

            foreach (var item in items)
            {
                var id = idProp.GetValue(item).ToString();
                var found = existingIds.Contains(id);
                if (found)
                    updates.Add(item);
                else
                    inserts.Add(item);
            }

            return (updates, inserts);
        }

        private ClientRequestProperties GetClientRequestProps(TimeSpan timeout = default)
        {
            var requestProps = new ClientRequestProperties
            {
                ClientRequestId = Guid.NewGuid().ToString()
            };
            if (timeout != default)
            {
                requestProps.SetOption(ClientRequestProperties.OptionServerTimeout, timeout);
            }

            return requestProps;
        }

        #region obj mapping

        /// <summary>
        ///     ObjectReader is buggy and only relies FieldInfo then passing nameBasedColumnMapping=true
        ///     https://msazure.visualstudio.com/_search?action=contents&text=ObjectReader&type=code&lp=custom-Collection&filters=
        ///     &pageSize=25&result
        ///     =DefaultCollection%2FOne%2FAzure-Kusto-Service%2FGBdev%2F%2FSrc%2FCommon%2FKusto.Cloud.Platform%2FData%2FTypedDataReader.cs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Dictionary<int, (PropertyInfo prop, Func<object, object> converter)> BuildFieldMapping<T>(
            IDataReader reader)
        {
            return BuildFieldMapping(typeof(T), reader);
        }

        private Dictionary<int, (PropertyInfo prop, Func<object, object> converter)> BuildFieldMapping(Type type,
            IDataReader reader)
        {
            var constructor = type.GetConstructors().SingleOrDefault(c => !c.GetParameters().Any());
            if (constructor == null) throw new Exception($"type {type.Name} doesn't have parameterless constructor");

            // handle json property mappings
            var props = type.GetProperties().Where(p => p.CanWrite).ToList();
            var propNameMappings = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var prop in props)
            {
                var jsonProp = prop.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonProp != null)
                    propNameMappings.Add(jsonProp.PropertyName, prop);
                else
                    propNameMappings.Add(prop.Name, prop);
            }

            var propMappings = new Dictionary<int, (PropertyInfo prop, Func<object, object> converter)>();
            var fieldTable = reader.GetSchemaTable();
            if (fieldTable == null) throw new InvalidOperationException("Query doesn't return schema info");

            for (var i = 0; i < fieldTable.Rows.Count; i++)
            {
                var fieldName = (string) fieldTable.Rows[i]["ColumnName"];
                var property = type.GetProperty(fieldName);
                if (property == null) propNameMappings.TryGetValue(fieldName, out property);
                var dataType = (Type) fieldTable.Rows[i]["DataType"];
                if (property != null)
                {
                    Func<object, object> converter = null;
                    if (!property.PropertyType.IsAssignableFrom(dataType))
                        converter = CreateConverter(dataType, property.PropertyType);
                    propMappings.Add(i, (property, converter));
                }
                else
                {
                    logger.LogWarning($"Missing mapping for field: {fieldName}");
                }
            }

            return propMappings;
        }

        private T Create<T>(IDataReader reader,
            Dictionary<int, (PropertyInfo prop, Func<object, object> converter)> propMappings)
        {
            return (T) Create(typeof(T), reader, propMappings);
        }

        private object Create(Type type, IDataReader reader,
            Dictionary<int, (PropertyInfo prop, Func<object, object> converter)> propMappings)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var idx in propMappings.Keys)
            {
                var value = reader.GetValue(idx);
                if (value == null || value == DBNull.Value) continue;

                var prop = propMappings[idx].prop;
                if (prop.PropertyType != value.GetType())
                {
                    var converter = propMappings[idx].converter;
                    if (converter != null)
                    {
                        value = converter(value);
                        prop.SetValue(instance, value);
                    }
                    else
                    {
                        try
                        {
                            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                            value = Convert.ChangeType(
                                value.ToString(),
                                underlyingType != null ? underlyingType : prop.PropertyType);
                            prop.SetValue(instance, value);
                        }
                        catch
                        {
                            logger.LogWarning($"Faile to convert type for column: {prop.Name}, value: {value}");
                        }
                    }
                }
                else
                {
                    prop.SetValue(instance, value);
                }
            }

            return instance;
        }

        private Func<object, object> CreateConverter(Type srcType, Type tgtType)
        {
            if (tgtType.IsEnum && srcType == typeof(string))
            {
                object Converter(object s)
                {
                    return Enum.Parse(tgtType, (string) s, true);
                }

                return Converter;
            }

            if (tgtType == typeof(bool) && srcType == typeof(sbyte))
            {
                object Converter(object s)
                {
                    return Convert.ChangeType(s, tgtType);
                }

                return Converter;
            }

            if (tgtType == typeof(string[]) && srcType == typeof(string))
            {
                object Converter(object s)
                {
                    var stringValue = s.ToString().Trim().Trim('[', ']');
                    var items = stringValue.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim().Trim(new[] {'"'}).Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToArray();
                    return items;
                }

                return Converter;
            }

            if (tgtType == typeof(string) && srcType == typeof(string[]))
            {
                object Converter(object s)
                {
                    return s is string[] stringArray && stringArray.Length > 0 ? string.Join(",", stringArray) : "";
                }

                return Converter;
            }

            return null;
        }

        #endregion
    }
}