// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActivityFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Pipelines
{
    using System.Threading;
    using System.Threading.Tasks.Dataflow;

    public enum PipelineActivityType
    {
        Producer,
        Filter,
        Transform,
        Broadcast,
        Batch,
        Action
    }

    public interface IPipelineActivityFactory<in TInput, out TOutput>
    {
        PipelineActivityType ActivityType { get; }

        IPropagatorBlock<TInput, TOutput> CreateTask(PipelineExecutionContext context, CancellationToken cancellationToken);

        ISourceBlock<TOutput> CreateFirstTask(PipelineExecutionContext context, CancellationToken cancellationToken);

        ITargetBlock<TInput> CreateFinalTask(PipelineExecutionContext context, CancellationToken cancellationToken);
    }
}