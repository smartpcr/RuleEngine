// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadTransformer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Transformer
{
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Validation;

    public interface IPayloadTransformer<T> where T : class, new()
    {
        IPropagatorBlock<(T Payload, ValidationRule Rule), EvaluationResult> CreateTransformActivity(EvaluationContext context, CancellationToken cancel);
        EvaluationResult Evaluate((T Payload, ValidationRule Rule) payload, EvaluationContext context);
    }
}