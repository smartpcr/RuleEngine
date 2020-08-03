// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineBlocks.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Builders
{
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Rules;

    public class PipelineBlocks<TInput, TOutput> 
        where TInput: class,new()
        where TOutput: class,new()
    {
        public BufferBlock<(TInput Payload, ValidationRule Rule)> ProducerBlock { get; set; }
        public IPropagatorBlock<(TInput Payload, ValidationRule Rule), TOutput> TransformerBlock { get; set; }
        public IPropagatorBlock<TOutput, TOutput[]> BatcherBlock { get; set; }
        public ITargetBlock<TOutput[]> PersistenceBlock { get; set; }
    }
}