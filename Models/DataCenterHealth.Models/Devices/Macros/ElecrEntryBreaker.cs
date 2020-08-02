// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElecrEntryBreaker.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class ElecrEntryBreaker
    {
        /// <summary>
        /// ElecrEntryBreaker.Any
        /// </summary>
        public static bool IsElecrEntryBreaker(this PowerDevice device)
        {
            return device.DevicePath.HasValue && device.DevicePath.Value == DevicePath.Elecr &&
                   device.DeviceType == DeviceType.Breaker &&
                   device.HierarchyId.HasValue && device.HierarchyId > 8;
        }

        /// <summary>
        /// ElecrEntryBreaker.Load
        /// </summary>
        public static double GetElecrEntryBreakerLoad(this PowerDevice device)
        {
            var elecrEntryBreakerKwTot = IsElecrEntryBreaker(device) ? device.LastReadings.Where(r => r.DataPoint == "Pwr.kVA tot").ToList() : null;
            var elecrEntryBreakerAmps = IsElecrEntryBreaker(device) ? device.LastReadings.Where(r => r.ChannelType == "Amps").ToList() : null;
            if (elecrEntryBreakerKwTot?.Any() == true)
            {
                return elecrEntryBreakerKwTot.FirstOrDefault().Value;
            }
            if (elecrEntryBreakerAmps?.Any() == true)
            {
                return elecrEntryBreakerAmps.Sum(r => r.Value) * 110 / 10000;
            }

            return 0.0;
        }

        /// <summary>
        /// ElecrColoLoadDevices.Any
        /// </summary>
        public static bool IsElecrColoDevice(this PowerDevice device)
        {
            return device.DevicePath == DevicePath.Elecr &&
                   (device.DeviceType == DeviceType.Breaker || device.DeviceType == DeviceType.PowerMeter) &&
                   (device.HierarchyId > 6 && device.HierarchyId <= 8 || device.HierarchyId == 4);
        }

        /// <summary>
        /// ElecrColoLoadDevices.Load
        /// </summary>
        public static double GetElecrColoDeviceLoad(this PowerDevice device)
        {
            var kwTotDataPoints = IsElecrColoDevice(device) ? device.LastReadings?.Where(r => r.DataPoint == "Pwr.kVA tot") : null;
            var ampsDataPoints = IsElecrColoDevice(device) ? device.LastReadings?.Where(r => r.ChannelType == "Amps") : null;
            if (kwTotDataPoints?.Any() == true)
            {
                return kwTotDataPoints.First().Value;
            }

            if (ampsDataPoints?.Any() == true)
            {
                return ampsDataPoints.Sum(r => r.Value) * 110 / 1000;
            }

            return 0.0;
        }
    }
}