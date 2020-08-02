// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParentPath.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Traversals;
    using Microsoft.Extensions.Logging;

    public static class ParentPath
    {
        public static bool? RootInHierarchyGenOrUts(this PowerDevice device)
        {
            if (device.EvaluationContext == null)
                throw new InvalidOperationException("device evaluation context is not initialized");
            
            var checkName = "parent hierarchy check";
            device.EvaluationContext.Logger.LogDebug($"Started {checkName} for device {device.DeviceName}...");
            var allowedHierarchiesForPowersourceDevices = new List<string>
            {
                DeviceHierarchies.UTS_Facility,
                DeviceHierarchies.UTS_Campus,
                DeviceHierarchies.GEN
            };
            var expectedValues = string.Join(",", allowedHierarchiesForPowersourceDevices);

            var currentDevice = device.EvaluationContext.DeviceLookup.ContainsKey(device.DeviceName)
                ? device.EvaluationContext.DeviceLookup[device.DeviceName]
                : null;
            if (currentDevice == null)
            {
                device.EvaluationContext.Logger.LogDebug($"Skip {checkName} for device {device.DeviceName}: device not found");
                return null;
            }

            if (currentDevice.DeviceType == DeviceType.Zenon)
            {
                device.EvaluationContext.Logger.LogDebug($"device {device.DeviceName} is zenon type, skip {checkName}");
                return null;
            }

            if (currentDevice.DirectDownstreamDeviceList?.Count > 0)
            {
                device.EvaluationContext.Logger.LogDebug($"device {device.DeviceName} is not leaf, skip {checkName}");
                return null;
            }

            var leafDeviceDetail = DeviceHierarchyDeviceTraversal.ToDetail(currentDevice, device.EvaluationContext.RelationLookup);
            var allParents = device.EvaluationContext.DeviceTraversal.FindAllParents(leafDeviceDetail, out _)?.ToList();
            var inCorrectHierarchy =
                allParents?.Any(p => allowedHierarchiesForPowersourceDevices.Contains(p.General.Hierarchy)) == true;
            var visitedHierarchies = allParents?.Any() == true
                ? string.Join(",", allParents.Select(p => p.General.Hierarchy))
                : "";
            if (!inCorrectHierarchy)
            {
                device.EvaluationContext.AppTelemetry.RecordMetric(
                    "parent-hierarchy-error",
                    1,
                    ("leafDevice", currentDevice.DeviceName),
                    ("dcName", device.EvaluationContext.DcName));
                device.EvaluationContext.Logger.LogWarning($"{checkName} failed for: {currentDevice.DeviceName}");
                
                device.AddEvaluationEvidence(new DeviceValidationEvidence()
                {
                    Actual = visitedHierarchies,
                    Expected = expectedValues,
                    PropertyPath = "Hierarchy",
                    Error = "none of parent devices are in GEN/UTS hierarchy",
                    Operator = "",
                    Score = 0
                });
            }

            return inCorrectHierarchy;
        }

        public static bool? PowerSourceDeviceHierarchyInGenOrUts(this PowerDevice device)
        {
            if (device.EvaluationContext == null)
                throw new InvalidOperationException("device evaluation context is not initialized");
            
            var checkName = "power source parent hierarchy check";
            device.EvaluationContext.Logger.LogDebug($"Started {checkName} for device {device.DeviceName}...");

            var allowedHierarchiesForPowersourceDevices = new List<string>
            {
                DeviceHierarchies.UTS_Facility,
                DeviceHierarchies.UTS_Campus,
                DeviceHierarchies.GEN
            };
            var expectedValues = string.Join(",", allowedHierarchiesForPowersourceDevices);

            var currentDevice = device.EvaluationContext.DeviceLookup.ContainsKey(device.DeviceName)
                ? device.EvaluationContext.DeviceLookup[device.DeviceName]
                : null;
            if (currentDevice == null)
            {
                device.EvaluationContext.Logger.LogDebug($"Skip {checkName} for device {device.DeviceName}: device not found");
                return null;
            }

            if (currentDevice.DeviceType == DeviceType.Zenon)
            {
                device.EvaluationContext.Logger.LogDebug($"device {device.DeviceName} is zenon type, skip {checkName}");
                return null;
            }

            var deviceDetail = DeviceHierarchyDeviceTraversal.ToDetail(currentDevice, device.EvaluationContext.RelationLookup);
            if (deviceDetail.Hierarchy.DirectUpstreamDeviceList?.Any(a => a.AssociationType == AssociationType.PowerSource) != true)
            {
                device.EvaluationContext.Logger.LogDebug($"device {deviceDetail.General.DeviceName} doesn't have power source parent, skip {checkName}");
                return null;
            }

            var inCorrectHierarchy = allowedHierarchiesForPowersourceDevices.Contains(deviceDetail.General.Hierarchy);
            if (!inCorrectHierarchy)
            {
                device.EvaluationContext.AppTelemetry.RecordMetric(
                    "powersource-hierarchy-error",
                    1,
                    ("deviceName", deviceDetail.General.DeviceName),
                    ("dcName", device.EvaluationContext.DcName));
                device.EvaluationContext.Logger.LogWarning($"{checkName} failed for {deviceDetail.General.DeviceName}");
                
                device.AddEvaluationEvidence(new DeviceValidationEvidence()
                {
                    Actual = deviceDetail.General.Hierarchy,
                    Expected = expectedValues,
                    Score = 0,
                    Error = "power source device hierarhy should be in GEN/UTS",
                    Operator = "",
                    PropertyPath = "Hierarchy"
                });
            }

            return inCorrectHierarchy;
        }
    }
}