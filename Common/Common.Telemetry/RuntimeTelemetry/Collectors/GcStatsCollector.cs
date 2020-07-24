namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using EventSources;
    using Microsoft.ApplicationInsights;
    using Util;

    /// <summary>
    ///     Measures how the frequency and duration of garbage collections and volume of allocations. Includes information
    ///     such as the generation the collection is running for, what triggered the collection and the type of the collection.
    /// </summary>
    internal sealed class GcStatsCollector : IEventSourceStatsCollector
    {
        private const string
            LabelHeap = "gc_heap",
            LabelGeneration = "gc_generation",
            LabelReason = "gc_reason",
            LabelType = "gc_type";

        private const int
            EventIdGcStart = 1,
            EventIdGcStop = 2,
            EventIdSuspendEEStart = 9,
            EventIdRestartEEStop = 3,
            EventIdHeapStats = 4,
            EventIdAllocTick = 10;

        private readonly Ratio _gcCpuRatio = Ratio.ProcessTotalCpu();

        private readonly EventPairTimer<uint, GcData> _gcEventTimer = new EventPairTimer<uint, GcData>(
            EventIdGcStart,
            EventIdGcStop,
            x => (uint) x.Payload[0],
            x => new GcData((uint) x.Payload[1], (DotNetRuntimeEventSource.GCType) x.Payload[3]));

        private readonly EventPairTimer<int> _gcPauseEventTimer = new EventPairTimer<int>(
            EventIdSuspendEEStart,
            EventIdRestartEEStop,
            // Suspensions/ Resumptions are always done sequentially so there is no common value to match events on. Return a constant value as the event id.
            x => 1);

        private readonly Ratio _gcPauseRatio = Ratio.ProcessTime();

        private readonly Dictionary<DotNetRuntimeEventSource.GCReason, string> _gcReasonToLabels =
            LabelGenerator.MapEnumToLabelValues<DotNetRuntimeEventSource.GCReason>();

        internal Metric GcCollectionSeconds { get; private set; }
        internal Metric GcPauseSeconds { get; private set; }
        internal Metric GcCollectionReasons { get; private set; }
        internal Metric GcCpuRatio { get; private set; }
        internal Metric GcPauseRatio { get; private set; }
        internal Metric AllocatedBytes { get; private set; }
        internal Metric GcHeapSizeBytes { get; private set; }
        internal Metric GcNumPinnedObjects { get; private set; }
        internal Metric GcFinalizationQueueLength { get; private set; }

        public Guid EventSourceGuid => DotNetRuntimeEventSource.Id;
        public EventKeywords Keywords => (EventKeywords) DotNetRuntimeEventSource.Keywords.GC;
        public EventLevel Level => EventLevel.Verbose;

        public void RegisterMetrics(TelemetryClient telemetry)
        {
            GcCollectionSeconds = telemetry.GetMetric("dotnet_gc_collection_seconds", LabelGeneration, LabelType);
            GcPauseSeconds = telemetry.GetMetric("dotnet_gc_pause_seconds");
            GcCollectionReasons = telemetry.GetMetric("dotnet_gc_collection_reasons_total", LabelReason);
            GcCpuRatio = telemetry.GetMetric("dotnet_gc_cpu_ratio");
            GcPauseRatio = telemetry.GetMetric("dotnet_gc_pause_ratio");
            AllocatedBytes = telemetry.GetMetric("dotnet_gc_allocated_bytes_total", LabelHeap);
            GcHeapSizeBytes = telemetry.GetMetric("dotnet_gc_heap_size_bytes", LabelGeneration);
            GcNumPinnedObjects = telemetry.GetMetric("dotnet_gc_pinned_objects");
            GcFinalizationQueueLength = telemetry.GetMetric("dotnet_gc_finalization_queue_length");
        }

        public void UpdateMetrics()
        {
            GcCpuRatio.TrackValue(_gcCpuRatio.CalculateConsumedRatio(GcCollectionSeconds));
            GcPauseRatio.TrackValue(_gcPauseRatio.CalculateConsumedRatio(GcPauseSeconds));
        }

        public void ProcessEvent(EventWrittenEventArgs e)
        {
            if (e.EventId == EventIdAllocTick)
            {
                const uint lohHeapFlag = 0x1;
                var heapLabelValue = ((uint) e.Payload[1] & lohHeapFlag) == lohHeapFlag ? "loh" : "soh";
                AllocatedBytes.TrackValue(e.Payload[0], heapLabelValue);
                return;
            }

            if (e.EventId == EventIdHeapStats)
            {
                GcHeapSizeBytes.TrackValue(e.Payload[0], "0");
                GcHeapSizeBytes.TrackValue(e.Payload[2], "1");
                GcHeapSizeBytes.TrackValue(e.Payload[4], "2");
                GcHeapSizeBytes.TrackValue(e.Payload[6], "loh");

                GcFinalizationQueueLength.TrackValue((ulong) e.Payload[9]);
                GcNumPinnedObjects.TrackValue((uint) e.Payload[10]);
                return;
            }

            // flags representing the "Garbage Collection" + "Preparation for garbage collection" pause reasons
            const uint suspendGcReasons = 0x1 | 0x6;

            if (e.EventId == EventIdSuspendEEStart && ((uint) e.Payload[0] & suspendGcReasons) == 0)
                // Execution engine is pausing for a reason other than GC, discard event.
                return;

            if (_gcPauseEventTimer.TryGetEventPairDuration(e, out var pauseDuration))
            {
                GcPauseSeconds.TrackValue(pauseDuration.TotalSeconds);
                return;
            }

            if (e.EventId == EventIdGcStart)
            {
                var dimension = _gcReasonToLabels[(DotNetRuntimeEventSource.GCReason) e.Payload[2]];
                GcCollectionReasons.TrackValue(1, dimension);
            }

            if (_gcEventTimer.TryGetEventPairDuration(e, out var gcDuration, out var gcData))
                GcCollectionSeconds.TrackValue(gcDuration.TotalSeconds, gcData.GetGenerationToString(),
                    gcData.GetTypeToString());
        }

        private struct GcData
        {
            private static readonly Dictionary<DotNetRuntimeEventSource.GCType, string> GcTypeToLabels =
                LabelGenerator.MapEnumToLabelValues<DotNetRuntimeEventSource.GCType>();

            public GcData(uint generation, DotNetRuntimeEventSource.GCType type)
            {
                Generation = generation;
                Type = type;
            }

            public uint Generation { get; }
            public DotNetRuntimeEventSource.GCType Type { get; }

            public string GetTypeToString()
            {
                return GcTypeToLabels[Type];
            }

            public string GetGenerationToString()
            {
                if (Generation > 2) return "loh";

                return Generation.ToString();
            }
        }
    }
}