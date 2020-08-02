// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceDataPointEnricher.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Cache;
    using Common.Telemetry;
    using DataCenterHealth.Entities.Devices;
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;
    using PowerDevice = DataCenterHealth.Models.Devices.PowerDevice;

    public class DeviceDataPointEnricher : IContextEnricher<PowerDevice>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly ILogger<DeviceDataPointEnricher> logger;
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

        public DeviceDataPointEnricher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DeviceDataPointEnricher>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var kustoRepoFactory = serviceProvider.GetRequiredService<KustoRepoFactory>();
            zenonDataPointRepo = kustoRepoFactory.CreateRepository<ZenonDataPoint>();
            enabledDcRepo = kustoRepoFactory.CreateRepository<ArgusEnabledDc>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
        }

        public string Name => nameof(DeviceDataPointEnricher);
        public int ApplyOrder => 2;

        public void Enrich(PipelineExecutionContext context, PowerDevice instance)
        {
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

        public void EnrichLiveData(PipelineExecutionContext context, PowerDevice instance)
        {
            // do nothing
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void EnsureLookup(PipelineExecutionContext context)
        {
            var dcName = context.DcName;
            if (zenonDataPointsLookup == null || currentDcName != dcName)
            {
                lock (syncObj)
                {
                    if (zenonDataPointsLookup == null || currentDcName != dcName)
                    {
                        var cancel = new CancellationToken();
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

            context.ZenonDataPointLookup = zenonDataPointsLookup;
            context.EnabledDataCenters = enabledDcNames;
        }

        public void EnsureLiveData(PipelineExecutionContext context)
        {
            // do nothing
        }

        private void ReleaseUnmanagedResources()
        {
            zenonDataPointsLookup?.Clear();
            zenonDataPointsLookup = null;
        }

        ~DeviceDataPointEnricher()
        {
            ReleaseUnmanagedResources();
        }
    }
}