﻿namespace Common.Telemetry.RuntimeTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Collectors;
    using Microsoft.ApplicationInsights;

    /// <summary>
    ///     Configures what .NET core runtime metrics will be collected.
    /// </summary>
    public static class DotNetRuntimeTelemetryBuilder
    {
        /// <summary>
        ///     Includes all available .NET runtime metrics by default. Call <see cref="Builder.StartCollecting" />
        ///     to begin collecting metrics.
        /// </summary>
        /// <returns></returns>
        public static Builder Default(TelemetryClient telemetry)
        {
            return Customize(telemetry)
                .WithContentionStats()
                .WithJitStats()
                .WithThreadPoolSchedulingStats()
                .WithThreadPoolStats()
                .WithGcStats();
        }

        /// <summary>
        ///     Allows you to customize the types of metrics collected.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     Include specific .NET runtime metrics by calling the WithXXX() methods and then call
        ///     <see cref="Builder.StartCollecting" />
        /// </remarks>
        public static Builder Customize(TelemetryClient telemetry)
        {
            return new Builder(telemetry);
        }

        public class Builder
        {
            private Action<Exception> _errorHandler;

            public Builder(TelemetryClient telemetry)
            {
                Telemetry = telemetry;
            }

            internal HashSet<IEventSourceStatsCollector> StatsCollectors { get; } =
                new HashSet<IEventSourceStatsCollector>(new TypeEquality<IEventSourceStatsCollector>());

            public TelemetryClient Telemetry { get; }

            /// <summary>
            ///     Finishes configuration and starts collecting .NET runtime metrics. Returns a <see cref="IDisposable" /> that
            ///     can be disposed of to stop metric collection.
            /// </summary>
            /// <returns></returns>
            public IDisposable StartCollecting()
            {
                if (DotNetRuntimeStatsCollector.Instance != null)
                    throw new InvalidOperationException(
                        ".NET runtime metrics are already being collected. Dispose() of your previous collector before calling this method again.");

                var runtimeStatsCollector =
                    new DotNetRuntimeStatsCollector(StatsCollectors.ToImmutableHashSet(), _errorHandler);
                runtimeStatsCollector.RegisterMetrics(Telemetry);
                return runtimeStatsCollector;
            }


            /// <summary>
            ///     Include metrics around the volume of work scheduled on the worker thread pool
            ///     and the scheduling delays.
            /// </summary>
            public Builder WithThreadPoolSchedulingStats()
            {
                StatsCollectors.Add(new ThreadPoolSchedulingStatsCollector());
                return this;
            }

            /// <summary>
            ///     Include metrics around the size of the worker and IO thread pools and reasons
            ///     for worker thread pool changes.
            /// </summary>
            public Builder WithThreadPoolStats()
            {
                StatsCollectors.Add(new ThreadPoolStatsCollector());
                return this;
            }

            /// <summary>
            ///     Include metrics around volume of locks contended.
            /// </summary>
            public Builder WithContentionStats()
            {
                StatsCollectors.Add(new ContentionStatsCollector());
                return this;
            }

            /// <summary>
            ///     Include metrics summarizing the volume of methods being compiled
            ///     by the Just-In-Time compiler.
            /// </summary>
            public Builder WithJitStats()
            {
                StatsCollectors.Add(new JitStatsCollector());
                return this;
            }

            /// <summary>
            ///     Include metrics recording the frequency and duration of garbage collections/ pauses, heap sizes and
            ///     volume of allocations.
            /// </summary>
            public Builder WithGcStats()
            {
                StatsCollectors.Add(new GcStatsCollector());
                return this;
            }

            public Builder WithCustomCollector(IEventSourceStatsCollector statsCollector)
            {
                StatsCollectors.Add(statsCollector);
                return this;
            }

            /// <summary>
            ///     Specifies a function to call when an exception occurs within the .NET stats collectors.
            ///     Only one error handler may be specified.
            /// </summary>
            /// <param name="handler"></param>
            /// <returns></returns>
            public Builder WithErrorHandler(Action<Exception> handler)
            {
                _errorHandler = handler;
                return this;
            }

            internal class TypeEquality<T> : IEqualityComparer<T>
            {
                public bool Equals(T x, T y)
                {
                    return x.GetType() == y.GetType();
                }

                public int GetHashCode(T obj)
                {
                    return obj.GetType().GetHashCode();
                }
            }
        }
    }
}