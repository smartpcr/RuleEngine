// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Volt.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class Volt
    {
        public static double MinChannelVolt(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("Volt.")).Min(r => r.Value) ?? 0.0;
        }

        public static double MinS1ChannelVolt(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("S1.Volt.")).Min(r => r.Value) ?? 0.0;
        }

        public static double MaxChannelVolt(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("Volt.")).Max(r => r.Value) ?? 0.0;
        }

        public static double MaxS1ChannelVolt(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("S1.Volt.")).Max(r => r.Value) ?? 0.0;
        }

        public static bool MaxChannelVoltGreaterThanRating(this PowerDevice device, double percentage)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Contains("Volt.") && r.Rating.HasValue && r.Value > r.Rating * percentage) == true;
        }

        public static bool MaxS1ChannelVoltGreaterThanRating(this PowerDevice device, double percentage)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Contains("S1.Volt.") && r.Rating.HasValue && r.Value > r.Rating * percentage) == true;
        }
    }
}