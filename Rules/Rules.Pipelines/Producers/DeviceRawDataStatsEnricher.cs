//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DeviceRawDataStatsEnricher.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Cache;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Traversals;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;

    public class DeviceRawDataStatsEnricher : IContextEnricher<PowerDevice>
    {
        private readonly ILoggerFactory loggerFactory;

        private const string zenonQueryTemplate = @"
let datacenterName = '{0}';
let devicesForDatacenter = materialize(cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ElectricalDevices_v3
| summarize IngestionTimestamp = max(IngestionTimestamp) by DcName
| where DcName == datacenterName
| join cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ElectricalDevices_v3
    on      $left.DcName == $right.DcName
    and     $left.IngestionTimestamp == $right.IngestionTimestamp
| join kind=leftouter cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ElectricalDevicePaths_v3
    on      $left.DeviceId == $right.DeviceId
    and     $left.IngestionTimestamp == $right.IngestionTimestamp);
let zenonEvents = cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ZenonEvent
| where env_time > ago(1h)
| where DataCenterName == datacenterName
| summarize
    Count=count(),
    Avg=avg(Value),
    Max=max(Value),
    Min=min(Value),
    MaxEventTime=max(env_time),
    MinEventTime=min(env_time),
    MaxPolledTime=max(EventPolledTimeStamp),
    MinPolledTime=min(EventPolledTimeStamp)
    by DataCenterName, DeviceName, DataPoint, ChannelType, Channel;
zenonEvents
| join kind=leftouter devicesForDatacenter on $left.DeviceName==$right.Name
| extend Rating = iff(ChannelType == 'Amps', AmpRating, iff(ChannelType == 'Volt', VoltageRating, iff(ChannelType == 'Power', KwRating, toreal(0))))
| project DcName, DeviceName, HierarchyId, DataPoint=substring(DataPoint, indexof(DataPoint,'.')+1), ChannelType, Channel, MaxEventTime, MinEventTime, MaxPolledTime, MinPolledTime, Avg, Max, Min, Count, Rating
| order by DcName asc, DeviceName asc, DataPoint asc";

        private const string zenonLastReadTemplate = @"
let datacenterName = '{0}';
let devicesForDatacenter = materialize(cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ElectricalDevices_v3
| summarize IngestionTimestamp = max(IngestionTimestamp) by DcName
| where DcName == datacenterName
| join cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ElectricalDevices_v3
    on      $left.DcName == $right.DcName
    and     $left.IngestionTimestamp == $right.IngestionTimestamp
| join kind=leftouter cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ElectricalDevicePaths_v3
    on      $left.DeviceId == $right.DeviceId
    and     $left.IngestionTimestamp == $right.IngestionTimestamp);
let zenonEvents = cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ZenonEvent
| where env_time > ago(1h)
| where DataCenterName == datacenterName
| summarize arg_max(env_time, *) by DataCenterName, DeviceName, DataPoint
| join kind=leftouter Zenon_EventStatus_QualityAnalysis on $left.EventStatus==$right.Code
| project DcName = DataCenterName, DeviceName, DataPoint, ChannelType, Channel, Value, EventTime = env_time, PolledTime = EventPolledTimeStamp, Quality;
zenonEvents
| join kind=leftouter devicesForDatacenter on $left.DeviceName==$right.Name
| extend Rating = iff(ChannelType == 'Amps', AmpRating, iff(ChannelType == 'Volt', VoltageRating, iff(ChannelType == 'Power', KwRating, toreal(0))))
| project DcName, DeviceName, HierarchyId, DataPoint=substring(DataPoint, indexof(DataPoint,'.')+1), ChannelType, Channel, EventTime, PolledTime, Value, Quality, Rating
| order by DcName asc, DeviceName asc, DataPoint asc";

        private readonly ILogger<DeviceRawDataStatsEnricher> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly IDocDbRepository<DeviceRelation> deviceRelationRepo;

        private readonly IDocDbRepository<PowerDevice> powerDeviceRepo;
        private readonly object syncDeviceList = new object();
        private readonly object syncZenonEvents = new object();
        private readonly IKustoRepo<ZenonEventStats> zenonRawRepo;
        private readonly IKustoRepo<ZenonLastReading> zenonLastReadingRepo;
        private string currentDcName;

        private Dictionary<string, PowerDevice> deviceLookups;
        private Dictionary<string, List<DeviceRelation>> relationLookup;
        private Dictionary<string, List<ZenonEventStats>> zenonEventLookups;
        private Dictionary<string, List<ZenonLastReading>> zenonLastReadings;

        public DeviceRawDataStatsEnricher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<DeviceRawDataStatsEnricher>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var kustoRepoFactory = serviceProvider.GetRequiredService<KustoRepoFactory>();
            zenonRawRepo = kustoRepoFactory.CreateRepository<ZenonEventStats>();
            zenonLastReadingRepo = kustoRepoFactory.CreateRepository<ZenonLastReading>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            powerDeviceRepo = repoFactory.CreateRepository<PowerDevice>();
            deviceRelationRepo = repoFactory.CreateRepository<DeviceRelation>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
        }

        public string Name => GetType().Name;
        public int ApplyOrder => 3;

        public void Enrich(PipelineExecutionContext context, PowerDevice instance)
        {
        }

        public void EnrichLiveData(PipelineExecutionContext context, PowerDevice instance)
        {
            using var scope = appTelemetry.StartOperation(this);

            EnsureLiveData(context);

            instance.ReadingStats = zenonEventLookups.ContainsKey(instance.DeviceName)
                ? zenonEventLookups[instance.DeviceName]
                : new List<ZenonEventStats>();
            instance.LastReadings = zenonLastReadings.ContainsKey(instance.DeviceName)
                ? zenonLastReadings[instance.DeviceName]
                : new List<ZenonLastReading>();

            if (instance.Children?.Any() == true)
            {
                foreach (var childDevice in instance.Children)
                {
                    childDevice.ReadingStats = zenonEventLookups.ContainsKey(childDevice.DeviceName)
                        ? zenonEventLookups[childDevice.DeviceName]
                        : new List<ZenonEventStats>();
                    childDevice.LastReadings = zenonLastReadings.ContainsKey(childDevice.DeviceName)
                        ? zenonLastReadings[childDevice.DeviceName]
                        : new List<ZenonLastReading>();
                }
            }

            if (instance.PrimaryParentDevice != null)
            {
                instance.PrimaryParentDevice.ReadingStats = zenonEventLookups.ContainsKey(instance.PrimaryParentDevice.DeviceName)
                    ? zenonEventLookups[instance.PrimaryParentDevice.DeviceName]
                    : new List<ZenonEventStats>();
                instance.PrimaryParentDevice.LastReadings = zenonLastReadings.ContainsKey(instance.PrimaryParentDevice.DeviceName)
                    ? zenonLastReadings[instance.PrimaryParentDevice.DeviceName]
                    : new List<ZenonLastReading>();
            }

            if (instance.RedundantDevice != null)
            {
                instance.RedundantDevice.ReadingStats = zenonEventLookups.ContainsKey(instance.RedundantDevice.DeviceName)
                    ? zenonEventLookups[instance.RedundantDevice.DeviceName]
                    : new List<ZenonEventStats>();
                instance.RedundantDevice.LastReadings = zenonLastReadings.ContainsKey(instance.RedundantDevice.DeviceName)
                    ? zenonLastReadings[instance.RedundantDevice.DeviceName]
                    : new List<ZenonLastReading>();
            }
        }

        public void EnsureLookup(PipelineExecutionContext context)
        {
            PopulateDeviceRelations(context);
        }

        public void EnsureLiveData(PipelineExecutionContext context)
        {
            PopulateZenonEvents(context);
        }

        private void PopulateDeviceRelations(PipelineExecutionContext context)
        {
            var dcName = context.DcName;
            if (deviceLookups == null || currentDcName != dcName)
            {
                lock (syncDeviceList)
                {
                    if (deviceLookups == null || currentDcName != dcName)
                    {
                        var cancel = new CancellationToken();
                        try
                        {
                            var cacheKey = $"{nameof(PowerDevice)}-list-{dcName}";
                            logger.LogInformation($"retrieving device list: {dcName}");
                            var deviceList = cache.GetOrUpdateAsync(
                                cacheKey,
                                async () => await powerDeviceRepo.GetLastModificationTime("", cancel),
                                async () =>
                                {
                                    var devices = await powerDeviceRepo.Query($"c.dcName = '{dcName}'");
                                    return devices.ToList();
                                },
                                cancel).GetAwaiter().GetResult();
                            logger.LogInformation($"Total of {deviceList.Count} power devices found for dc: {dcName}");

                            logger.LogInformation($"retrieving device relation: {dcName}");
                            var relationList = cache.GetOrUpdateAsync(
                                $"{nameof(DeviceRelation)}-list-{dcName}",
                                async () => await deviceRelationRepo.GetLastModificationTime($"c.dcName = '{dcName}'", cancel),
                                async () =>
                                {
                                    var deviceRelations = await deviceRelationRepo.Query($"c.dcName = '{dcName}'");
                                    return deviceRelations.ToList();
                                }, cancel).GetAwaiter().GetResult();
                            logger.LogInformation($"total of {relationList.Count} associations found for dc: {dcName}");

                            relationLookup = relationList.GroupBy(dr => dr.Name).ToDictionary(g => g.Key, g => g.ToList());
                            foreach (var powerDevice in deviceList)
                            {
                                var relations = relationLookup.ContainsKey(powerDevice.DeviceName)
                                    ? relationLookup[powerDevice.DeviceName]
                                    : null;
                                if (relations != null)
                                {
                                    powerDevice.DirectUpstreamDeviceList = relations
                                        .Where(r => r.DirectUpstreamDeviceList != null)
                                        .SelectMany(r => r.DirectUpstreamDeviceList).ToList();
                                    powerDevice.DirectDownstreamDeviceList = relations
                                        .Where(r => r.DirectDownstreamDeviceList != null)
                                        .SelectMany(r => r.DirectDownstreamDeviceList).ToList();
                                }
                                else
                                {
                                    powerDevice.DirectDownstreamDeviceList = new List<DeviceAssociation>();
                                    powerDevice.DirectUpstreamDeviceList = new List<DeviceAssociation>();
                                }
                            }

                            deviceLookups = deviceList.ToDictionary(d => d.DeviceName);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to populate device relations");
                            throw;
                        }
                    }
                }
            }

            context.DeviceLookup = deviceLookups;
            context.RelationLookup = relationLookup;
            context.DeviceTraversal = new DeviceHierarchyDeviceTraversal(deviceLookups, relationLookup, loggerFactory);
        }

        private void PopulateZenonEvents(PipelineExecutionContext context)
        {
            var dcName = context.DcName;
            if (zenonEventLookups == null || currentDcName != dcName)
            {
                lock (syncZenonEvents)
                {
                    if (zenonEventLookups == null || currentDcName != dcName)
                    {
                        currentDcName = dcName;
                        var zenonStatsQuery = string.Format(zenonQueryTemplate, dcName);
                        logger.LogInformation($"retrieving zenon event stats, dcName: {dcName}");
                        var eventStats = zenonRawRepo.Query(zenonStatsQuery, default).GetAwaiter().GetResult();
                        var zenonEventStats = eventStats.ToList();
                        logger.LogInformation($"Total of {zenonEventStats.Count} power devices retrieved for dc: {dcName}");
                        zenonEventLookups = zenonEventStats.GroupBy(e => e.DeviceName).ToDictionary(g => g.Key, g => g.ToList());

                        var zenonLastReadingQuery = string.Format(zenonLastReadTemplate, dcName);
                        logger.LogInformation($"retrieving zenon last readings, dcName: {dcName}");
                        var lastReadings = zenonLastReadingRepo.Query(zenonLastReadingQuery, default).GetAwaiter().GetResult();
                        var lastReadingList = lastReadings.ToList();
                        logger.LogInformation($"Total of {lastReadingList.Count} last readings retrieved for dc: {dcName}");
                        zenonLastReadings = lastReadingList.GroupBy(e => e.DeviceName).ToDictionary(g => g.Key, g => g.ToList());
                    }
                }
            }

            context.ZenonStatsLookup = zenonEventLookups;
            context.ZenonLastReadingLookup = zenonLastReadings;
        }


        #region dispose

        private void ReleaseUnmanagedResources()
        {
            zenonEventLookups?.Clear();
            zenonEventLookups = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~DeviceRawDataStatsEnricher()
        {
            ReleaseUnmanagedResources();
        }

        #endregion
    }
}