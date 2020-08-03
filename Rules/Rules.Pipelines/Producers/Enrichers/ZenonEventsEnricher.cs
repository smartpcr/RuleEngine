// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZenonEventsEnricher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers.Enrichers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ZenonEventsEnricher : IContextEnricher<PowerDevice>
    {
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

        private readonly ILogger<ZenonEventsEnricher> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly IKustoRepo<ZenonEventStats> zenonRawRepo;
        private readonly IKustoRepo<ZenonLastReading> zenonLastReadingRepo;
        
        private string currentDcName;
        private readonly object syncZenonEvents = new object();
        private Dictionary<string, List<ZenonEventStats>> zenonEventLookups;
        private Dictionary<string, List<ZenonLastReading>> zenonLastReadings;

        public string Name => GetType().Name;
        public int ApplyOrder => 3;
        
        public ZenonEventsEnricher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ZenonEventsEnricher>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var kustoRepoFactory = serviceProvider.GetRequiredService<KustoRepoFactory>();
            zenonRawRepo = kustoRepoFactory.CreateRepository<ZenonEventStats>();
            zenonLastReadingRepo = kustoRepoFactory.CreateRepository<ZenonLastReading>();
        }
        
        public void EnsureLookup(EvaluationContext context, bool isLiveData = false, CancellationToken cancel = default)
        {
            if (!isLiveData) return;
            PopulateZenonEvents(context);
        }

        public void Enrich(EvaluationContext context, PowerDevice instance, bool isLiveData = false)
        {
            if (!isLiveData) return;
            
            using var scope = appTelemetry.StartOperation(this);
            EnsureLookup(context, true);

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

        private void PopulateZenonEvents(EvaluationContext context)
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

            context.SetZenonReadingStats(zenonEventLookups);
            context.SetLastZenonReadings(zenonLastReadings);
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

        ~ZenonEventsEnricher()
        {
            ReleaseUnmanagedResources();
        }
        #endregion
    }
}