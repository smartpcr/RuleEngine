// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextEnricherFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers.Enrichers
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    public class ContextEnricherFactory<T> where T : class, new()
    {
        public IContextEnricher<T> GetContextEnricher(IServiceProvider sp, string name)
        {
            var enrichers = sp.GetServices<IContextEnricher<T>>();
            return enrichers.First(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}