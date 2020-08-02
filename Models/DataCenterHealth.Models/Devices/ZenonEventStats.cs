//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ZenonEventStats.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

using System;

namespace DataCenterHealth.Models.Devices
{
    public class ZenonEventStats : BaseEntity
    {
        public string DcName { get; set; }
        public string DeviceName { get; set; }
        public double? HierarchyId { get; set; }
        public string DataPoint { get; set; }
        public string ChannelType { get; set; }
        public string Channel { get; set; }
        public DateTime MaxEventTime { get; set; }
        public DateTime MinEventTime { get; set; }
        public DateTime MaxPolledTime { get; set; }
        public DateTime MinPolledTime { get; set; }
        public int Count { get; set; }
        public double Avg { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double? Rating { get; set; }
    }

    public class ZenonLastReading : BaseEntity
    {
        public string DcName { get; set; }
        public string DeviceName { get; set; }
        public double? HierarchyId { get; set; }
        public string DataPoint { get; set; }
        public string ChannelType { get; set; }
        public string Channel { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime PolledTime { get; set; }
        public double Value { get; set; }
        public int? Quality { get; set; }
        public double? Rating { get; set; }
    }

    public class AllowedRange
    {
        public string DeviceName { get; set; }
        public string DataPoint { get; set; }
        public TimeSpan PollingInverval { get; set; }
        public decimal RangeMin { get; set; }
        public decimal RangeMax { get; set; }
    }
}