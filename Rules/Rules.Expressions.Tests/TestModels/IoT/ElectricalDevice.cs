// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System.Collections.Generic;

    public class ElectricalDevice
    {
        public string DeviceName { get; set; }
        public string LocationName { get; set; }
        public DeviceType DeviceType { get; set; }
        public string Hierarchy { get; set; }
        
        public List<ReadingStats> ReadingStats { get; set; }
        public List<LastReading> LastReadings { get; set; }
    }
}