// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;
    using Kusto.Cloud.Platform.Utils;

    public static class Source
    {
        /// <summary>
        /// TagName.StartsWith.S1
        /// </summary>
        public static bool IsS1Channel(this PowerDevice device)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Contains("S1.") || r.DataPoint.Contains("Source1.")) == true;
        }

        /// <summary>
        /// TagName.StartsWith.S2
        /// </summary>
        public static bool IsS2Channel(this PowerDevice device)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Contains("S2.") || r.DataPoint.Contains("Source2.")) == true;
        }

        /// <summary>
        /// TagName.StartsWith.Source2
        /// </summary>
        public static bool IsSource2Channel(this PowerDevice device)
        {
            return device.LastReadings?.Any(r => r.DataPoint.Contains("Source2.")) == true;
        }
    }
}