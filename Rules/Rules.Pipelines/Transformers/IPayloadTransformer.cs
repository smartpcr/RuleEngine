// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadTransformer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public interface IPayloadTransformer<in TInput, out TOutput, in TConfig> : IPipelineActivityFactory<TInput, TOutput>
        where TInput : class, new()
        where TOutput : class, new()
        where TConfig : class, new()
    {
        string Name { get; }
    }

    public class PayloadTransformerFactory<TInput, TOutput, TConfig>
        where TInput : class, new()
        where TOutput : class, new()
        where TConfig : class, new()
    {
        public IPayloadTransformer<TInput, TOutput, TConfig> GetTransformer(IServiceProvider sp, string name)
        {
            var svcs = sp.GetServices<IPayloadTransformer<TInput, TOutput, TConfig>>();
            return svcs.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}