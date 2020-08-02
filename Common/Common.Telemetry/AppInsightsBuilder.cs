namespace Common.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Config;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.GenevaAgent;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace.Configuration;
    using OpenTelemetry.Trace.Samplers;
    using Serilog;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public static class AppInsightsBuilder
    {
        public static IServiceCollection AddAppInsights(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var settings = configuration.GetConfiguredSettings<AppInsightsSettings>();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isProdEnv = string.IsNullOrEmpty(env) ||
                            env.Equals("prod", StringComparison.OrdinalIgnoreCase) ||
                            env.Equals("production", StringComparison.OrdinalIgnoreCase);
            var instrumentationKey = GetInstrumentationKey(settings);
            Console.WriteLine($"instrumentation key: {instrumentationKey}, env: {env}");

            if (serviceProvider.GetService<IHostingEnvironment>() == null)
                services.TryAddSingleton<IHostingEnvironment, SelfHostingEnvironment>();

            AddApplicationInsightsStorage(services, isProdEnv);

            if (settings.IsJob)
            {
                services.AddApplicationInsightsTelemetryWorkerService(o =>
                {
                    o.InstrumentationKey = instrumentationKey;
                    o.ApplicationVersion = settings.Version;
                    o.DeveloperMode = !isProdEnv;
                    o.EnableHeartbeat = true;
                    o.EnableDebugLogger = !isProdEnv;
                });
            }
            else
            {
                var options = new ApplicationInsightsServiceOptions
                {
                    InstrumentationKey = instrumentationKey,
                    DeveloperMode = !isProdEnv,
                    EnableDebugLogger = !isProdEnv,
                    AddAutoCollectedMetricExtractor = true,
                    EnableAdaptiveSampling = false
                };
                options.DependencyCollectionOptions.EnableLegacyCorrelationHeadersInjection = true;
                options.RequestCollectionOptions.InjectResponseHeaders = true;
                options.RequestCollectionOptions.TrackExceptions = true;
                services.AddApplicationInsightsTelemetry(options);
            }

            services.AddSingleton<ITelemetryInitializer, ContextTelemetryInitializer>();
            if (settings.Geneva?.UseGenevaSink == true)
            {
                AddGenevaSink(services, settings);
            }
            services.AddAppInsightsLogging();
            Console.WriteLine("Enabled app insights");

            // open telemetry
            if (settings.EnableTracing)
            {
                services.AddOpenTelemetry((sp, builder) =>
                {
                    builder.UseApplicationInsights(o =>
                    {
                        o.InstrumentationKey = instrumentationKey;
                        o.TelemetryInitializers.Add(
                            new ContextTelemetryInitializer(new OptionsWrapper<AppInsightsSettings>(settings)));
                    });

                    builder.SetSampler(new AlwaysSampleSampler())
                        .AddDependencyCollector(config => { config.SetHttpFlavor = true; })
                        .AddRequestCollector()
                        .SetResource(new Resource(new Dictionary<string, object>
                        {
                            {"service.name", settings.Role}
                        }));
                });

                Console.WriteLine("export open telemetry to app insights");
            }

            serviceProvider = services.BuildServiceProvider();
            services.TryAddSingleton<IAppTelemetry>(sp => new AppTelemetry(serviceProvider, configuration));
            Console.WriteLine("enabled app telemetry");

            return services;
        }

        private static void AddAppInsightsLogging(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var telemetryConfiguration = serviceProvider.GetRequiredService<TelemetryConfiguration>();
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                .CreateLogger();
            services.AddLogging(lb => lb.AddSerilog(logger));
        }

        private static string GetInstrumentationKey(AppInsightsSettings settings)
        {
            var instrumentationKey =
                Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY") ??
                settings.InstrumentationKey;

            if (string.IsNullOrEmpty(instrumentationKey))
                throw new InvalidOperationException("App insights instrumentation key is not configured");

            return instrumentationKey;
        }

        private static void AddApplicationInsightsStorage(IServiceCollection services, bool isProd)
        {
            var aiStorage = Path.Combine(Path.GetTempPath(), "appinsights-store");
            if (!Directory.Exists(aiStorage)) Directory.CreateDirectory(aiStorage);
            services.TryAddSingleton<ITelemetryChannel>(new ServerTelemetryChannel
            {
                StorageFolder = aiStorage,
                DeveloperMode = !isProd
            });
        }

        private static void AddGenevaSink(IServiceCollection services, AppInsightsSettings settings)
        {
            var config = new TelemetryConfiguration {InstrumentationKey = GetInstrumentationKey(settings)};

            TelemetryConfiguration genevaMdsdConfig = new TelemetryConfiguration(settings.Geneva.OneDSSinkChannel);
            TelemetrySink mdsdSink = new TelemetrySink(config, new GenevaAgentChannel()) { Name = settings.Geneva.OneDSSinkChannel };
            mdsdSink.Initialize(genevaMdsdConfig);
            config.TelemetrySinks.Add(mdsdSink);
            config.TelemetryProcessorChainBuilder.Use(next => new GenevaMetricsProcessor(next)
            {
                MetricAccountName = settings.Geneva.GenevaMetricsAccountName,
                MetricNamespace = settings.Geneva.GenevaMetricsNamespace
            });
            config.TelemetryProcessorChainBuilder.Build();
            Console.WriteLine($"Added geneva metrics sink, account={settings.Geneva.GenevaMetricsAccountName}, namespace={settings.Geneva.GenevaMetricsNamespace}");

            services.AddSingleton(config);
        }
    }
}