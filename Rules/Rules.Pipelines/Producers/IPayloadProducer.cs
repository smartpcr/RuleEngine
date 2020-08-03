// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadProducer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Validation;

    public interface IPayloadProducer<T>
    {
        Task<IEnumerable<(T Payload, ValidationRule Rule)>> Produce(EvaluationContext context, CancellationToken cancel);

        BufferBlock<(T Payload, ValidationRule Rule)> CreateProducerActivity(CancellationToken cancellationToken);
    }
}