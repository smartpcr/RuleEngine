namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Diagnostics.Tracing;
    using Microsoft.ApplicationInsights;

    public interface IEventSourceStatsCollector
    {
        Guid EventSourceGuid { get; }
        EventKeywords Keywords { get; }
        EventLevel Level { get; }
        void ProcessEvent(EventWrittenEventArgs e);
        void RegisterMetrics(TelemetryClient telemetry);
        void UpdateMetrics();
    }
}