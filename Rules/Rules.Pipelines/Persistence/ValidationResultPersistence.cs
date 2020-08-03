// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationPersistence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Persistence
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Common.Telemetry;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ValidationResultPersistence : IPayloadPersistence<EvaluationResult>
    {
        private readonly PipelineSettings settings;
        private readonly IAppTelemetry appTelemetry;
        private readonly ILogger<ValidationResultPersistence> logger;
        private readonly IDocDbRepository<EvaluationResult> resultRepo;

        public ValidationResultPersistence(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<PipelineSettings>();
            logger = loggerFactory.CreateLogger<ValidationResultPersistence>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            resultRepo = repoFactory.CreateRepository<EvaluationResult>();
        }
        
        public ITargetBlock<EvaluationResult[]> CreateFinalTask(EvaluationContext context, CancellationToken cancel)
        {
            var totalSentToSave = 0;
            var saveResultBlock = new ActionBlock<EvaluationResult[]>(async array =>
            {
                Interlocked.Increment(ref totalSentToSave);
                logger.LogInformation($"received batch to save, total {totalSentToSave}");

                await BulkInsert(array, context, cancel);
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = settings.PersistenceBatchSize,
                MaxDegreeOfParallelism = settings.MaxParallelism,
                CancellationToken = cancel
            });

            return saveResultBlock;
        }
        
        private async Task BulkInsert(
            EvaluationResult[] payload,
            EvaluationContext context,
            CancellationToken cancellationToken)
        {
            var payloadList = payload.Where(p => p.Passed.HasValue).ToList();
            logger.LogInformation($"Saving {payloadList.Count} out of {payload.Length} that are evaluated...");
            if (payloadList.Count > 0)
                try
                {
                    await resultRepo.BulkUpsert(payloadList, cancellationToken);
                    context.AddTotalSaved(payload.Length);
                    logger.LogInformation($"total saved: {context.TotalSaved}");
                    appTelemetry.RecordMetric(
                        $"{nameof(ValidationResultPersistence)}-total",
                        context.TotalSaved);
                }
                catch (Exception ex)
                {
                    context.AddTotalFailed(1);
                    logger.LogError(ex, "Failed to save to cosmosdb");
                }
        }
    }
}