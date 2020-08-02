// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveToKusto.cs" company="Microsoft Corporation">
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
    using Common.Kusto;
    using Common.Telemetry;
    using DataCenterHealth.Models.Jobs;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Pipelines;

    public class SaveToKusto : BasePayloadPersistence<DeviceValidationResult[], DeviceValidationJob>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly IKustoClient client;
        private readonly ILogger<SaveToKusto> logger;

        public SaveToKusto(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            logger = loggerFactory.CreateLogger<SaveToKusto>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            client = serviceProvider.GetRequiredService<IKustoClient>();
        }

        public override string Name => GetType().Name;

        protected override async Task BulkInsert(DeviceValidationResult[] payload, PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var payloadList = payload.Where(p => p.Assert.HasValue).ToList();
            logger.LogInformation($"Saving {payloadList.Count} out of {payload.Length} that are evaluated...");
            if (payloadList.Count > 0)
                try
                {
                    await client.BulkInsert("ValidationResult", payload.ToList(), IngestMode.AppendOnly, "Id", cancellationToken);
                    context.AddTotalSaved(payload.Length);
                    logger.LogInformation($"total saved: {context.TotalSaved}");
                    appTelemetry.RecordMetric(
                        $"{GetType().Name}-total",
                        context.TotalSaved);
                }
                catch (Exception ex)
                {
                    context.AddTotalFailed(1);
                    logger.LogError(ex, "Failed to save to kusto");
                }
        }

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }
    }
}