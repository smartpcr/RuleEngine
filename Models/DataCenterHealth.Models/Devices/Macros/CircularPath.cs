// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CircularPath.cs" company="Microsoft Corporation">
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

    public static class CircularPath
    {
        public static bool HasCircularPath(this PowerDevice device)
        {
            if (device.EvaluationContext == null)
                throw new InvalidOperationException("device evaluation context is not initialized");

            var leaf = device.EvaluationContext.DeviceLookup.ContainsKey(device.DeviceName)
                ? device.EvaluationContext.DeviceLookup[device.DeviceName]
                : null;
            if (leaf == null || leaf.DirectDownstreamDeviceList?.Count > 0)
            {
                device.EvaluationContext.Logger.LogDebug($"device {device.DeviceName} is not leaf, skip circular path check");
                return false;
            }
            var leafDeviceDetail = DeviceHierarchyDeviceTraversal.ToDetail(leaf, device.EvaluationContext.RelationLookup);
            var allParents = device.EvaluationContext.DeviceTraversal.FindAllParents(leafDeviceDetail, out var devicesInLoop)?.ToList() ?? new List<PowerDeviceDetail>();
            var allParentDeviceNames = allParents.Select(p => p.General.DeviceName).ToList();
            var haveCircularPath = devicesInLoop.Count > 0;

            if (haveCircularPath)
            {
                var evidence = new DeviceValidationEvidence()
                {
                    Actual = $"device in loop: {string.Join(",", devicesInLoop)}",
                    Expected = $"parents: {string.Join(",", allParentDeviceNames)}",
                    Error = "current device have circular path",
                    Operator = "",
                    Score = 0,
                    PropertyPath = "Parent"
                };
                device.AddEvaluationEvidence(evidence);
            }

            return haveCircularPath;
        }
    }
}