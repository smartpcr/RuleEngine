// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IContextProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataCenterHealth.Models.Validation;
    using Rules.Pipelines.Producers.Enrichers;

    public interface IContextProvider<T> : IDisposable where T : class, new()
    {
        IEnumerable<IContextEnricher<T>> ContextEnrichers { get; }
        Task<IEnumerable<T>> Provide(EvaluationContext context, ValidationContextScope contextScope, List<string> ids, CancellationToken cancel);
    }
}