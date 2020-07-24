// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricsSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    public class MetricsSettings
    {
        public bool UseAppInsights { get; set; }
        public bool UsePrometheus { get; set; }
        public PrometheusMetricSettings Prometheus { get; set; }
        public AppInsightsSettings AppInsights { get; set; }
    }
}