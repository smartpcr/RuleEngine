//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="KustoRepo.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Config;
    using Common.Kusto;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class KustoRepo<T> : IKustoRepo<T> where T : class, new()
    {
        private readonly IKustoClient kustoClient;
        private readonly ILogger<KustoRepo<T>> logger;
        private readonly KustoSettings kustoSettings;

        public KustoRepo(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<KustoRepo<T>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var kustoDataSettings = configuration.GetConfiguredSettings<KustoDataSettings>();
            var prop = kustoDataSettings.GetType().GetProperties()
                .FirstOrDefault(p =>
                {
                    var customAttr = p.GetCustomAttribute<MappedModelAttribute>();
                    if (customAttr != null && customAttr.ModelType == typeof(T)) return true;

                    return false;
                });
            if (prop == null) throw new Exception($"Missing backend mapping for model: {typeof(T).Name}");
            kustoSettings = prop.GetValue(kustoDataSettings) as KustoSettings;
            kustoClient = new KustoClient(serviceProvider, loggerFactory, kustoSettings);
        }

        public async Task<IList<T>> Query([NotNull]string query, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Executing kusto query: {query}");
            var watch = Stopwatch.StartNew();
            var items = await kustoClient.ExecuteQuery<T>(query, TimeSpan.FromMinutes(30), cancellationToken);
            var list = items.ToList();
            watch.Stop();
            logger.LogInformation($"it took {watch.Elapsed} to retrieve {list.Count} records");
            return list;
        }

        public async Task<IList<TRecord>> ExecuteQuery<TRecord>([NotNull]string query, Func<IDataReader, TRecord> read, CancellationToken cancel)
        {
            var output = new List<TRecord>();
            var reader = await kustoClient.ExecuteReader(query);
            while (reader.Read() && !cancel.IsCancellationRequested)
            {
                var instance = read(reader);
                output.Add(instance);
            }
            reader.Close();
            logger.LogInformation($"total of {output.Count} records retrieved from kusto");
            return output;
        }

        public async Task<DateTime> GetLastModificationTime([NotNull]string query, CancellationToken cancel)
        {
            var modificationTimeField = "LastWriteTime";
            var kustoQuery = @"
| extend IngestionTime=ingestion_time()
| summarize LastWriteTime=max(IngestionTime)";
            kustoQuery = $"{query} {kustoQuery}";
            DateTime lastWriteTime;
            try
            {
                lastWriteTime = await kustoClient.ExecuteScalar<DateTime>(kustoQuery, modificationTimeField, cancel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to get last ingestion time using kusto query: \n{kustoQuery}");
                lastWriteTime = default;
            }
            return lastWriteTime;
        }

        public async Task<int> Ingest(List<T> payload, IngestMode ingestMode, CancellationToken cancel)
        {
            var itemsAdded = await kustoClient.BulkInsert(kustoSettings.TableName, payload, ingestMode, "id", cancel);
            return itemsAdded;
        }
    }
}