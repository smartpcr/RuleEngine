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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Config;
    using global::Kusto.Data.Common;
    using global::Kusto.Ingest;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class KustoClient : IKustoClient
    {
        private readonly ICslAdminProvider adminClient;
        private readonly IKustoIngestClient ingestClient;
        private readonly KustoSettings kustoSettings;
        private readonly ILogger<KustoClient> logger;
        private readonly ICslQueryProvider queryClient;

        public KustoClient(IConfiguration configuration, ILoggerFactory loggerFactory,
            KustoSettings kustoSettings = null)
        {
            logger = loggerFactory.CreateLogger<KustoClient>();
            this.kustoSettings = kustoSettings ?? configuration.GetConfiguredSettings<KustoSettings>();
            var clientFactory = new ClientFactory(configuration, kustoSettings);
            queryClient = clientFactory.QueryQueryClient;
            adminClient = clientFactory.AdminClient;
            ingestClient = clientFactory.IngestClient;
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"kusto query:\n{query}");
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                new ClientRequestProperties {ClientRequestId = Guid.NewGuid().ToString()});
            return Read<T>(reader, cancellationToken);
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
                new ClientRequestProperties {ClientRequestId = Guid.NewGuid().ToString()});
            return await Read(reader, onBatchReceived, cancellationToken, batchSize);
        }

        public async Task<(int Total, object LastRecord)> ExecuteQuery(Type entityType, string query, Func<IList<object>, CancellationToken, Task> onBatchReceived, CancellationToken cancellationToken = default,
            int batchSize = 100)
        {
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() });
            return await Read(entityType, reader, onBatchReceived, cancellationToken, batchSize);
        }

        public async Task<IEnumerable<T>> ExecuteFunction<T>(string functionName, CancellationToken cancellationToken,
            params (string name, string value)[] parameters)
        {
            var functionParameters = parameters.Select(p => new KeyValuePair<string, string>(p.name, p.value));
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                functionName,
                new ClientRequestProperties(null, functionParameters) {ClientRequestId = Guid.NewGuid().ToString()});
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
                new ClientRequestProperties(null, functionParameters) {ClientRequestId = Guid.NewGuid().ToString()});
            await Read(reader, onBatchReceived, cancellationToken, batchSize);
        }

        public async Task BulkInsert<T>(
            string tableName,
            IList<T> items,
            bool appendOnly,
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
            if (!appendOnly)
            {
                var upserts = await CheckExistingRecords(tableName, items.ToList(), idPropName);
                await using var memoryStream = new MemoryStream();
                await using var writer = new StreamWriter(memoryStream);
                if (upserts.inserts.Count > 0)
                {
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
                await using var memoryStream = new MemoryStream();
                await using var writer = new StreamWriter(memoryStream);
                foreach (var item in items) await writer.WriteLineAsync(JsonConvert.SerializeObject(item));
                await writer.FlushAsync();
                totalSize = memoryStream.Length;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await ingestClient.IngestFromStreamAsync(memoryStream, props);
            }

            logger.LogInformation($"ingested {items.Count} records to kuso table {tableName}, size: {totalSize} bytes");
        }

        public async Task<T> ExecuteScalar<T>(string query, string fieldName, CancellationToken cancel)
        {
            logger.LogInformation($"kusto query:\n{query}");
            var reader = await queryClient.ExecuteQueryAsync(
                kustoSettings.DbName,
                query,
                new ClientRequestProperties {ClientRequestId = Guid.NewGuid().ToString()});
            if (reader.Read())
            {
                return (T) reader[fieldName];
            }
            reader.Dispose();

            return default(T);
        }

        public async Task DropTable(string tableName, CancellationToken cancel)
        {
            logger.LogInformation($"dropping kusto table: {tableName}...");
            var showTableCmd = CslCommandGenerator.GenerateTableDropCommand(tableName, true);
            await adminClient.ExecuteControlCommandAsync(kustoSettings.DbName, showTableCmd);
            logger.LogInformation($"kusto table: {tableName} is dropped");
        }

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

        private async Task<(int Total, object LastRecord)> Read(Type entityType, IDataReader reader, Func<IList<object>, CancellationToken, Task> onBatchReceived, CancellationToken cancellationToken, int batchSize)
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

            var existingIds = new HashSet<string>();

            string lastId = null;
            int throttleSize = 500000;
            while (true)
            {
                var batchRead = 0;
                var idQuery = $"{tableName} \n| order by {idProp.Name} asc \n| project {idProp.Name} \n| take {throttleSize}";
                if (lastId != null)
                {
                    idQuery = $"{tableName} \n| where strcmp({idProp.Name},'{lastId}')>0 \n| order by {idProp.Name} asc \n| project {idProp.Name} \n| take {throttleSize}";
                }
                var reader = await queryClient.ExecuteQueryAsync(
                    kustoSettings.DbName,
                    idQuery,
                    new ClientRequestProperties { ClientRequestId = Guid.NewGuid().ToString() });
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