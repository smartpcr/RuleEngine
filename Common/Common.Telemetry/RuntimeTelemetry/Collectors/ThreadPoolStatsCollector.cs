namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using EventSources;
    using Microsoft.ApplicationInsights;
    using Util;

    /// <summary>
    ///     Measures the size of the worker + IO thread pools, worker pool throughput and reasons for worker pool
    ///     adjustments.
    /// </summary>
    public class ThreadPoolStatsCollector : IEventSourceStatsCollector
    {
        private const int
            EventIdThreadPoolSample = 54,
            EventIdThreadPoolAdjustment = 55,
            EventIdIoThreadCreate = 44,
            EventIdIoThreadRetire = 46,
            EventIdIoThreadUnretire = 47,
            EventIdIoThreadTerminate = 45;

        private readonly Dictionary<DotNetRuntimeEventSource.ThreadAdjustmentReason, string> _adjustmentReasonToLabel =
            LabelGenerator.MapEnumToLabelValues<DotNetRuntimeEventSource.ThreadAdjustmentReason>();

        internal Metric NumThreads { get; private set; }
        internal Metric NumIocThreads { get; private set; }

        // TODO resolve issue where throughput cannot be calculated (stats event is giving garbage values)
        // internal Counter Throughput { get; private set; }
        internal Metric AdjustmentsTotal { get; private set; }

        public Guid EventSourceGuid => DotNetRuntimeEventSource.Id;
        public EventKeywords Keywords => (EventKeywords) DotNetRuntimeEventSource.Keywords.Threading;
        public EventLevel Level => EventLevel.Informational;

        public void RegisterMetrics(TelemetryClient telemetry)
        {
            NumThreads = telemetry.GetMetric("dotnet_threadpool_num_threads");
            NumIocThreads = telemetry.GetMetric("dotnet_threadpool_io_num_threads");
            // Throughput = metrics.CreateCounter("dotnet_threadpool_throughput_total", "The total number of work items that have finished execution in the thread pool");
            AdjustmentsTotal = telemetry.GetMetric("dotnet_threadpool_adjustments_total", "adjustment_reason");
        }

        public void UpdateMetrics()
        {
        }

        public void ProcessEvent(EventWrittenEventArgs e)
        {
            switch (e.EventId)
            {
                case EventIdThreadPoolSample:
                    // Throughput.Inc((double) e.Payload[0]);
                    return;

                case EventIdThreadPoolAdjustment:
                    NumThreads.TrackValue((uint) e.Payload[1]);
                    AdjustmentsTotal.TrackValue(1,
                        _adjustmentReasonToLabel[(DotNetRuntimeEventSource.ThreadAdjustmentReason) e.Payload[2]]);
                    return;

                case EventIdIoThreadCreate:
                case EventIdIoThreadRetire:
                case EventIdIoThreadUnretire:
                case EventIdIoThreadTerminate:
                    NumIocThreads.TrackValue((uint) e.Payload[0]);
                    return;
            }
        }
    }
}