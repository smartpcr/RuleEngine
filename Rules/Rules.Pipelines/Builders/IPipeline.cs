// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipelineFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Builders
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using Rules.Pipelines.Batches;
    using Rules.Pipelines.Persistence;
    using Rules.Pipelines.Producers;
    using Rules.Pipelines.Transformer;

    public interface IPipeline<T> where T : class, new()
    {
        string Name { get; }
        IPayloadProducer<PowerDevice> Producer { get; }
        IPayloadTransformer<PowerDevice> Transformer{ get; }
        IPayloadBatcher<EvaluationResult> Batcher{ get; }
        IPayloadPersistence<EvaluationResult> Persister{ get; }
        List<IDataflowBlock> AllBlocks { get; }
        
        PipelineBlocks<T, EvaluationResult> CreatePipeline(EvaluationContext context, CancellationToken cancel);
    }
}