// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextTelemetryInitializer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using System.Linq;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Options;

    internal class ContextTelemetryInitializer : ITelemetryInitializer
    {
        private readonly AppInsightsSettings settings;

        public ContextTelemetryInitializer(IOptions<AppInsightsSettings> serviceContext)
        {
            settings = serviceContext.Value;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = settings.Role;
            telemetry.Context.Component.Version = settings.Version;
            telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
            telemetry.Context.GlobalProperties["AppVersion"] = settings.Version;

            if (settings.Tags?.Any() == true)
                telemetry.Context.GlobalProperties["tags"] = string.Join(",", settings.Tags);
        }
    }
}