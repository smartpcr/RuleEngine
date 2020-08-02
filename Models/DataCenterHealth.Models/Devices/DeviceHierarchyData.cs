// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceHierarchyData.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System.Collections.Generic;

    public class DeviceHierarchyData
    {
        public List<DeviceNode> Nodes { get; set; }
        public List<DeviceLink> Links { get; set; }
    }

    public class DeviceNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public string Hierarchy { get; set; }
        public string DeviceState { get; set; }
        public decimal? Amperage { get; set; }
        public decimal? Voltage { get; set; }
        public decimal? Power { get; set; }

        public DeviceNode(PowerDevice device)
        {
            Id = device.DeviceName;
            Name = device.DeviceName;
            DeviceType = device.DeviceType.ToString();
            Hierarchy = device.Hierarchy;
            DeviceState = device.DeviceState.ToString();
        }

        public DeviceNode(PowerDeviceDetail detail)
        {
            Id = detail.General.DeviceName;
            Name = detail.General.DeviceName;
            DeviceType = detail.General.DeviceType;
            Hierarchy = detail.General.Hierarchy;
            DeviceState = detail.General.DeviceState;
        }
    }

    public class DeviceLink
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public decimal Weight { get; set; }
        public string Type { get; set; }
        public bool Overlap { get; set; }

        public DeviceLink(PowerDeviceDetail parent, PowerDeviceDetail child, DeviceLinkType linkType)
        {
            Source = parent.General.DeviceName;
            Target = child.General.DeviceName;
            Weight = linkType == DeviceLinkType.Primary ? 1.0M : 0.5M;
            Type = linkType.ToString();
        }

        public DeviceLink(string parentId, string childId, DeviceLinkType linkType)
        {
            Source = parentId;
            Target = childId;
            Weight = 1.0M;
            Type = linkType.ToString();
        }

        public DeviceLink(string parentId, string childId, AssociationType associationType)
        {
            Source = parentId;
            Target = childId;
            Weight = 1.0M;
            switch (associationType)
            {
                case AssociationType.Primary:
                    Type = DeviceLinkType.Primary.ToString();
                    break;
                case AssociationType.Backup:
                    Type = DeviceLinkType.Backup.ToString();
                    break;
                case AssociationType.Maintenance:
                    Type = DeviceLinkType.Maintenance.ToString();
                    break;
                case AssociationType.PowerSource:
                    Type = DeviceLinkType.PowerSource.ToString();
                    break;
                default:
                    Type = DeviceLinkType.Primary.ToString();
                    break;
            }
        }
    }

    public enum DeviceLinkType
    {
        Primary,
        Secondary,
        Redundant,
        PowerSource,
        Backup,
        Maintenance,
        Sibling,
        Unknown
    }
}