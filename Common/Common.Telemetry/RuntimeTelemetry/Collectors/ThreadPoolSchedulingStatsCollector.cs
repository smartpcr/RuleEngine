namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Diagnostics.Tracing;
    using EventSources;
    using Microsoft.ApplicationInsights;
    using Util;

    /// <summary>
    ///     Measures the volume of work scheduled on the thread pool and the delay between scheduling the work and it beginning
    ///     execution.
    /// </summary>
    internal sealed class ThreadPoolSchedulingStatsCollector : IEventSourceStatsCollector
    {
        private const int EventIdThreadPoolEnqueueWork = 30, EventIdThreadPoolDequeueWork = 31;

        private readonly EventPairTimer<long> _eventPairTimer = new EventPairTimer<long>(
            EventIdThreadPoolEnqueueWork,
            EventIdThreadPoolDequeueWork,
            x => (long) x.Payload[0]
        );

        internal Metric ScheduledCount { get; private set; }
        internal Metric ScheduleDelay { get; private set; }

        public EventKeywords Keywords => (EventKeywords) FrameworkEventSource.Keywords.ThreadPool;
        public EventLevel Level => EventLevel.Verbose;
        public Guid EventSourceGuid => FrameworkEventSource.Id;

        public void RegisterMetrics(TelemetryClient telemetry)
        {
            ScheduledCount = telemetry.GetMetric("dotnet_threadpool_scheduled_total");
            ScheduleDelay = telemetry.GetMetric("dotnet_threadpool_scheduling_delay_seconds");
        }

        public void UpdateMetrics()
        {
        }

        public void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdThreadPoolEnqueueWork) ScheduledCount.TrackValue(1);

            if (_eventPairTimer.TryGetEventPairDuration(e, out var duration))
                ScheduleDelay.TrackValue(duration.TotalSeconds);
        }
    }
}