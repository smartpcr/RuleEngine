// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadPersistence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Persistence
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public interface IPayloadPersistence<TPayload, TConfig> : IPipelineActivityFactory<TPayload, TPayload>
        where TConfig : class, new()
    {
        string Name { get; }
    }

    public class PayloadPersistenceFactory<TInput, TConfig>
        where TConfig : class, new()
    {
        public IPayloadPersistence<TInput, TConfig> GetPersistenceActivity(IServiceProvider sp, string name)
        {
            var svcs = sp.GetServices<IPayloadPersistence<TInput, TConfig>>();
            return svcs.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}