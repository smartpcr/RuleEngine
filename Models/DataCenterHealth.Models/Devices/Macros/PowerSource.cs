// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerSource.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    public static class PowerSource
    {
        public static bool IsPowerSourceDevice(this PowerDevice device)
        {
            return device.DevicePath == DevicePath.Self &&
                   device.DeviceFamily == DeviceFamily.PowerSource &&
                   device.Hierarchy != "GEN";
        }
    }
}