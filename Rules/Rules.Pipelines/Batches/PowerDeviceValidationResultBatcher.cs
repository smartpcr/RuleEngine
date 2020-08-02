// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceValidationResultBatcher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Batches
{
    using System;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using DataCenterHealth.Models.Jobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Pipelines;

    public class PowerDeviceValidationResultBatcher : IPayloadBatcher<DeviceValidationResult>
    {
        private readonly ILogger<PowerDeviceValidationResultBatcher> logger;
        private readonly PipelineSettings settings;

        public PowerDeviceValidationResultBatcher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<PowerDeviceValidationResultBatcher>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }

        public PipelineActivityType ActivityType => PipelineActivityType.Batch;

        public IPropagatorBlock<DeviceValidationResult, DeviceValidationResult[]> CreateTask(
            PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            var batchBlock = new BatchBlock<DeviceValidationResult>(settings.PersistenceBatchSize);
            logger.LogInformation(
                $"created batch for {nameof(DeviceValidationResult)} with size: {settings.PersistenceBatchSize}");
            return batchBlock;
        }

        public ISourceBlock<DeviceValidationResult[]> CreateFirstTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ITargetBlock<DeviceValidationResult> CreateFinalTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}