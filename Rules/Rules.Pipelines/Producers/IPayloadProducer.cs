// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadProducer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public interface IPayloadProducer<TPayload, in TConfig> : IPipelineActivityFactory<TPayload, TPayload>
        where TPayload : class, new()
        where TConfig : class, new()
    {
        string Name { get; }
        Task<IList<TPayload>> Produce(
            PipelineExecutionContext context,
            TConfig config,
            CancellationToken cancellationToken);
    }

    public class PayloadProducerFactory<TInput, TConfig>
        where TInput : class, new()
        where TConfig : class, new()
    {
        public IPayloadProducer<TInput, TConfig> GetTransformer(IServiceProvider sp, string name)
        {
            var svcs = sp.GetServices<IPayloadProducer<TInput, TConfig>>();
            return svcs.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}