﻿namespace Common.Telemetry.RuntimeTelemetry.Util
{
    using System;
    using System.Diagnostics;
    using Microsoft.ApplicationInsights;

    /// <summary>
    ///     Helps calculate the ratio of process resources consumed by some activity.
    /// </summary>
    public class Ratio
    {
        private readonly Func<TimeSpan> _getElapsedTime;
        private double _lastEventTotalSeconds;
        private TimeSpan _lastProcessTime;

        internal Ratio(Func<TimeSpan> getElapsedTime)
        {
            _getElapsedTime = getElapsedTime;
            _lastProcessTime = _getElapsedTime();
        }

        /// <summary>
        ///     Calculates the ratio of CPU time consumed by an activity.
        /// </summary>
        /// <returns></returns>
        public static Ratio ProcessTotalCpu()
        {
            return new Ratio(() => Process.GetCurrentProcess().TotalProcessorTime);
        }

        /// <summary>
        ///     Calculates the ratio of process time consumed by an activity.
        /// </summary>
        /// <returns></returns>
        public static Ratio ProcessTime()
        {
            var startTime = DateTime.UtcNow;
            return new Ratio(() => DateTime.UtcNow - startTime);
        }

        public double CalculateConsumedRatio(double eventsCpuTimeTotalSeconds)
        {
            var currentProcessTime = _getElapsedTime();
            var consumedProcessTime = currentProcessTime - _lastProcessTime;
            var eventsConsumedTimeSeconds = eventsCpuTimeTotalSeconds - _lastEventTotalSeconds;

            if (eventsConsumedTimeSeconds < 0.0)
                // In this case, the difference between our last observed events CPU time and the current events CPU time is negative.
                // This means that we are being passed a non-counting value (which the caller should not be doing).
                // Rather than throwing an exception which could jeopardize the stability of event collection, we'll return a zero
                // TODO re-visit this and consider how to notify the user this is occurring
                return 0.0;

            _lastProcessTime = currentProcessTime;
            _lastEventTotalSeconds = eventsCpuTimeTotalSeconds;

            if (consumedProcessTime == TimeSpan.Zero)
                // Avoid divide by zero
                return 0.0;
            return Math.Min(1.0, eventsConsumedTimeSeconds / consumedProcessTime.TotalSeconds);
        }

        public double CalculateConsumedRatio(Metric metric)
        {
            // not supported to retrieve past values from metric
            return 0;
        }
    }
}