// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Quality.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class QualityCheck
    {
        public static bool QualityEquals(this PowerDevice device, int quality)
        {
            return device.LastReadings?.Any(r => r.Quality == quality) == true;
        }

        public static bool QualityNotEquals(this PowerDevice device, int quality)
        {
            return device.LastReadings?.All(r => r.Quality != quality) == true;
        }
    }
}