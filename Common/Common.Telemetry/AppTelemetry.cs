// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppTelemetry.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Config;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    
    public class AppTelemetry : IAppTelemetry, IDisposable
    {
        private readonly string ns;
        private readonly TelemetryClient telemetryClient;

        public AppTelemetry(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
            var settings = configuration.GetConfiguredSettings<AppInsightsSettings>();
            ns = settings.Namespace;
            //var tracerFactory = serviceProvider.GetService<TracerFactoryBase>();
        }

        public string OperationId => Activity.Current.Id;

        public void RecordMetric(string name, long value, params (string key, string value)[] dimensions)
        {
            ValidateDimensions(dimensions);

            var metricIdentifier = new MetricIdentifier(ns, name, dimensions.Select(p => p.key).ToList());
            var metric = telemetryClient.GetMetric(metricIdentifier);
            var tags = dimensions.Select(p => p.value).ToArray();
            if (tags.Length > 10)
            {
                var m = telemetryClient.GetMetric(new MetricIdentifier(ns, "invalid-metric", "metric-name",
                    "too many dimensions"));
                m.TrackValue(1, ns + "/" + name, dimensions.Length.ToString());
            }

            metric?.Record(value, dimensions.Select(p => p.value).ToArray());
        }

        public IDisposable StartOperation([NotNull] object caller, string parentOperationId = null,
            [CallerMemberName] string operationName = "")
        {
            operationName = caller == null ? operationName : caller.GetType().Name + "." + operationName;
            return OperationScope.StartOperation(parentOperationId, operationName, telemetryClient);
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties)
        {
            telemetryClient.TrackEvent(eventName, properties);
        }

        public void Dispose()
        {
            telemetryClient?.Flush();
        }

        private static void ValidateDimensions((string key, string value)[] dimensions)
        {
            if (null == dimensions || 0 == dimensions.Length) return;

            var keys = new HashSet<string>(dimensions.Length);
            for (var i = 0; i < dimensions.Length; ++i)
            {
                if (keys.Contains(dimensions[i].key))
                    throw new ArgumentException(
                        $"Duplicate key `{dimensions[i].key}` not allowed for metric dimensions!");
                if (string.IsNullOrEmpty(dimensions[i].value)) dimensions[i].value = "?";
                keys.Add(dimensions[i].key);
            }
        }
    }

    internal static class AppInsightsMetricExtension
    {
        public static void Record(this Metric metric, double value, params string[] tags)
        {
            switch (tags.Length)
            {
                case 0:
                    metric.TrackValue(value);
                    break;
                case 1:
                    metric.TrackValue(value, tags[0]);
                    break;
                case 2:
                    metric.TrackValue(value, tags[0], tags[1]);
                    break;
                case 3:
                    metric.TrackValue(value, tags[0], tags[1], tags[2]);
                    break;
                case 4:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3]);
                    break;
                case 5:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4]);
                    break;
                case 6:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4], tags[5]);
                    break;
                case 7:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4], tags[5], tags[6]);
                    break;
                case 8:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4], tags[5], tags[6], tags[7]);
                    break;
                case 9:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4], tags[5], tags[6], tags[7],
                        tags[8]);
                    break;
                case 10:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4], tags[5], tags[6], tags[7],
                        tags[8], tags[9]);
                    break;
                default:
                    metric.TrackValue(value, tags[0], tags[1], tags[2], tags[3], tags[4], tags[5], tags[6], tags[7],
                        tags[8], tags[9]);
                    break;
            }
        }
    }
}