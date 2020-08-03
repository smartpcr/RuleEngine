// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceRelationEnricher.cs" company="Microsoft Corporation">
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
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Traversals;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DeviceRelationEnricher : IContextEnricher<PowerDevice>
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<DeviceRelationEnricher> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly IDocDbRepository<PowerDevice> powerDeviceRepo;
        private readonly IDocDbRepository<DeviceRelation> deviceRelationRepo;
        
        private readonly object syncObj = new object();
        private string currentDcName;
        private Dictionary<string, PowerDevice> lookups;
        private Dictionary<string, PowerDevice> redundantDeviceLookup;
        private Dictionary<string, List<DeviceRelation>> relationLookup;
        
        public string Name => nameof(DeviceRelationEnricher);
        public int ApplyOrder => 1;

        public DeviceRelationEnricher(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<DeviceRelationEnricher>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            powerDeviceRepo = repoFactory.CreateRepository<PowerDevice>();
            deviceRelationRepo = repoFactory.CreateRepository<DeviceRelation>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
        }
        
        public void EnsureLookup(EvaluationContext context, bool isLiveData = false, CancellationToken cancel = default)
        {
            if (isLiveData) return;
            
            var dcName = context.DcName;
            if (lookups == null || currentDcName != dcName)
            {
                lock (syncObj)
                {
                    if (lookups == null || currentDcName != dcName)
                    {
                        try
                        {
                            currentDcName = dcName;
                            var cacheKey = $"{nameof(PowerDevice)}-list-{dcName}";
                            logger.LogInformation($"retrieving device list: {dcName}");
                            var dcNameQuery = $"c.dcName = '{dcName}'";
                            var deviceList = cache.GetOrUpdateAsync(
                                cacheKey,
                                async () => await powerDeviceRepo.GetLastModificationTime(dcNameQuery, cancel),
                                async () =>
                                {
                                    var devices = await powerDeviceRepo.Query(dcNameQuery);
                                    return devices.ToList();
                                },
                                cancel).GetAwaiter().GetResult();
                            logger.LogInformation($"Total of {deviceList.Count} power devices found for dc: {dcName}");

                            logger.LogInformation($"retrieving device relation: {dcName}");
                            var relationList = cache.GetOrUpdateAsync(
                                $"list-{nameof(DeviceRelation)}-{dcName}",
                                async () => await deviceRelationRepo.GetLastModificationTime(dcNameQuery, cancel),
                                async () =>
                                {
                                    var relations = await deviceRelationRepo.Query(dcNameQuery);
                                    return relations.ToList();
                                },
                                cancel).GetAwaiter().GetResult();
                            logger.LogInformation($"total of {relationList.Count} associations found for dc: {dcName}");

                            relationLookup = relationList.GroupBy(dr => dr.Name)
                                .ToDictionary(g => g.Key, g => g.ToList());
                            foreach (var powerDevice in deviceList)
                            {
                                var relations = relationLookup.ContainsKey(powerDevice.DeviceName)
                                    ? relationLookup[powerDevice.DeviceName]
                                    : null;
                                if (relations != null)
                                {
                                    powerDevice.DirectUpstreamDeviceList =
                                        relations
                                            .Where(r => r.DirectUpstreamDeviceList != null)
                                            .SelectMany(r => r.DirectUpstreamDeviceList).ToList();
                                    powerDevice.DirectDownstreamDeviceList =
                                        relations
                                            .Where(r => r.DirectDownstreamDeviceList != null)
                                            .SelectMany(r => r.DirectDownstreamDeviceList).ToList();
                                }
                                else
                                {
                                    powerDevice.DirectDownstreamDeviceList = new List<DeviceAssociation>();
                                    powerDevice.DirectUpstreamDeviceList = new List<DeviceAssociation>();
                                }
                            }

                            lookups = deviceList.ToDictionary(d => d.DeviceName);
                            var redundantDeviceNames = deviceList.Where(d => !string.IsNullOrEmpty(d.RedundantDeviceNames)).Select(d => d.RedundantDeviceNames)
                                .ToList();
                            redundantDeviceLookup = deviceList.Where(d => redundantDeviceNames.Contains(d.DeviceName)).ToDictionary(d => d.DeviceName);
                            logger.LogInformation($"lookup is populated: {lookups.Count}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to populate lookups");
                            throw;
                        }
                    }
                }
            }

            context.SetDevices(lookups);
            context.SetRedundantDevices(redundantDeviceLookup);
            context.SetDeviceRelations(relationLookup);
        }

        public void Enrich(EvaluationContext context, PowerDevice instance, bool isLiveData = false)
        {
            if (isLiveData) return;
            
            using var scope = appTelemetry.StartOperation(this);
            EnsureLookup(context);

            if (!string.IsNullOrEmpty(instance.PrimaryParent) &&
                instance.PrimaryParentDevice == null &&
                lookups.ContainsKey(instance.PrimaryParent))
                instance.PrimaryParentDevice = lookups[instance.PrimaryParent];

            if (!string.IsNullOrEmpty(instance.SecondaryParent) &&
                instance.SecondaryParentDevice == null &&
                lookups.ContainsKey(instance.SecondaryParent))
                instance.SecondaryParentDevice = lookups[instance.SecondaryParent];

            IDeviceTraversalStrategy deviceTraversal = new DeviceHierarchyDeviceTraversal(lookups, relationLookup, loggerFactory);
            if (!string.IsNullOrEmpty(instance.PrimaryParent) && instance.AllParents == null)
            {
                instance.AllParents = deviceTraversal.FindAllParentDevices(instance, out _)?.ToList() ?? new List<PowerDevice>();
            }

            if (!string.IsNullOrEmpty(instance.RedundantDeviceNames) &&
                instance.RedundantDevice == null &&
                lookups.ContainsKey(instance.RedundantDeviceNames))
            {
                instance.RedundantDevice = lookups[instance.RedundantDeviceNames];
            }

            if (!string.IsNullOrEmpty(instance.MaintenanceParent) &&
                instance.MaintenanceParentDevice == null &&
                lookups.ContainsKey(instance.MaintenanceParent))
            {
                instance.MaintenanceParentDevice = lookups[instance.MaintenanceParent];
            }

            instance.Children = deviceTraversal.FindChildDevices(instance, AssociationType.Primary)?.ToList() ?? new List<PowerDevice>();
            instance.SiblingDevices = lookups.Values.Where(
                v => v.DeviceName != instance.DeviceName && v.PrimaryParent == instance.PrimaryParent).ToList() ?? new List<PowerDevice>();

            instance.IsRedundantDevice = redundantDeviceLookup?.ContainsKey(instance.DeviceName) == true;
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

        ~DeviceRelationEnricher()
        {
            ReleaseUnmanagedResources();
        }

        #endregion
    }
}