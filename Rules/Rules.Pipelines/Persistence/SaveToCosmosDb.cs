// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceValidationResultToDb.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Persistence
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Telemetry;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Pipelines;

    public class SaveToCosmosDb : BasePayloadPersistence<DeviceValidationResult[], DeviceValidationJob>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ILogger<SaveToCosmosDb> logger;
        private readonly IDocDbRepository<DeviceValidationResult> resultRepo;

        public SaveToCosmosDb(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider)
        {
            logger = loggerFactory.CreateLogger<SaveToCosmosDb>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            resultRepo = repoFactory.CreateRepository<DeviceValidationResult>();
        }

        public override string Name => GetType().Name;

        protected override async Task BulkInsert(
            DeviceValidationResult[] payload,
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var payloadList = payload.Where(p => p.Assert.HasValue).ToList();
            logger.LogInformation($"Saving {payloadList.Count} out of {payload.Length} that are evaluated...");
            if (payloadList.Count > 0)
                try
                {
                    await resultRepo.BulkUpsert(payloadList, cancellationToken);
                    context.AddTotalSaved(payload.Length);
                    logger.LogInformation($"total saved: {context.TotalSaved}");
                    appTelemetry.RecordMetric(
                        $"{nameof(SaveToCosmosDb)}-total",
                        context.TotalSaved);
                }
                catch (Exception ex)
                {
                    context.AddTotalFailed(1);
                    logger.LogError(ex, "Failed to save to cosmosdb");
                }
        }

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }
    }
}