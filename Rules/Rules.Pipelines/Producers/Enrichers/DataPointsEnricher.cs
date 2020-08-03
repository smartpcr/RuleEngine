// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataPointsEnricher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers.Enrichers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Cache;
    using Common.Telemetry;
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.DataTypes;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DataPointsEnricher : IContextEnricher<PowerDevice>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly ILogger<DataPointsEnricher> logger;
        private readonly IKustoRepo<ZenonDataPoint> zenonDataPointRepo;
        private readonly IKustoRepo<ArgusEnabledDc> enabledDcRepo;
        private readonly object syncObj = new object();
        private string currentDcName;
        private Dictionary<string, List<ZenonDataPoint>> zenonDataPointsLookup;
        private List<string> enabledDcNames;

        private const string zenonDataPointQueryTemplate = @"
ZenonDataPoints
    | where DcName == ""{0}""";
        private const string enabledDcQuery = "ArgusEnabledDc";
        
        public string Name => nameof(DataPointsEnricher);
        public int ApplyOrder => 2;

        public DataPointsEnricher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DataPointsEnricher>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var kustoRepoFactory = serviceProvider.GetRequiredService<KustoRepoFactory>();
            zenonDataPointRepo = kustoRepoFactory.CreateRepository<ZenonDataPoint>();
            enabledDcRepo = kustoRepoFactory.CreateRepository<ArgusEnabledDc>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
        }
        
        
        public void EnsureLookup(EvaluationContext context, bool isLiveData = false, CancellationToken cancel = default)
        {
            if (isLiveData) return;
            
            var dcName = context.DcName;
            if (zenonDataPointsLookup == null || currentDcName != dcName)
            {
                lock (syncObj)
                {
                    if (zenonDataPointsLookup == null || currentDcName != dcName)
                    {
                        currentDcName = dcName;

                        enabledDcNames = cache.GetOrUpdateAsync(
                            "enabled-dcnames",
                            async () => await enabledDcRepo.GetLastModificationTime(enabledDcQuery, cancel),
                            async () =>
                            {
                                var dcs = await enabledDcRepo.ExecuteQuery(enabledDcQuery, (reader) => reader.GetString(0), cancel);
                                return dcs.ToList();
                            }, cancel).GetAwaiter().GetResult();
                        logger.LogInformation($"total of {enabledDcNames.Count} data centers are enabled for argus monitoring");

                        var kustoQuery = string.Format(zenonDataPointQueryTemplate, dcName);
                        var zenonDataPointList = cache.GetOrUpdateAsync(
                            $"list-{nameof(ZenonDataPoint)}-{dcName}",
                            async () => await zenonDataPointRepo.GetLastModificationTime(kustoQuery, cancel),
                            async () =>
                            {
                                var zenonDataPoints = await zenonDataPointRepo.Query(kustoQuery, cancel);
                                return zenonDataPoints.ToList();
                            }, cancel).GetAwaiter().GetResult();
                        logger.LogInformation($"total of {zenonDataPointList.Count} is retrieved for '{dcName}'");

                        zenonDataPointsLookup = zenonDataPointList.GroupBy(zdp => zdp.DeviceName)
                            .ToDictionary(g => g.Key, g => g.ToList());
                    }
                }
            }

            context.SetZenonDataPoints(zenonDataPointsLookup);
            context.SetEnabledDataCenters(enabledDcNames);
        }

        public void Enrich(EvaluationContext context, PowerDevice instance, bool isLiveData = false)
        {
            if (isLiveData) return;
            
            using var scope = appTelemetry.StartOperation(this);

            EnsureLookup(context);

            if (zenonDataPointsLookup.ContainsKey(instance.DeviceName))
            {
                instance.DataPoints = zenonDataPointsLookup[instance.DeviceName]
                    .Where(zdp => !zdp.FilterdOutInPG)
                    .Select(zdp => new DataPoint()
                    {
                        Name = zdp.DataPoint,
                        Channel = zdp.Channel,
                        Offset = zdp.Offset,
                        Priminitive = zdp.Primitive,
                        Scaling = zdp.Scaling,
                        ChannelType = zdp.ChannelType,
                        PollInterval = zdp.PollInterval,
                        FilterdOutInPG = zdp.FilterdOutInPG
                    }).ToList();
            }
            else
            {
                instance.DataPoints = new List<DataPoint>();
            }
        }

        #region dispose
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            zenonDataPointsLookup?.Clear();
            zenonDataPointsLookup = null;
        }

        ~DataPointsEnricher()
        {
            ReleaseUnmanagedResources();
        }
        #endregion
    }
}