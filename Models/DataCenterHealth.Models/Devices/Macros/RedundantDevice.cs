// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedundantDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class RedundantDevice
    {
        public static bool HasRedundantDevice(this PowerDevice device)
        {
            return device.RedundantDevice != null;
        }

        public static bool HasNoRedundantDevice(this PowerDevice device)
        {
            return device.RedundantDevice == null;
        }

        public static double GetRedundantDeviceKwValue(this PowerDevice device)
        {
            return device.RedundantDevice?.LastReadings.FirstOrDefault(r => r.ChannelType == "Pwr" && r.Channel == "kW tot")?.Value ?? 0.0;
        }

        public static bool RedundantDeviceKwValueIsNull(this PowerDevice device)
        {
            return device.RedundantDevice != null && device.RedundantDevice.LastReadings.FirstOrDefault(r => r.ChannelType == "Pwr" && r.Channel == "kW tot") == null;
        }

        public static bool RedundantDeviceKwValueIsNotNull(this PowerDevice device)
        {
            var reading = device.RedundantDevice?.LastReadings?.FirstOrDefault(r => r.ChannelType == "Pwr" && r.Channel == "kW tot");
            return reading != null && reading.Value > 0;
        }

        /// <summary>
        /// Value>RedundantDeviceInformation.KwRating-RedundantDeviceInformation.KwValue
        /// </summary>
        public static bool KwValueGreaterThanRedundant(this PowerDevice device)
        {
            var reading = device.LastReadings?.FirstOrDefault(r => r.ChannelType == "Pwr" && r.Channel == "kW tot");
            var redundantReading = device.RedundantDevice?.LastReadings?.FirstOrDefault(r => r.ChannelType == "Pwr" && r.Channel == "kW tot");
            return reading != null && redundantReading != null && redundantReading.Rating.HasValue &&
                   reading.Value > redundantReading.Rating - redundantReading.Value;
        }
    }
}