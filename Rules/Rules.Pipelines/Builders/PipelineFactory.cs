// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Builders
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    public class PipelineFactory
    {
        public IPipeline<T> CreatePipeline<T>(IServiceProvider serviceProvider) where T : class, new()
        {
            var pipelines = serviceProvider.GetServices<IPipeline<T>>();
            return pipelines.First(p => p.Name.Equals(typeof(T).Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}