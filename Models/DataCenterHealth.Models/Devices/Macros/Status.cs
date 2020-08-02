// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Status.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class Status
    {
        /// <summary>
        /// StatusOpenChannelValue
        /// </summary>
        public static double? GetStatusChannelValue(this PowerDevice device)
        {
            var statusOpenDataPoints = device.LastReadings?.Where(r => r.DataPoint.Contains("Status.Open"));
            var statusCloseDataPoints = device.LastReadings?.Where(r => r.DataPoint.Contains("Status.Close"));
            if (statusOpenDataPoints?.Any() == true)
            {
                return statusOpenDataPoints.First().Value;
            }

            if (statusCloseDataPoints?.Any() == true)
            {
                return statusCloseDataPoints.First().Value > 0.0 ? 0.0 : 1.0;
            }

            return null;
        }
    }
}