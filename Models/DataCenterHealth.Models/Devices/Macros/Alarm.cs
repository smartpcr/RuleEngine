// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Alarm.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class Alarm
    {
        public static bool IsAlerm(this PowerDevice device)
        {
            return device.LastReadings?.Any(r => r.ChannelType == "Alarms") == true;
        }
    }
}