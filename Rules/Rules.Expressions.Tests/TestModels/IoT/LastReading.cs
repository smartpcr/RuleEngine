// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZenonLastReading.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System;

    public class LastReading
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
        public int KWhStaleness { get; set; }
    }
}