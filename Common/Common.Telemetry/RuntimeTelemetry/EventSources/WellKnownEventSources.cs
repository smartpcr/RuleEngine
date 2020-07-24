namespace Common.Telemetry.RuntimeTelemetry.EventSources
{
    using System;

    /// <summary>
    ///     wellknown event source ids
    /// </summary>
    public static class WellKnownEventSources
    {
        public static Guid DotNetRuntime => Guid.Parse("5e5bb766-bbfc-5662-0548-1d44fad9bb56");
        public static Guid FrameworkEventSource => Guid.Parse("8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1");
        public static Guid ConcurrentCollectionsEventSource => Guid.Parse("35167F8E-49B2-4b96-AB86-435B59336B5E");
        public static Guid SynchronizationEventSource => Guid.Parse("EC631D38-466B-4290-9306-834971BA0217");
        public static Guid TplEventSource => Guid.Parse("2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5");
        public static Guid PlinqEventSource => Guid.Parse("159eeeec-4a14-4418-a8fe-faabcd987887");
        public static Guid AspNetEventSource => Guid.Parse("ee799f41-cfa5-550b-bf2c-344747c1c668");
    }
}