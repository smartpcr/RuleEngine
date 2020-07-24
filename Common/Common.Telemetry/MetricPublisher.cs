// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricPublisher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Extensions.DependencyInjection;
    using Prometheus.Client;

    public interface IMetricPublisher
    {
        void WriteMetric(string name, double value, params KeyValuePair<string, string>[] dimensions);
    }

    public class MetricPublisher : IMetricPublisher
    {
        private readonly ConcurrentDictionary<string, Metric> _aiMetrics;
        private readonly ConcurrentDictionary<string, Counter> _prometheusMetrics;
        private readonly MetricsSettings _settings;
        private readonly TelemetryClient _telemetryClient;

        public MetricPublisher(IServiceCollection services, MetricsSettings settings)
        {
            _settings = settings;

            var serviceProvider = services.BuildServiceProvider();
            if (settings.UseAppInsights) _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

            _aiMetrics = new ConcurrentDictionary<string, Metric>();
            _prometheusMetrics = new ConcurrentDictionary<string, Counter>();
        }

        public void WriteMetric(string name, double value, params KeyValuePair<string, string>[] dimensions)
        {
            if (_settings.UsePrometheus)
            {
                if (!_prometheusMetrics.TryGetValue(name, out var counter))
                {
                    counter = Metrics.CreateCounter(name, name, dimensions?.Select(p => p.Key).ToArray());
                    _prometheusMetrics.AddOrUpdate(name, counter, (k, v) => counter);
                }

                counter.Inc(value);
            }

            if (_settings.UseAppInsights && _telemetryClient != null)
            {
                var id = new MetricIdentifier(_settings.AppInsights.Namespace, name);
                if (!_aiMetrics.TryGetValue(id.MetricId, out var metric))
                {
                    metric = GetAppInsightsMetric(id, dimensions?.Select(p => p.Key).ToArray());
                    _aiMetrics.AddOrUpdate(id.MetricId, metric, (k, v) => metric);
                }

                TrackValue(metric, value, dimensions?.Select(p => p.Value).ToArray());
            }
        }

        private Metric GetAppInsightsMetric(MetricIdentifier id, params string[] dimensionNames)
        {
            if (dimensionNames == null)
                return _telemetryClient.GetMetric(id);
            switch (dimensionNames.Length)
            {
                case 1:
                    return _telemetryClient.GetMetric(id.MetricId, dimensionNames[0]);
                case 2:
                    return _telemetryClient.GetMetric(id.MetricId, dimensionNames[0], dimensionNames[1]);
                case 3:
                    return _telemetryClient.GetMetric(id.MetricId, dimensionNames[0], dimensionNames[1],
                        dimensionNames[2]);
                case 4:
                    return _telemetryClient.GetMetric(id.MetricId, dimensionNames[0], dimensionNames[1],
                        dimensionNames[2], dimensionNames[3]);
                default:
                    throw new Exception("Too many dimensions");
            }
        }

        private void TrackValue(Metric metric, double value, params string[] dimensionValues)
        {
            if (dimensionValues == null) metric.TrackValue((long) value);
            switch (dimensionValues.Length)
            {
                case 1:
                    metric.TrackValue((long) value, dimensionValues[0]);
                    break;
                case 2:
                    metric.TrackValue((long) value, dimensionValues[0], dimensionValues[1]);
                    break;
                case 3:
                    metric.TrackValue((long) value, dimensionValues[0], dimensionValues[1], dimensionValues[2]);
                    break;
                case 4:
                    metric.TrackValue((long) value, dimensionValues[0], dimensionValues[1], dimensionValues[2],
                        dimensionValues[3]);
                    break;
                default:
                    throw new Exception("Too many dimensions");
            }
        }
    }
}