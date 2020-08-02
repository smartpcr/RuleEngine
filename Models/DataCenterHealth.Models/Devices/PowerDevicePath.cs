// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDevicePath.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    public class PowerDevicePath
    {
        public string DeviceName { get; set; }
        public double HierarchyId { get; set; }
        public DevicePath DevicePath { get; set; }
        public DeviceFamily DeviceFamily { get; set; }
        public int Validate { get; set; }
    }
}