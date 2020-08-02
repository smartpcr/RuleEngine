// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadBatcher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Batches
{
    using Pipelines;

    public interface IPayloadBatcher<TPayload> : IPipelineActivityFactory<TPayload, TPayload[]>
        where TPayload : class, new()
    {
    }
}