// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Amps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class Amps
    {
        /// <summary>
        /// TotalChannelAmps
        /// </summary>
        public static double TotalChannelAmps(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("Amps.")).Sum(r => r.Value) ?? 0.0;
        }

        /// <summary>
        /// TotalS1ChannelAmps
        /// </summary>
        public static double TotalS1ChannelAmps(this PowerDevice device)
        {
            return device.LastReadings?.Where(r => r.DataPoint.Contains("S1.Amps.")).Sum(r => r.Value) ?? 0.0;
        }

    }
}