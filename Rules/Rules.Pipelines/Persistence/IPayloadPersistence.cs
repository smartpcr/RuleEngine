// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadPersistence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Persistence
{
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Validation;

    public interface IPayloadPersistence<T> where T : class, new()
    {
        ITargetBlock<T[]> CreateFinalTask(EvaluationContext context, CancellationToken cancel);
    }
}