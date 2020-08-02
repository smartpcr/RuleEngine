// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StalenessCheck.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices.Macros
{
    using System;
    using System.Linq;

    public static class StalenessCheck
    {
        public static bool IsStale(this PowerDevice device, int min)
        {
            return device.LastReadings?.Any(r => r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) == true;
        }

        public static bool CheckStaleness(this PowerDevice device, int min)
        {
            var skipStaleCheck = device.LastReadings?.Any() != true && device.DeviceName.Contains("IDF") && !device.DeviceName.Contains("-PM2");
            var readingStats = device.ReadingStats?.FirstOrDefault(r => r.DataPoint == "Energy.kWh");
            if (!skipStaleCheck)
            {
                var powerDataPoints = device.LastReadings?.Where(r => r.ChannelType.Equals("Pwr"));
                return powerDataPoints?.Any() != true ||
                       powerDataPoints?.Any(r => r.PolledTime < DateTime.UtcNow.AddMinutes(0 - min)) == true ||
                       (readingStats != null && readingStats.Min == readingStats.Max);
            }

            return false;
        }

        public static int StaledAmpsChannels(this PowerDevice device, int min)
        {
            return device.LastReadings?.Count(r =>
                r.DataPoint.Contains("Amps.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
        }

        public static int StaledS1AmpsChannels(this PowerDevice device, int min)
        {
            return device.LastReadings?.Count(r =>
                r.DataPoint.Contains("S1.Amps.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
        }

        public static bool AllAmpsChannelsAreStale(this PowerDevice device, int min)
        {
            var staledChannelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("Amps.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
            var channelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("Amps.", StringComparison.OrdinalIgnoreCase) ) ?? 0;
            return staledChannelCount == channelCount;
        }

        public static bool AllS1AmpsChannelsAreStale(this PowerDevice device, int min)
        {
            var staledChannelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("S1.Amps.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
            var channelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("S1.Amps.", StringComparison.OrdinalIgnoreCase) ) ?? 0;
            return staledChannelCount == channelCount;
        }

        public static int StaledVoltChannels(this PowerDevice device, int min)
        {
            return device.LastReadings?.Count(r =>
                r.DataPoint.Contains("Volt.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
        }

        public static int StaledS1VoltChannels(this PowerDevice device, int min)
        {
            return device.LastReadings?.Count(r =>
                r.DataPoint.Contains("S1.Volt.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
        }

        public static bool AllVoltChannelsAreStale(this PowerDevice device, int min)
        {
            var staledChannelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("Volt.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
            var channelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("Volt.", StringComparison.OrdinalIgnoreCase) ) ?? 0;
            return staledChannelCount == channelCount;
        }

        public static bool AllS1VoltChannelsAreStale(this PowerDevice device, int min)
        {
            var staledChannelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("S1.Volt.", StringComparison.OrdinalIgnoreCase) &&
                r.EventTime < DateTime.UtcNow.AddMinutes(0 - min)) ?? 0;
            var channelCount = device.LastReadings?.Count(r =>
                r.DataPoint.Contains("S1.Volt.", StringComparison.OrdinalIgnoreCase) ) ?? 0;
            return staledChannelCount == channelCount;
        }
    }
}