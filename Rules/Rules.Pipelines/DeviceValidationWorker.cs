// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationWorker.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Config;
    using Common.Storage;
    using Common.Telemetry;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Persistence;
    using Pipelines;

    public class DeviceValidationWorker : BackgroundService
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly IValidator validator;
        private readonly IExportValidationResultToKusto exportToKusto;
        private readonly ILogger<DeviceValidationWorker> logger;
        private readonly PipelineSettings pipelineSettings;
        private readonly IQueueClient<DeviceValidationJob> queueClient;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;

        public DeviceValidationWorker(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DeviceValidationWorker>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            queueClient = serviceProvider.GetRequiredService<IQueueClient<DeviceValidationJob>>();
            validator = serviceProvider.GetRequiredService<IValidator>();

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            pipelineSettings = configuration.GetConfiguredSettings<PipelineSettings>();
            exportToKusto = serviceProvider.GetRequiredService<IExportValidationResultToKusto>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("trying to get job payload from queue");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messages = await queueClient.Dequeue(pipelineSettings.MaxParallelJobs,
                        pipelineSettings.ProcessTimeout, stoppingToken);
                    if (messages.Any())
                    {
                        foreach (var message in messages)
                        {
                            var validationJob = message.Value;
                            using var scope = appTelemetry.StartOperation(this, validationJob.ActivityId);

                            var run = new DeviceValidationRun
                            {
                                JobId = validationJob.Id,
                                ExecutionTime = DateTime.UtcNow
                            };
                            run = await runRepo.Create(run);
                            var timeoutCancellationSource =
                                new CancellationTokenSource(pipelineSettings.ProcessTimeout);
                            var timeoutCancellation = timeoutCancellationSource.Token;
                            var linkedCancellation =
                                CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCancellation);

                            try
                            {
                                run = await validator.ValidateDevices(validationJob, run, linkedCancellation.Token);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, $"failed validation: {ex.Message}");
                            }

                            if (run.Succeed || message.DequeueCount >= pipelineSettings.MaxDequeueCount)
                            {
                                logger.LogInformation(
                                    $"removing job from queue, id: {message.MessageId}, receipt: {message.Receipt}");
                                await queueClient.DeleteMessage(message.MessageId, message.Receipt, stoppingToken);
                            }
                            else if (message.DequeueCount < pipelineSettings.MaxDequeueCount)
                            {
                                await queueClient.ResetVisibility(message.MessageId, message.Receipt, validationJob,
                                    stoppingToken);
                                logger.LogInformation($"job failed, dequeue count: {message.DequeueCount}, will try again");
                            }

                            var jobStatus = run.Succeed ? "successful" : "failed";
                            logger.LogInformation($"pipeline completed, {jobStatus}");

                            if (!run.Succeed) continue;

                            try
                            {
                                logger.LogInformation("exporting validation results...");
                                var totalResultsExported = await exportToKusto.ExportToKusto(run.Id, stoppingToken);
                                logger.LogInformation(
                                    $"total of {totalResultsExported} validation results exported to kusto");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "failed to export to kusto");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"failed to process job, {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}