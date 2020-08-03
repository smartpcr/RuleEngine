// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationPipelineFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using Microsoft.Extensions.DependencyInjection;
    using Rules.Pipelines.Batches;
    using Rules.Pipelines.Persistence;
    using Rules.Pipelines.Producers;
    using Rules.Pipelines.Transformer;

    public class DeviceValidationPipeline : IPipeline<PowerDevice>
    {
        public IPayloadProducer<PowerDevice> Producer { get; }
        public IPayloadTransformer<PowerDevice> Transformer{ get; }
        public IPayloadBatcher<EvaluationResult> Batcher{ get; }
        public IPayloadPersistence<EvaluationResult> Persister{ get; }
        public List<IDataflowBlock> AllBlocks { get; private set; }

        public DeviceValidationPipeline(IServiceProvider serviceProvider)
        {
            Producer = serviceProvider.GetRequiredService<IPayloadProducer<PowerDevice>>();
            Transformer = serviceProvider.GetRequiredService<IPayloadTransformer<PowerDevice>>();
            Batcher = serviceProvider.GetRequiredService<IPayloadBatcher<EvaluationResult>>();
            Persister = serviceProvider.GetRequiredService<IPayloadPersistence<EvaluationResult>>();
        }

        public string Name => nameof(PowerDevice);

        public PipelineBlocks<PowerDevice, EvaluationResult> CreatePipeline(EvaluationContext context, CancellationToken cancel)
        {
            var producerActivity = Producer.CreateProducerActivity(cancel);
            var transformerActivity = Transformer.CreateTransformActivity(context, cancel);
            var batcherActivity = Batcher.CreateBatchActivity(context, cancel);
            var persistenceBlock = Persister.CreateFinalTask(context, cancel);
            
            AllBlocks = new List<IDataflowBlock>()
            {
                producerActivity,
                transformerActivity,
                batcherActivity,
                persistenceBlock
            };
            
            return new PipelineBlocks<PowerDevice, EvaluationResult>()
            {
                ProducerBlock = producerActivity,
                TransformerBlock = transformerActivity,
                BatcherBlock = batcherActivity,
                PersistenceBlock = persistenceBlock
            };
        }
    }
}