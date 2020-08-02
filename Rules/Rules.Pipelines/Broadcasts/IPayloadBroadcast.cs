// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPayloadBroadcast.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Broadcasts
{
    using Pipelines;

    public interface IPayloadBroadcast<TPayload> : IPipelineActivityFactory<TPayload, TPayload>
        where TPayload : class, new()
    {
        int TotalConsumers { get; set; }
    }
}