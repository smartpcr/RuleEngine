// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPipeline.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Pipelines
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public interface IPipeline<TPayload, TResult, in TConfig>
        where TPayload : class, new()
        where TResult : class
        where TConfig : class, new()
    {
        string Name { get; }
        Task<TResult> ExecuteAsync(string runId, TConfig config, CancellationToken cancellationToken);
    }
}