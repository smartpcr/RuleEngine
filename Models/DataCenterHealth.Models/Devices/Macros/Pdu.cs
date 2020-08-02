// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Pdu.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    public static class Pdu
    {
        public static bool IsPudFeederBreaker(this PowerDevice device)
        {
            return device.HierarchyId >= 10.0 && device.DeviceType == DeviceType.Breaker;
        }
    }
}