// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITraversalStrategy.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Traversals
{
    using System.Collections.Generic;
    using Devices;

    public interface IDeviceTraversalStrategy
    {
        IEnumerable<PowerDeviceDetail> FindParents(
            DeviceHierarchy currentHierarchy,
            AssociationType? associationType,
            List<string> devicesInLoop,
            HashSet<string> visited = null);

        IEnumerable<PowerDevice> FindParentDevices(
            PowerDevice currentDevice,
            AssociationType? associationType,
            List<string> devicesInLoop,
            HashSet<string> visited = null);

        IEnumerable<PowerDeviceDetail> FindAllParents(PowerDeviceDetail deviceDetail, out List<string> devicesInLoop);

        IEnumerable<PowerDevice> FindAllParentDevices(PowerDevice currentDevice, out List<string> devicesInLoop);

        IEnumerable<PowerDeviceDetail> FindChildren(
            DeviceHierarchy currentHierarchy,
            AssociationType? associationType,
            HashSet<string> visited = null);

        IEnumerable<PowerDevice> FindChildDevices(
            PowerDevice currentDevice,
            AssociationType? associationType,
            HashSet<string> visited = null);

        IEnumerable<PowerDeviceDetail> FindAllChildren(PowerDeviceDetail deviceDetail);

        IEnumerable<PowerDevice> FindAllChildDevices(PowerDevice currentDevice);
    }
}