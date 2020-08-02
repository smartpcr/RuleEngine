// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DevicePathEnricher.cs" company="Microsoft Corporation">
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
    using System.Threading.Tasks;
    using Common.Cache;
    using Common.Kusto;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;

    public class DevicePathEnricher : IContextEnricher<PowerDevice>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly ILogger<DevicePathEnricher> logger;
        private readonly IKustoClient kustoClient;
        private readonly object syncObj = new object();
        private string currentDcName;
        private Dictionary<string, PowerDevicePath> lookups;

        private const string listPanelNameQueryTemplate =
            "cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').ListPowerDeviceDeviceNames_v14('{0}')";
        private const string getDevicesForPanelQueryTemplate =
            "cluster('mciocihprod.kusto.windows.net').database('MCIOCIHArgusProd').GetDevicesForEquipment_v14('{0}', '{1}')";

        public string Name => nameof(DeviceRelationEnricher);
        public int ApplyOrder => 4;

        public DevicePathEnricher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DevicePathEnricher>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            kustoClient = serviceProvider.GetRequiredService<IKustoClient>();
        }

        public void Enrich(PipelineExecutionContext context, PowerDevice instance)
        {
            using var scope = appTelemetry.StartOperation(this);
            EnsureLookup(context);
            if (lookups.ContainsKey(instance.DeviceName))
            {
                var devicePath = lookups[instance.DeviceName];
                instance.DeviceFamily = devicePath.DeviceFamily;
                instance.DevicePath = devicePath.DevicePath;
                instance.HierarchyId = devicePath.HierarchyId;
                instance.Validate = devicePath.Validate;
            }
        }

        public void EnrichLiveData(PipelineExecutionContext context, PowerDevice instance)
        {
        }

        public void EnsureLookup(PipelineExecutionContext context)
        {
            var dcName = context.DcName;
            if (lookups == null || currentDcName != dcName)
            {
                lock (syncObj)
                {
                    if (lookups == null || currentDcName != dcName)
                    {
                        var cancel = new CancellationToken();
                        try
                        {
                            currentDcName = dcName;
                            var cacheExpireTime = DateTime.UtcNow.AddDays(-5);

                            logger.LogInformation($"retrieving all panels for data center {dcName}");
                            var cacheKey = $"list-PowerDevicePanelNames";
                            var listPanelNamesQuery = string.Format(listPanelNameQueryTemplate, dcName);
                            var devicePathList = cache.GetOrUpdateAsync(
                                cacheKey,
                                async () => await Task.FromResult(cacheExpireTime),
                                async () =>
                                {
                                    var reader = await kustoClient.ExecuteReader(listPanelNamesQuery);
                                    var panelNames = new List<string>();
                                    while (reader.Read())
                                    {
                                        panelNames.Add(reader.GetString(0));
                                    }
                                    reader.Close();

                                    var devicePaths = new List<PowerDevicePath>();
                                    foreach (var panelName in panelNames)
                                    {
                                        var query = string.Format(getDevicesForPanelQueryTemplate, dcName, panelName);
                                        reader = await kustoClient.ExecuteReader(query);
                                        while (reader.Read())
                                        {
                                            devicePaths.Add(new PowerDevicePath()
                                            {
                                                DeviceName = reader.Value<string>("Name"),
                                                DeviceFamily = reader.EnumValue<DeviceFamily>("DeviceFamily"),
                                                DevicePath = reader.EnumValue<DevicePath>("DevicePath"),
                                                HierarchyId = reader.Value<double>("HierarchyId"),
                                                Validate = reader.Value<int>("Validate")
                                            });
                                        }
                                        reader.Close();
                                    }

                                    return devicePaths;
                                },
                                cancel).GetAwaiter().GetResult();
                            logger.LogInformation($"Total of {devicePathList.Count} devices found for dc: {dcName}");
                            lookups = devicePathList.GroupBy(dp => dp.DeviceName).ToDictionary(g => g.Key, g => g.First());
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to populate lookups");
                            throw;
                        }
                    }
                }
            }
        }

        public void EnsureLiveData(PipelineExecutionContext context)
        {
        }

        #region dispose

        private void ReleaseUnmanagedResources()
        {
            lookups?.Clear();
            lookups = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~DevicePathEnricher()
        {
            ReleaseUnmanagedResources();
        }

        #endregion
    }
}