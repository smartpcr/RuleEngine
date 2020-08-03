// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationResultBatcher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Batches
{
    using System;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ValidationResultBatcher : IPayloadBatcher<EvaluationResult>
    {
        private readonly ILogger<ValidationResultBatcher> logger;
        private readonly PipelineSettings settings;

        public ValidationResultBatcher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ValidationResultBatcher>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }
        
        public IPropagatorBlock<EvaluationResult, EvaluationResult[]> CreateBatchActivity(EvaluationContext context, CancellationToken cancel)
        {
            var batchBlock = new BatchBlock<EvaluationResult>(settings.PersistenceBatchSize);
            logger.LogInformation(
                $"created batch for {nameof(EvaluationResult)} with size: {settings.PersistenceBatchSize}");
            return batchBlock;
        }
    }
}