// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataPoint.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System;
    using System.Linq;

    public static class DataPoint
    {
        public static bool ChannelNameEquals(this PowerDevice device, string name)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Equals(name, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static bool ChannelNameNotEquals(this PowerDevice device, string name)
        {
            return device.LastReadings?.All(r => !r.DataPoint.Equals(name, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static bool ChannelNameContains(this PowerDevice device, string name)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Contains(name, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static bool ChannelNameNotContains(this PowerDevice device, string name)
        {
            return device.LastReadings?.All(r => !r.DataPoint.Contains(name, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static bool ChannelNameStartsWith(this PowerDevice device, string name)
        {
            return device.LastReadings?.Any(r => r.DataPoint.StartsWith(name, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static bool ChannelNameNotStartsWith(this PowerDevice device, string name)
        {
            return device.LastReadings?.All(r => !r.DataPoint.StartsWith(name, StringComparison.OrdinalIgnoreCase)) == true;
        }
    }
}