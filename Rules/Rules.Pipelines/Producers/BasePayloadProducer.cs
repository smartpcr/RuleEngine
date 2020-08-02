// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePayloadProducer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public abstract class BasePayloadProducer<TPayload, TConfig> : IPayloadProducer<TPayload, TConfig>
        where TPayload : class, new()
        where TConfig : class, new()
    {
        protected BasePayloadProducer(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }

        private PipelineSettings Settings { get; }
        public abstract string Name { get; }
        public PipelineActivityType ActivityType => PipelineActivityType.Producer;

        public ISourceBlock<TPayload> CreateFirstTask(
            PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var producerBlock = new BufferBlock<TPayload>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = Settings.MaxBufferCapacity,
                    CancellationToken = cancellationToken
                });
            return producerBlock;
        }

        public abstract Task<IList<TPayload>> Produce(
            PipelineExecutionContext context,
            TConfig config,
            CancellationToken cancellationToken);

        public IPropagatorBlock<TPayload, TPayload> CreateTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ITargetBlock<TPayload> CreateFinalTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}