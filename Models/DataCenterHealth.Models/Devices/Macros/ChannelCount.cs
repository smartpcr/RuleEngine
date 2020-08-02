// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelCount.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class ChannelCount
    {
        /// <summary>
        /// TotalChannels
        /// </summary>
        public static int TotalChannels(this PowerDevice device)
        {
            return device.LastReadings?.Count ?? 0;
        }

        /// <summary>
        /// TotalAmpsChannels
        /// </summary>
        public static int TotalAmpsChannels(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("Amps.")).Count() ?? 0;
        }

        /// <summary>
        /// TotalS1AmpsChannels
        /// </summary>
        public static int TotalS1AmpsChannels(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("S1.Amps.")).Count() ?? 0;
        }

        /// <summary>
        /// TotalVoltChannels
        /// </summary>
        public static int TotalVoltChannels(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("Volt.")).Count() ?? 0;
        }

        /// <summary>
        /// TotalS1VoltChannels
        /// </summary>
        public static int TotalS1VoltChannels(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("S1.Volt.")).Count() ?? 0;
        }
    }
}