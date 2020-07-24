// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrometheusMetricSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    public class PrometheusMetricSettings
    {
        public string Role { get; set; }
        public string Namespace { get; set; }
        public string Route { get; set; } = "/metrics";

        /// <summary>
        ///     only used in console (generic host) app. When app used in k8s, make sure to config containerPort
        ///     better option would be always use webhost
        /// </summary>
        public int Port { get; set; }

        public bool UseHttps { get; set; }
    }
}