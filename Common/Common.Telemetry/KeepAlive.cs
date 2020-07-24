// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeepAlive.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class KeepAlive : BackgroundService
    {
        private readonly ILogger<KeepAlive> log;
        private readonly IAppTelemetry metrics;

        public KeepAlive(ILogger<KeepAlive> log, IAppTelemetry metrics)
        {
            this.log = log;
            this.metrics = metrics;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                log.LogTrace("heartbeat");
                metrics.RecordMetric("Heartbeat", 1);

                var sleepSeconds = 15;

                while (sleepSeconds-- > 0 && !stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            log.LogInformation("KeepAlive stopped!");
        }
    }

    public static class HostExtensions
    {
        public static Task OnShutDown(this IHost host)
        {
            // allow telemetry to escape before we shutdown
            return Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}