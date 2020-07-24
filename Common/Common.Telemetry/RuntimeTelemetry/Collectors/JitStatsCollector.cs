namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Diagnostics.Tracing;
    using EventSources;
    using Microsoft.ApplicationInsights;
    using Util;

    /// <summary>
    ///     Measures the activity of the JIT (Just In Time) compiler in a process.
    ///     Tracks how often it runs and how long it takes to compile methods
    /// </summary>
    internal sealed class JitStatsCollector : IEventSourceStatsCollector
    {
        private const int EventIdMethodJittingStarted = 145, EventIdMethodLoadVerbose = 143;
        private const string DynamicLabel = "dynamic";
        private const string LabelValueTrue = "true";
        private const string LabelValueFalse = "false";

        private readonly EventPairTimer<ulong> _eventPairTimer = new EventPairTimer<ulong>(
            EventIdMethodJittingStarted,
            EventIdMethodLoadVerbose,
            x => (ulong) x.Payload[0]
        );

        private readonly Ratio _jitCpuRatio = Ratio.ProcessTotalCpu();

        internal Metric MethodsJittedTotal { get; private set; }
        internal Metric MethodsJittedSecondsTotal { get; private set; }
        internal Metric CpuRatio { get; private set; }

        public EventKeywords Keywords => (EventKeywords) DotNetRuntimeEventSource.Keywords.Jit;
        public EventLevel Level => EventLevel.Verbose;
        public Guid EventSourceGuid => DotNetRuntimeEventSource.Id;

        public void RegisterMetrics(TelemetryClient telemetry)
        {
            MethodsJittedTotal = telemetry.GetMetric("dotnet_jit_method_total", DynamicLabel);
            MethodsJittedSecondsTotal = telemetry.GetMetric("dotnet_jit_method_seconds_total", DynamicLabel);
            CpuRatio = telemetry.GetMetric("dotnet_jit_cpu_ratio");
        }

        public void UpdateMetrics()
        {
            CpuRatio.TrackValue(_jitCpuRatio.CalculateConsumedRatio(MethodsJittedSecondsTotal));
        }

        public void ProcessEvent(EventWrittenEventArgs e)
        {
            if (_eventPairTimer.TryGetEventPairDuration(e, out var duration))
            {
                // dynamic methods are of special interest to us- only a certain number of JIT'd dynamic methods
                // will be cached. Frequent use of dynamic can cause methods to be evicted from the cache and re-JIT'd
                var methodFlags = (uint) e.Payload[5];
                var dynamicLabelValue = (methodFlags & 0x1) == 0x1 ? LabelValueTrue : LabelValueFalse;

                MethodsJittedTotal.TrackValue(1, dynamicLabelValue);
                MethodsJittedSecondsTotal.TrackValue(duration.TotalSeconds, dynamicLabelValue);
            }
        }
    }
}