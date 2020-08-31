// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Traversal.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT.Macros
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DeviceTraversal
    {
        public static List<string> Traverse(this Device device, TraverseDirection direction)
        {
            if (device.EvaluationContext == null)
                throw new InvalidOperationException("device evaluation context is not initialized");

            var path = new List<string>() {device.DeviceName};
            Walk(device, direction, path);
            return path;
        }

        private static void Walk(Device current, TraverseDirection direction, List<string> path)
        {
            string nextDeviceId = null;
            
            switch (direction)
            {
                case TraverseDirection.PrimaryParent:
                    nextDeviceId = current.PrimaryParent;
                    break;
                case TraverseDirection.SecondaryParent:
                    nextDeviceId = current.SecondaryParent;
                    break;
                case TraverseDirection.PowerSourceParent:
                    if (current.EvaluationContext.RelationLookup.ContainsKey(current.DeviceName))
                    {
                        var relation = current.EvaluationContext.RelationLookup[current.DeviceName].FirstOrDefault(
                            p => p.Value.Association == AssociationType.PowerSource && p.Value.Direction == DirectionType.DirectUpstream);
                        if (relation.Value != null)
                        {
                            nextDeviceId = relation.Value.ToDeviceName;
                        }
                    }
                    break;
                case TraverseDirection.Child:
                    foreach (var fromDeviceName in current.EvaluationContext.RelationLookup.Keys)
                    {
                        if (current.EvaluationContext.RelationLookup[fromDeviceName].ContainsKey(current.DeviceName))
                        {
                            nextDeviceId = fromDeviceName;
                            break;
                        }
                    }
                    break;
                case TraverseDirection.Redundant:
                    nextDeviceId = current.RedundantDeviceNames;
                    break;
            }

            if (!string.IsNullOrEmpty(nextDeviceId) && current.EvaluationContext.DeviceLookup.ContainsKey(nextDeviceId))
            {
                if (path.Contains(nextDeviceId))
                {
                    // circular
                    path.Add(nextDeviceId);
                    path.Add("!!");
                    return;
                }
                
                path.Add(nextDeviceId);
                var nextDevice = current.EvaluationContext.DeviceLookup[nextDeviceId];
                Walk(nextDevice, direction, path);
            }
        }
    }

    public enum TraverseDirection
    {
        PrimaryParent,
        SecondaryParent,
        PowerSourceParent,
        Child,
        Redundant
    }
}