// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataPointValue.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class DataPointValue
    {
        public static bool DataPointValueIsNull(this PowerDevice device)
        {
            return device.LastReadings == null || device.LastReadings.Count == 0 || device.LastReadings.All(r => r.Value == 0);
        }

        public static bool DataPointValueIsNotNull(this PowerDevice device)
        {
            return device.LastReadings != null && device.LastReadings.Any(r => r.Value != 0);
        }

        public static bool DataPointValueOutOfRange(this PowerDevice device)
        {
            return device.LastReadings?.Any(r => r.Rating.HasValue && r.Value > r.Rating.Value) == true;
        }

        public static bool DataPointValueWithinRange(this PowerDevice device)
        {
            return device.LastReadings?.All(r => r.Rating.HasValue && r.Value <= r.Rating.Value) == true;
        }

        public static bool DataPointValueGreaterThanRatingPct(this PowerDevice device, double percentage)
        {
            return device.LastReadings?.Any(r => r.Rating.HasValue && r.Value > r.Rating.Value * percentage) == true;
        }
    }
}