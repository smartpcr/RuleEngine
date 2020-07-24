﻿namespace Common.Telemetry.RuntimeTelemetry.Collectors
{
    using System;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// </summary>
    internal sealed class DotNetEventListener : EventListener
    {
        private readonly IEventSourceStatsCollector _collector;
        private readonly Action<Exception> _errorHandler;

        internal DotNetEventListener(IEventSourceStatsCollector collector, Action<Exception> errorHandler)
        {
            _collector = collector;
            _errorHandler = errorHandler;
            EnableEventSources(collector);
        }

        private void EnableEventSources(IEventSourceStatsCollector forCollector)
        {
            EventSourceCreated += (sender, e) =>
            {
                var es = e.EventSource;
                if (es.Guid == forCollector.EventSourceGuid)
                    EnableEvents(es, forCollector.Level, forCollector.Keywords);
            };
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            try
            {
                _collector.ProcessEvent(eventData);
            }
            catch (Exception e)
            {
                _errorHandler(e);
            }
        }
    }
}