// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceHierarchyTraversal.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Traversals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Devices;
    using Microsoft.Extensions.Logging;

    public class DeviceHierarchyDeviceTraversal : IDeviceTraversalStrategy
    {
        private readonly Dictionary<string, PowerDevice> deviceLookup;
        private readonly ILogger<DeviceHierarchyDeviceTraversal> logger;
        private readonly Dictionary<string, List<DeviceRelation>> relationLookup;

        public DeviceHierarchyDeviceTraversal(
            Dictionary<string, PowerDevice> deviceLookup,
            Dictionary<string, List<DeviceRelation>> relationLookup,
            ILoggerFactory loggerFactory)
        {
            this.deviceLookup = deviceLookup;
            this.relationLookup = relationLookup;
            logger = loggerFactory.CreateLogger<DeviceHierarchyDeviceTraversal>();
        }

        public IEnumerable<PowerDeviceDetail> FindParents(
            DeviceHierarchy currentHierarchy,
            AssociationType? associationType,
            List<string> devicesInLoop,
            HashSet<string> visited = null)
        {
            var parentDevices = new List<PowerDeviceDetail>();

            var upstreamDevices =
                associationType.HasValue
                    ? currentHierarchy.DirectUpstreamDeviceList?.Where(r => r.AssociationType == associationType.Value)
                        .ToList()
                    : currentHierarchy.DirectUpstreamDeviceList;
            if (upstreamDevices?.Any() == true)
            {
                foreach (var upstreamDevice in upstreamDevices)
                    if (deviceLookup.ContainsKey(upstreamDevice.DeviceName))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(upstreamDevice.DeviceName, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(upstreamDevice.DeviceName);
                            logger.LogWarning($"circular path: deviceName={upstreamDevice.DeviceName}");
                        }
                        else
                            parentDevices.Add(ToDetail(deviceLookup[upstreamDevice.DeviceName], relationLookup));
                    }

                if (parentDevices.Any()) return parentDevices;
            }

            switch (associationType)
            {
                case AssociationType.Primary:
                    if (!string.IsNullOrEmpty(currentHierarchy.PrimaryParent) &&
                        deviceLookup.ContainsKey(currentHierarchy.PrimaryParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentHierarchy.PrimaryParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentHierarchy.PrimaryParent);
                            logger.LogWarning($"circular path: deviceName={currentHierarchy.PrimaryParent}");
                            return null;
                        }

                        return new List<PowerDeviceDetail>
                        {
                            ToDetail(deviceLookup[currentHierarchy.PrimaryParent], relationLookup)
                        };
                    }

                    break;
                case AssociationType.Backup:
                    if (!string.IsNullOrEmpty(currentHierarchy.SecondaryParent) &&
                        deviceLookup.ContainsKey(currentHierarchy.SecondaryParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentHierarchy.SecondaryParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentHierarchy.SecondaryParent);
                            logger.LogWarning($"circular path: deviceName={currentHierarchy.SecondaryParent}");
                            return null;
                        }

                        return new List<PowerDeviceDetail>
                        {
                            ToDetail(deviceLookup[currentHierarchy.SecondaryParent], relationLookup)
                        };
                    }

                    break;
                case AssociationType.Maintenance:
                    if (!string.IsNullOrEmpty(currentHierarchy.MaintenanceParent) &&
                        deviceLookup.ContainsKey(currentHierarchy.MaintenanceParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentHierarchy.MaintenanceParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentHierarchy.MaintenanceParent);
                            logger.LogWarning($"circular path: deviceName={currentHierarchy.MaintenanceParent}");
                            return null;
                        }

                        return new List<PowerDeviceDetail>
                        {
                            ToDetail(deviceLookup[currentHierarchy.MaintenanceParent], relationLookup)
                        };
                    }

                    break;
                case AssociationType.PowerSource:
                    if (!string.IsNullOrEmpty(currentHierarchy.PowerSourceParent) &&
                        deviceLookup.ContainsKey(currentHierarchy.PowerSourceParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentHierarchy.PowerSourceParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentHierarchy.PowerSourceParent);
                            logger.LogWarning($"circular path: deviceName={currentHierarchy.PowerSourceParent}");
                            return null;
                        }

                        return new List<PowerDeviceDetail>
                        {
                            ToDetail(deviceLookup[currentHierarchy.PowerSourceParent], relationLookup)
                        };
                    }

                    break;
            }

            return null;
        }

        public IEnumerable<PowerDevice> FindParentDevices(
            PowerDevice currentDevice,
            AssociationType? associationType,
            List<string> devicesInLoop,
            HashSet<string> visited = null)
        {
            var parentDevices = new List<PowerDevice>();

            var upstreamDevices =
                associationType.HasValue
                    ? currentDevice.DirectUpstreamDeviceList?.Where(r => r.AssociationType == associationType.Value)
                        .ToList()
                    : currentDevice.DirectUpstreamDeviceList;
            if (upstreamDevices?.Any() == true)
            {
                foreach (var upstreamDevice in upstreamDevices)
                    if (deviceLookup.ContainsKey(upstreamDevice.DeviceName))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(upstreamDevice.DeviceName, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(upstreamDevice.DeviceName);
                            logger.LogWarning($"circular path: deviceName={upstreamDevice.DeviceName}");
                        }
                        else
                            parentDevices.Add(deviceLookup[upstreamDevice.DeviceName]);
                    }

                if (parentDevices.Any()) return parentDevices;
            }

            switch (associationType)
            {
                case AssociationType.Primary:
                    if (!string.IsNullOrEmpty(currentDevice.PrimaryParent) &&
                        deviceLookup.ContainsKey(currentDevice.PrimaryParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentDevice.PrimaryParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentDevice.PrimaryParent);
                            logger.LogWarning($"circular path: deviceName={currentDevice.PrimaryParent}");
                            return null;
                        }

                        return new List<PowerDevice>
                        {
                            deviceLookup[currentDevice.PrimaryParent]
                        };
                    }
                    break;
                case AssociationType.Backup:
                    if (!string.IsNullOrEmpty(currentDevice.SecondaryParent) &&
                        deviceLookup.ContainsKey(currentDevice.SecondaryParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentDevice.SecondaryParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentDevice.SecondaryParent);
                            logger.LogWarning($"circular path: deviceName={currentDevice.SecondaryParent}");
                            return null;
                        }

                        return new List<PowerDevice>
                        {
                            deviceLookup[currentDevice.SecondaryParent]
                        };
                    }
                    break;
                case AssociationType.Maintenance:
                    if (!string.IsNullOrEmpty(currentDevice.MaintenanceParent) &&
                        deviceLookup.ContainsKey(currentDevice.MaintenanceParent))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(currentDevice.MaintenanceParent, StringComparison.OrdinalIgnoreCase)))
                        {
                            devicesInLoop.Add(currentDevice.MaintenanceParent);
                            logger.LogWarning($"circular path: deviceName={currentDevice.MaintenanceParent}");
                            return null;
                        }

                        return new List<PowerDevice>
                        {
                            deviceLookup[currentDevice.MaintenanceParent]
                        };
                    }
                    break;
            }

            return null;
        }

        public IEnumerable<PowerDeviceDetail> FindAllParents(PowerDeviceDetail deviceDetail, out List<string> devicesInLoop)
        {
            devicesInLoop = new List<string>();
            var allParents = new List<PowerDeviceDetail>();
            var parentVisited = new HashSet<string> {deviceDetail.General.DeviceName};
            var parentDevices = FindParents(deviceDetail.Hierarchy, AssociationType.Primary, devicesInLoop, parentVisited);
            while (parentDevices?.Any() == true)
            {
                var grandParents = new List<PowerDeviceDetail>();
                foreach (var parentDevice in parentDevices)
                {
                    if (parentVisited.Contains(parentDevice.General.DeviceName)) continue;

                    allParents.Add(parentDevice);
                    parentVisited.Add(parentDevice.General.DeviceName);
                    deviceDetail.Hierarchy.RootDevice = parentDevice;

                    var currentHierarchy = parentDevice.Hierarchy;
                    var newParentDevices = FindParents(currentHierarchy, null, devicesInLoop, parentVisited);
                    if (newParentDevices?.Any() == true) grandParents.AddRange(newParentDevices);
                }

                parentDevices = grandParents;
            }

            return allParents;
        }

        public IEnumerable<PowerDevice> FindAllParentDevices(PowerDevice currentDevice, out List<string> devicesInLoop)
        {
            devicesInLoop = new List<string>();
            var allParents = new List<PowerDevice>();
            var parentVisited = new HashSet<string> {currentDevice.DeviceName};
            var parentDevices = FindParentDevices(currentDevice, AssociationType.Primary, devicesInLoop, parentVisited);
            while (parentDevices?.Any() == true)
            {
                var grandParents = new List<PowerDevice>();
                foreach (var parentDevice in parentDevices)
                    if (!parentVisited.Contains(parentDevice.DeviceName))
                    {
                        allParents.Add(parentDevice);
                        parentVisited.Add(parentDevice.DeviceName);
                        currentDevice.RootDevice = parentDevice;

                        var newParentDevices = FindParentDevices(parentDevice, null, devicesInLoop, parentVisited);
                        if (newParentDevices?.Any() == true) grandParents.AddRange(newParentDevices);
                    }

                parentDevices = grandParents;
            }

            return allParents;
        }

        public IEnumerable<PowerDeviceDetail> FindChildren(DeviceHierarchy currentHierarchy,
            AssociationType? associationType, HashSet<string> visited = null)
        {
            var childDevices = new List<PowerDeviceDetail>();
            var downstreamDevices =
                associationType.HasValue
                    ? currentHierarchy.DirectDownstreamDeviceList
                        ?.Where(r => r.AssociationType == associationType.Value).ToList()
                    : currentHierarchy.DirectDownstreamDeviceList;
            if (downstreamDevices?.Any() == true)
            {
                var distinctDownstreamDeviceNames = downstreamDevices.Select(d => d.DeviceName).Distinct().ToList();
                foreach (var downstreamDeviceName in distinctDownstreamDeviceNames)
                {
                    if (deviceLookup.ContainsKey(downstreamDeviceName))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(downstreamDeviceName, StringComparison.OrdinalIgnoreCase)))
                            logger.LogWarning($"circular path, deviceName={downstreamDeviceName}");
                        else
                            childDevices.Add(ToDetail(deviceLookup[downstreamDeviceName], relationLookup));
                    }
                }
            }
            else
            {
                var childDevicesFound = deviceLookup.Values
                    .Where(v => v.PrimaryParent != null &&
                                v.PrimaryParent == currentHierarchy.DeviceName)
                    .ToList();
                if (childDevicesFound.Any())
                    foreach (var childDevice in childDevicesFound)
                        if (visited != null && visited.Any(v =>
                            v.Equals(childDevice.DeviceName, StringComparison.OrdinalIgnoreCase)))
                            logger.LogWarning($"circular path, deviceName={childDevice.DeviceName}");
                        else
                            childDevices.Add(ToDetail(childDevice, relationLookup));
            }

            return childDevices;
        }

        public IEnumerable<PowerDevice> FindChildDevices(
            PowerDevice currentDevice,
            AssociationType? associationType,
            HashSet<string> visited = null)
        {
            var childDevices = new List<PowerDevice>();
            var downstreamDevices =
                associationType.HasValue
                    ? currentDevice.DirectDownstreamDeviceList
                        ?.Where(r => r.AssociationType == associationType.Value).ToList()
                    : currentDevice.DirectDownstreamDeviceList;
            if (downstreamDevices?.Any() == true)
            {
                var distinctDownstreamDeviceNames = downstreamDevices.Select(d => d.DeviceName).Distinct().ToList();
                foreach (var downstreamDeviceName in distinctDownstreamDeviceNames)
                {
                    if (deviceLookup.ContainsKey(downstreamDeviceName))
                    {
                        if (visited != null && visited.Any(v =>
                            v.Equals(downstreamDeviceName, StringComparison.OrdinalIgnoreCase)))
                            logger.LogWarning($"circular path, deviceName={downstreamDeviceName}");
                        else
                            childDevices.Add(deviceLookup[downstreamDeviceName]);
                    }
                }
            }
            else
            {
                var childDevicesFound = deviceLookup.Values
                    .Where(v => v.PrimaryParent != null &&
                                v.PrimaryParent == currentDevice.DeviceName)
                    .ToList();
                if (childDevicesFound.Any())
                    foreach (var childDevice in childDevicesFound)
                        if (visited != null && visited.Any(v =>
                            v.Equals(childDevice.DeviceName, StringComparison.OrdinalIgnoreCase)))
                            logger.LogWarning($"circular path, deviceName={childDevice.DeviceName}");
                        else
                            childDevices.Add(childDevice);
            }

            return childDevices;
        }

        public IEnumerable<PowerDeviceDetail> FindAllChildren(PowerDeviceDetail deviceDetail)
        {
            var children = new List<PowerDeviceDetail>();
            var childrenVisited = new HashSet<string> {deviceDetail.General.DeviceName};
            var childDevices = FindChildren(deviceDetail.Hierarchy, AssociationType.Primary, childrenVisited);
            while (childDevices.Any())
            {
                var grandChildren = new List<PowerDeviceDetail>();

                foreach (var child in childDevices)
                {
                    if (!childrenVisited.Contains(child.General.DeviceName))
                    {
                        childrenVisited.Add(child.General.DeviceName);
                        children.Add(child);

                        var grandChildDevices = FindChildren(child.Hierarchy, AssociationType.Primary, childrenVisited);
                        if (grandChildDevices.Any()) grandChildren.AddRange(grandChildDevices);
                    }
                }

                childDevices = grandChildren;
            }

            return children;
        }

        public IEnumerable<PowerDevice> FindAllChildDevices(PowerDevice currentDevice)
        {
            var children = new List<PowerDevice>();
            var childrenVisited = new HashSet<string> {currentDevice.DeviceName};
            var childDevices = FindChildDevices(currentDevice, AssociationType.Primary, childrenVisited);
            while (childDevices.Any())
            {
                var grandChildren = new List<PowerDevice>();

                foreach (var child in childDevices)
                {
                    if (!childrenVisited.Contains(child.DeviceName))
                    {
                        childrenVisited.Add(child.DeviceName);
                        children.Add(child);

                        var grandChildDevices = FindChildDevices(child, AssociationType.Primary, childrenVisited);
                        if (grandChildDevices.Any()) grandChildren.AddRange(grandChildDevices);
                    }
                }

                childDevices = grandChildren;
            }

            return children;
        }

        public static PowerDeviceDetail ToDetail(PowerDevice device, Dictionary<string, List<DeviceRelation>> lookup)
        {
            var detail = new PowerDeviceDetail
            {
                General = new DeviceGeneral
                {
                    DeviceName = device.DeviceName,
                    DcName = device.DcName,
                    Hierarchy = device.Hierarchy,
                    DeviceState = device.DeviceState.ToString(),
                    DeviceType = device.DeviceType.ToString(),
                    OnboardingMode = device.OnboardingMode.ToString(),
                    DataType = device.DataType
                },
                CopaConfig = new CopaConfig
                {
                    ProjectName = device.ProjectName,
                    DriverName = device.DriverName,
                    ConfiguredDriverType = device.ConfiguredObjectType,
                    ConnectionName = device.ConnectionName,
                    ConfiguredObjectType = device.ConfiguredObjectType
                },
                Rating = new DeviceRating
                {
                    IsMonitorable = device.IsMonitorable,
                    Amperage = device.Amperage,
                    Voltage = device.Voltage,
                    RatedCapacity = device.RatedCapacity,
                    DeratedCapacity = device.DeratedCapacity,
                    MaxItCapacity = device.MaxItCapacity,
                    ItCapacity = device.ItCapacity,
                    AmpRating = device.AmpRating,
                    VoltageRating = device.VoltageRating,
                    KwRating = device.KwRating,
                    KvaRating = device.KvaRating,
                    PowerFactor = device.PowerFactor,
                    DeRatingFactor = device.DeRatingFactor
                },
                Location = new DeviceLocation
                {
                    ColoName = device.ColoName,
                    PanelName = device.PanelName
                },
                Hierarchy = new DeviceHierarchy
                {
                    DeviceName = device.DeviceName,
                    PrimaryParent = device.PrimaryParent,
                    SecondaryParent = device.SecondaryParent,
                    MaintenanceParent = device.MaintenanceParent,
                    RedundantDeviceNames = device.RedundantDeviceNames
                }
            };

            var relations = lookup.ContainsKey(device.DeviceName)
                ? lookup[device.DeviceName]
                : null;
            if (relations != null)
            {
                detail.Hierarchy.DirectUpstreamDeviceList =
                    relations
                        .Where(r => r.DirectUpstreamDeviceList != null)
                        .SelectMany(r => r.DirectUpstreamDeviceList).ToList();
                detail.Hierarchy.DirectDownstreamDeviceList =
                    relations
                        .Where(r => r.DirectDownstreamDeviceList != null)
                        .SelectMany(r => r.DirectDownstreamDeviceList).ToList();
            }
            else
            {
                detail.Hierarchy.DirectDownstreamDeviceList = new List<DeviceAssociation>();
                detail.Hierarchy.DirectUpstreamDeviceList = new List<DeviceAssociation>();
            }

            return detail;
        }
    }
}