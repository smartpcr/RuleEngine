namespace Common.Telemetry.RuntimeTelemetry.EventSources
{
    using System;

    /// <summary>
    ///     event source and keywords for framework metrics
    /// </summary>
    public class FrameworkEventSource
    {
        public static readonly Guid Id = WellKnownEventSources.FrameworkEventSource;

        [Flags]
        internal enum Keywords : long
        {
            Loader = 0x0001,
            ThreadPool = 0x0002,
            NetClient = 0x0004,
            DynamicTypeUsage = 0x0008,
            ThreadTransfer = 0x0010
        }
    }
}