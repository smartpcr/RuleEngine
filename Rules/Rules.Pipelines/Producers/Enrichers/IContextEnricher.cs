// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IContextEnricher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers.Enrichers
{
    using System;
    using System.Threading;
    using DataCenterHealth.Models.Validation;

    public interface IContextEnricher<in T> : IDisposable where T : class, new()
    {
        string Name { get; }
        int ApplyOrder { get; }
        void EnsureLookup(EvaluationContext context, bool isLiveData = false, CancellationToken cancel = default);
        void Enrich(EvaluationContext context, T instance, bool isLiveData = false);
    }
}