// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZenonEventStats.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System;

    public class ReadingStats
    {
        public string DcName { get; set; }
        public string DeviceName { get; set; }
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
}