// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rating.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class Rating
    {
        public static bool RatingIsNotNull(this PowerDevice device)
        {
            return device.LastReadings?.All(r => r.Rating.HasValue) == true;
        }

        public static bool RatingIsNull(this PowerDevice device)
        {
            return device.LastReadings?.Any(r => r.Rating.HasValue) != true;
        }
    }
}