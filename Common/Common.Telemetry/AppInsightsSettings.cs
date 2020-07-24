// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppInsightsSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    public class AppInsightsSettings
    {
        public string InstrumentationKey { get; set; }
        public string Role { get; set; }
        public string Namespace { get; set; }
        public string Version { get; set; }
        public string[] Tags { get; set; }
        public bool EnableTracing { get; set; }
        public bool IsJob { get; set; }
    }
}