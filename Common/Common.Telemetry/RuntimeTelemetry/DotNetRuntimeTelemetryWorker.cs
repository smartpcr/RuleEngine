namespace Common.Instrumentation.RuntimeTelemetry
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Telemetry.RuntimeTelemetry;

    /// <summary>
    /// </summary>
    public class DotNetRuntimeTelemetryWorker : BackgroundService, IDisposable
    {
        public DotNetRuntimeTelemetryWorker(
            ILogger<DotNetRuntimeTelemetryWorker> logger,
            TelemetryClient telemetry)
        {
            Logger = logger;
            Telemetry = telemetry;
        }

        public ILogger<DotNetRuntimeTelemetryWorker> Logger { get; }
        public TelemetryClient Telemetry { get; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation(".net runtime stats collector started...");
            DotNetRuntimeTelemetryBuilder.Default(Telemetry).WithErrorHandler(e =>
            {
                Console.WriteLine(e.ToString());
                Telemetry.TrackException(e);
            }).StartCollecting();
            return Task.CompletedTask;
        }
    }
}