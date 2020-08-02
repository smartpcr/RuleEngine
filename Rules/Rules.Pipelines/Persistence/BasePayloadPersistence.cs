// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePayloadPersistence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Persistence
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public abstract class BasePayloadPersistence<TPayload, TConfig> : IPayloadPersistence<TPayload, TConfig>
        where TConfig : class, new()
    {
        protected BasePayloadPersistence(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }

        protected PipelineSettings Settings { get; }
        public PipelineActivityType ActivityType => PipelineActivityType.Action;
        public abstract string Name { get; }

        public ITargetBlock<TPayload> CreateFinalTask(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var totalSentToSave = 0;
            var saveResultBlock = new ActionBlock<TPayload>(async array =>
            {
                Interlocked.Increment(ref totalSentToSave);
                LogInformation($"received batch to save, total {totalSentToSave}");

                await BulkInsert(array, context, cancellationToken);
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = Settings.PersistenceBatchSize,
                MaxDegreeOfParallelism = Settings.MaxParallelism,
                CancellationToken = cancellationToken
            });

            return saveResultBlock;
        }

        public IPropagatorBlock<TPayload, TPayload> CreateTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ISourceBlock<TPayload> CreateFirstTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected abstract Task BulkInsert(TPayload payload, PipelineExecutionContext context,
            CancellationToken cancellationToken);

        protected abstract void LogInformation(string message);
    }
}