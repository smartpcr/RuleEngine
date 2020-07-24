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
    using Microsoft.Extensions.Logging.ApplicationInsights;
    using Microsoft.Extensions.Options;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace.Configuration;
    using OpenTelemetry.Trace.Samplers;

    public static class AppInsightsBuilder
    {
        public static IServiceCollection AddAppInsights(
            this IServiceCollection services,
            IConfiguration configuration,
            ILogger logger)
        {
            var settings = configuration.GetConfiguredSettings<AppInsightsSettings>();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isProdEnv = string.IsNullOrEmpty(env) ||
                            env.Equals("prod", StringComparison.OrdinalIgnoreCase) ||
                            env.Equals("production", StringComparison.OrdinalIgnoreCase);
            var instrumentationKey = GetInstrumentationKey(settings);
            logger?.LogInformation($"instrumentation key: {instrumentationKey}, env: {env}");

            var serviceProvider = services.BuildServiceProvider();
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
            services.AddAppInsightsLogging(configuration, settings);
            logger?.LogInformation("Enabled app insights");

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

                logger?.LogInformation("export open telemetry to app insights");
            }

            serviceProvider = services.BuildServiceProvider();
            services.TryAddSingleton<IAppTelemetry>(sp => new AppTelemetry(serviceProvider, configuration));
            logger?.LogInformation("enabled app telemetry");

            return services;
        }

        private static void AddAppInsightsLogging(
            this IServiceCollection services,
            IConfiguration configuration,
            AppInsightsSettings settings)
        {
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddApplicationInsights(GetInstrumentationKey(settings));
                builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
            });
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
    }
}