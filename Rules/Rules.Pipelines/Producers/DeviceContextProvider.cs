// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceContextProvider.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
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
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Pipelines;

    public class DeviceContextProvider : IContextProvider<PowerDevice>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly IList<IContextEnricher<PowerDevice>> enrichers;
        private readonly ILogger<DeviceContextProvider> logger;
        private readonly IDocDbRepository<PowerDevice> powerDeviceRepo;
        private readonly PipelineSettings pipelineSettings;

        public DeviceContextProvider(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DeviceContextProvider>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            powerDeviceRepo = repoFactory.CreateRepository<PowerDevice>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            enrichers = serviceProvider.GetServices<IContextEnricher<PowerDevice>>().OrderBy(er => er.ApplyOrder).ToList();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            pipelineSettings = config.GetConfiguredSettings<PipelineSettings>();
        }

        public IEnumerable<IContextEnricher<PowerDevice>> ContextEnrichers => enrichers;

        public async Task<IEnumerable<PowerDevice>> Provide(
            PipelineExecutionContext context,
            ContextProviderScope providerScope,
            List<string> filterValues,
            CancellationToken cancellationToken)
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
            switch (providerScope)
            {
                case ContextProviderScope.Device:
                    dcName = filterValues[0].Split(new[] {'-'})[0];
                    break;
                default:
                    dcName = filterValues[0];
                    break;
            }

            var deviceList = await cache.GetOrUpdateAsync(
                cacheKey,
                async () => await powerDeviceRepo.GetLastModificationTime(null, cancellationToken),
                async () =>
                {
                    IEnumerable<PowerDevice> devices;
                    switch (providerScope)
                    {
                        case ContextProviderScope.Device:
                            devices = await powerDeviceRepo.Query("c.deviceName in ({0})", filterValues);
                            break;
                        default:
                            devices = await powerDeviceRepo.Query($"c.dcName='{dcName}'");
                            break;
                    }
                    return devices.ToList();
                },
                cancellationToken);
            logger.LogInformation($"Total of {deviceList.Count} power devices found for dc: {dcName}");

            if (enrichers.Any())
            {
                foreach (var enricher in enrichers)
                {
                    logger.LogInformation($"ensure lookup, {enricher.ApplyOrder}: {enricher.Name}");
                    enricher.EnsureLookup(context);
                    enricher.EnsureLiveData(context);
                }

                var cacheKeyForEnrichedDeviceList = $"{cacheKey}-enriched";
                var enrichedDevices = await cache.GetOrUpdateAsync(
                    cacheKeyForEnrichedDeviceList,
                    async () => await powerDeviceRepo.GetLastModificationTime(null, cancellationToken),
                    async () =>
                    {
                        foreach (var enricher in enrichers)
                        {
                            logger.LogInformation($"applying enrichment {enricher.ApplyOrder}: {enricher.Name}");
                            var totalEnriched = 0;
                            var watch = Stopwatch.StartNew();

                            // DO NOT use Parallel.ForEach within async block
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
                            }, pipelineSettings.MaxParallelism, cancellationToken);

                            watch.Stop();
                            logger.LogInformation($"Enrichment is done, took {watch.Elapsed}");
                        }

                        return deviceList;
                    },
                    cancellationToken);

                foreach (var enricher in enrichers)
                {
                    logger.LogInformation($"applying live data enrichment {enricher.ApplyOrder}: {enricher.Name}");
                    var totalEnriched = 0;
                    var watch = Stopwatch.StartNew();

                    // DO NOT use Parallel.ForEach within async block
                    await enrichedDevices.ParallelForEachAsync(device =>
                    {
                        enricher.EnrichLiveData(context, device);
                        Interlocked.Increment(ref totalEnriched);
                        if (totalEnriched % 100 == 0)
                        {
                            logger.LogInformation(
                                $"{enricher.Name}: enriched {totalEnriched} of {deviceList.Count}...");
                        }

                        return Task.CompletedTask;
                    }, pipelineSettings.MaxParallelism, cancellationToken);

                    watch.Stop();
                    logger.LogInformation($"Live data enrichment is done, took {watch.Elapsed}");
                }

                return enrichedDevices;
            }

            return deviceList;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            if (enrichers?.Any() == true)
                foreach (var enricher in enrichers)
                    enricher.Dispose();
        }

        ~DeviceContextProvider()
        {
            ReleaseUnmanagedResources();
        }
    }
}