// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadBatcher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Batches
{
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Validation;

    public interface IPayloadBatcher<T> where T : class, new()
    {
        IPropagatorBlock<EvaluationResult, EvaluationResult[]> CreateBatchActivity(EvaluationContext context, CancellationToken cancel);
    }
}