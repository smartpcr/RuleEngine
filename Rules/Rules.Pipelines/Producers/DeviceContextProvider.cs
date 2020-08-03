// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceContextProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Cache;
    using Common.Config;
    using Common.Telemetry;
    using Dasync.Collections;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Pipelines.Producers.Enrichers;

    public class DeviceContextProvider : IContextProvider<PowerDevice>
    {
        private readonly ILogger<DeviceContextProvider> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly IDocDbRepository<PowerDevice> deviceRepo;
        private readonly IList<IContextEnricher<PowerDevice>> deviceEnrichers;
        private readonly PipelineSettings pipelineSettings;

        public IEnumerable<IContextEnricher<PowerDevice>> ContextEnrichers => deviceEnrichers;
        
        public DeviceContextProvider(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DeviceContextProvider>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            deviceRepo = repoFactory.CreateRepository<PowerDevice>();
            deviceEnrichers = serviceProvider.GetServices<IContextEnricher<PowerDevice>>().OrderBy(er => er.ApplyOrder).ToList();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            pipelineSettings = config.GetConfiguredSettings<PipelineSettings>();
        }

        public async Task<IEnumerable<PowerDevice>> Provide(EvaluationContext context, ValidationContextScope contextScope, List<string> filterValues, CancellationToken cancel)
        {
            if (filterValues == null || filterValues.Count == 0)
            {
                throw new InvalidOperationException("filter values is empty");
            }

            var cacheKey = filterValues.Count == 1
                ? $"list-{nameof(PowerDevice)}-{filterValues[0]}"
                : $"list-{nameof(PowerDevice)}-{string.Join(",", filterValues).GetHashCode()}";

            using var scope = appTelemetry.StartOperation(this);
            string dcName;
            switch (contextScope)
            {
                case ValidationContextScope.Device:
                    dcName = filterValues[0].Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries)[0];
                    break;
                default:
                    dcName = filterValues[0];
                    break;
            }

            var deviceList = await cache.GetOrUpdateAsync(
                cacheKey,
                async () => await deviceRepo.GetLastModificationTime(null, cancel),
                async () =>
                {
                    IEnumerable<PowerDevice> devices;
                    switch (contextScope)
                    {
                        case ValidationContextScope.Device:
                            devices = await deviceRepo.Query("c.deviceName in ({0})", filterValues);
                            break;
                        case ValidationContextScope.DC:
                            devices = await deviceRepo.Query($"c.dcName='{dcName}'");
                            break;
                        default:
                            throw new NotSupportedException($"validation context scope '{contextScope}' for device is not supported");
                    }
                    return devices.ToList();
                },
                cancel);
            logger.LogInformation($"Total of {deviceList.Count} power devices found for dc: {dcName}");
            
            if (deviceEnrichers.Any())
            {
                foreach (var enricher in deviceEnrichers)
                {
                    logger.LogInformation($"ensure lookup, {enricher.ApplyOrder}: {enricher.Name}");
                    enricher.EnsureLookup(context, false, cancel);
                    enricher.EnsureLookup(context, true, cancel);
                }

                var cacheKeyForEnrichedDeviceList = $"{cacheKey}-enriched";
                var enrichedDevices = await cache.GetOrUpdateAsync(
                    cacheKeyForEnrichedDeviceList,
                    async () => await deviceRepo.GetLastModificationTime(null, cancel),
                    async () =>
                    {
                        foreach (var enricher in deviceEnrichers)
                        {
                            logger.LogInformation($"applying enrichment {enricher.ApplyOrder}: {enricher.Name}");
                            var totalEnriched = 0;
                            var watch = Stopwatch.StartNew();
                            await deviceList.ParallelForEachAsync(device =>
                            {
                                enricher.Enrich(context, device);
                                Interlocked.Increment(ref totalEnriched);
                                if (totalEnriched % 100 == 0)
                                {
                                    logger.LogInformation(
                                        $"{enricher.Name}: enriched {totalEnriched} of {deviceList.Count}...");
                                }

                                return Task.CompletedTask;
                            }, pipelineSettings.MaxParallelism, cancel);

                            watch.Stop();
                            logger.LogInformation($"Enrichment is done, took {watch.Elapsed}");
                        }

                        return deviceList;
                    },
                    cancel);

                foreach (var enricher in deviceEnrichers)
                {
                    logger.LogInformation($"applying live data enrichment {enricher.ApplyOrder}: {enricher.Name}");
                    var totalEnriched = 0;
                    var watch = Stopwatch.StartNew();
                    await enrichedDevices.ParallelForEachAsync(device =>
                    {
                        enricher.Enrich(context, device, true);
                        Interlocked.Increment(ref totalEnriched);
                        if (totalEnriched % 100 == 0)
                        {
                            logger.LogInformation(
                                $"{enricher.Name}: enriched {totalEnriched} of {deviceList.Count}...");
                        }

                        return Task.CompletedTask;
                    }, pipelineSettings.MaxParallelism, cancel);

                    watch.Stop();
                    logger.LogInformation($"Live data enrichment is done, took {watch.Elapsed}");
                }

                return enrichedDevices;
            }

            return deviceList;
        }
        
        #region dispose
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            if (deviceEnrichers?.Any() == true)
                foreach (var enricher in deviceEnrichers)
                    enricher.Dispose();
        }

        ~DeviceContextProvider()
        {
            ReleaseUnmanagedResources();
        }
        #endregion
    }
}