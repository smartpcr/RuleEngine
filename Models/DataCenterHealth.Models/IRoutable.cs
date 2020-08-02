// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRoutable.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    /// <summary>
    ///     use routekey to route payload to different downstream processing pipeline
    ///     used in dataflow broadcast block (TransformMany)
    /// </summary>
    public interface IRoutable
    {
        int RouteKey { get; set; }
        IRoutable Clone();
        IRoutable WithRouteKey(int key);
    }
}