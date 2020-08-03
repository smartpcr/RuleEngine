// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePayloadProducer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class BasePayloadProducer<T> : IPayloadProducer<T>
    {
        protected PipelineSettings Settings { get; }
        
        protected BasePayloadProducer(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }

        public abstract Task<IEnumerable<(T Payload, ValidationRule Rule)>> Produce(EvaluationContext context, CancellationToken cancel);

        public BufferBlock<(T Payload, ValidationRule Rule)> CreateProducerActivity(CancellationToken cancellationToken)
        {
            var producerBlock = new BufferBlock<(T Payload, ValidationRule Rule)>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = Settings.MaxBufferCapacity,
                    CancellationToken = cancellationToken
                });
            return producerBlock;
        }
    }
}