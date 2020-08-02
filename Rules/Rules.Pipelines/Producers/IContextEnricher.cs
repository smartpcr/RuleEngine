// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IContextEnricher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Rules.Validations.Pipelines;

    public interface IContextEnricher<in T> : IDisposable where T : class, new()
    {
        string Name { get; }
        int ApplyOrder { get; }
        void Enrich(PipelineExecutionContext context, T instance);
        void EnrichLiveData(PipelineExecutionContext context, T instance);
        void EnsureLookup(PipelineExecutionContext context);
        void EnsureLiveData(PipelineExecutionContext context);
    }

    public class ContextEnricherFactory<T> where T : class, new()
    {
        public IContextEnricher<T> GetContextEnricher(IServiceProvider sp, string name)
        {
            var enrichers = sp.GetServices<IContextEnricher<T>>();
            return enrichers.First(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}