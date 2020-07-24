namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// </summary>
    internal sealed class DotNetRuntimeStatsCollector : IDisposable
    {
        private readonly Action<Exception> _errorHandler;
        private readonly ImmutableHashSet<IEventSourceStatsCollector> _statsCollectors;
        private DotNetEventListener[] _eventListeners;

        internal DotNetRuntimeStatsCollector(ImmutableHashSet<IEventSourceStatsCollector> statsCollectors,
            Action<Exception> errorHandler)
        {
            _statsCollectors = statsCollectors;
            _errorHandler = errorHandler ?? (e => { });
            Instance = this;
        }

        internal static DotNetRuntimeStatsCollector Instance { get; private set; }

        public void Dispose()
        {
            try
            {
                if (_eventListeners == null)
                    return;

                foreach (var listener in _eventListeners)
                    listener?.Dispose();
            }
            finally
            {
                Instance = null;
            }
        }

        public void RegisterMetrics(TelemetryClient telemetry)
        {
            foreach (var sc in _statsCollectors) sc.RegisterMetrics(telemetry);

            // Metrics have been registered, start the event listeners
            _eventListeners = _statsCollectors
                .Select(sc => new DotNetEventListener(sc, _errorHandler))
                .ToArray();
        }

        public void UpdateMetrics()
        {
            foreach (var sc in _statsCollectors)
                try
                {
                    sc.UpdateMetrics();
                }
                catch (Exception e)
                {
                    _errorHandler(e);
                }
        }
    }
}