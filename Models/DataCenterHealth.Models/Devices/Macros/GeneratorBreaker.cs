// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneratorBreaker.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System.Linq;

    public static class GeneratorBreaker
    {
        /// <summary>
        /// GeneratorBreaker.Any
        /// </summary>
        public static bool IsGeneratorBreaker(this PowerDevice device)
        {
            return device.DevicePath == DevicePath.Self &&
                   device.DeviceFamily == DeviceFamily.PowerSource &&
                   (device.DeviceType == DeviceType.Breaker || device.DeviceType == DeviceType.Generator) &&
                   device.Hierarchy == "GEN";
        }

        /// <summary>
        /// GeneratorBreaker.Load
        /// </summary>
        public static double GetGeneratorBreakerLoad(this PowerDevice device)
        {
            var genBreakerLoad = IsGeneratorBreaker(device) ? device.LastReadings?.Where(r => r.DataPoint == "Pwr.kVA tot") : null;
            var genBreakerAmps = IsGeneratorBreaker(device) ? device.LastReadings?.Where(r => r.ChannelType == "Amps") : null;
            if (genBreakerLoad?.Any() == true)
            {
                return genBreakerLoad.First().Value;
            }

            if (genBreakerAmps?.Any() == true)
            {
                return genBreakerAmps.Sum(r => r.Value) * 110 / 1000;
            }

            return 0.0;
        }
    }
}