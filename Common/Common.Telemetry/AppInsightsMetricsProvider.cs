// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppInsightsMetricsProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;

    /// <summary>
    ///     wrapper to metric provider
    /// </summary>
    public interface IMetricsProducer
    {
        MetricProviderType ProviderType { get; }
        void RecordMetric(string name, double value, params KeyValuePair<string, string>[] labels);
    }

    public enum MetricProviderType
    {
        ApplicationInsights,
        Stats,
        Geneva,
        Prometheus
    }

    public class AppInsightsMetricsProvider : IMetricsProducer
    {
        private readonly ConcurrentDictionary<string, Metric> _metrics = new ConcurrentDictionary<string, Metric>();

        public AppInsightsMetricsProvider(TelemetryClient client)
        {
            Client = client;
        }

        public TelemetryClient Client { get; }
        public MetricProviderType ProviderType => MetricProviderType.ApplicationInsights;

        public void RecordMetric(string name, double value, params KeyValuePair<string, string>[] labels)
        {
            Metric metric;
            if (_metrics.TryGetValue(name, out var m))
            {
                metric = m;
            }
            else
            {
                if (labels?.Length > 0)
                {
                    var labelNames = labels.Select(p => p.Key).ToList();
                    switch (labelNames.Count)
                    {
                        case 1:
                            metric = Client.GetMetric(name, labelNames[0]);
                            break;
                        case 2:
                            metric = Client.GetMetric(name, labelNames[0], labelNames[1]);
                            break;
                        case 3:
                            metric = Client.GetMetric(name, labelNames[0], labelNames[1], labelNames[2]);
                            break;
                        case 4:
                        default:
                            metric = Client.GetMetric(name, labelNames[0], labelNames[1], labelNames[2], labelNames[3]);
                            break;
                    }
                }
                else
                {
                    metric = Client.GetMetric(name);
                }

                _metrics.AddOrUpdate(name, metric, (k, v) => metric);
            }

            if (labels?.Length > 0)
            {
                var labelValues = labels.Select(p => p.Value).ToList();
                switch (labelValues.Count)
                {
                    case 1:
                        metric.TrackValue(value, labelValues[0]);
                        break;
                    case 2:
                        metric.TrackValue(value, labelValues[0], labelValues[1]);
                        break;
                    case 3:
                        metric.TrackValue(value, labelValues[0], labelValues[1], labelValues[2]);
                        break;
                    case 4:
                    default:
                        metric.TrackValue(value, labelValues[0], labelValues[1], labelValues[2], labelValues[3]);
                        break;
                }
            }
            else
            {
                metric.TrackValue(value);
            }
        }
    }
}