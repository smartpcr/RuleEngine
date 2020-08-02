// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KwTot.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataCenterHealth.Models.Jobs;
    using Microsoft.Extensions.Logging;

    public static class KwTot
    {
        public static bool? KwTotMatchChildren(this PowerDevice device)
        {
            if (device.EvaluationContext == null)
                throw new InvalidOperationException("device evaluation context is not initialized");
            
            var dataPointName = "Pwr.kW tot";
            const double min = 0.001;
            const double max = 214748364.7;
            const double threshold = 10.0;

            device.EvaluationContext.Logger.LogDebug($"Checking {dataPointName} for device {device.DeviceName}");

            var currentDevice = device.EvaluationContext.DeviceLookup.ContainsKey(device.DeviceName)
                ? device.EvaluationContext.DeviceLookup[device.DeviceName]
                : null;
            if (currentDevice == null)
            {
                device.EvaluationContext.Logger.LogDebug($"Skip {dataPointName} check for device {device.DeviceName}: device not found");
                return null;
            }

            var zenonEvent = device.EvaluationContext.ZenonStatsLookup.ContainsKey(device.DeviceName)
                ? device.EvaluationContext.ZenonStatsLookup[device.DeviceName].FirstOrDefault(
                    z => z.DataPoint.Equals(dataPointName, StringComparison.OrdinalIgnoreCase))
                : null;
            if (zenonEvent == null)
            {
                device.EvaluationContext.Logger.LogDebug($"Skip {dataPointName} check for device {device.DeviceName}: no data");
                return null;
            }

            if (zenonEvent.Avg < min || zenonEvent.Avg > max)
            {
                device.EvaluationContext.Logger.LogDebug($"Skip {dataPointName} check for device {device.DeviceName}: value {zenonEvent.Avg} out of range");
                return null;
            }

            var childDevices = device.EvaluationContext.DeviceTraversal.FindChildDevices(
                currentDevice,
                AssociationType.Primary)?.ToList();
            if (!(childDevices?.Count > 0))
            {
                device.EvaluationContext.Logger.LogDebug($"Skip {dataPointName} check for device {device.DeviceName}: no children");
                return null;
            }

            double parentKwTotal = zenonEvent.Avg;
            double childKwTotal = 0;
            var childDevicesWithValue = new HashSet<string>();
            foreach (var child in childDevices)
            {
                if (device.EvaluationContext.ZenonStatsLookup.ContainsKey(child.DeviceName))
                {
                    var childZenonStats = device.EvaluationContext.ZenonStatsLookup[child.DeviceName];
                    var zenonDataPoint = childZenonStats.FirstOrDefault(zdp => zdp.DataPoint.Equals(dataPointName, StringComparison.OrdinalIgnoreCase));
                    if (zenonDataPoint != null && zenonDataPoint.Avg > min && zenonDataPoint.Avg < max)
                    {
                        childKwTotal += zenonDataPoint.Avg;
                        childDevicesWithValue.Add(child.DeviceName);
                    }
                }
            }

            var differencePct = Math.Min(100, Math.Abs(parentKwTotal - childKwTotal) / parentKwTotal * 100);
            if (differencePct > threshold)
            {
                device.EvaluationContext.AppTelemetry.RecordMetric(
                    "kw-tot-match-children-sum-error",
                    1,
                    ("deviceName", currentDevice.DeviceName),
                    ("dcName", device.EvaluationContext.DcName));
                device.EvaluationContext.Logger.LogWarning($"{dataPointName} check failed for: {currentDevice.DeviceName}");
                device.AddEvaluationEvidence(new DeviceValidationEvidence()
                {
                    Actual = $"parent: {Math.Round(parentKwTotal, 2)}",
                    Expected = $"children: {Math.Round(childKwTotal, 2)}",
                    Score = Math.Abs(Math.Round((100 - differencePct) / 100, 2)),
                    Error = "device reading for Pwr.kW tot should match that of children",
                    PropertyPath = "ReadingStats['Pwr.KW tot']",
                    Operator = ""
                });
            }

            return differencePct <= threshold;
        }
    }
}