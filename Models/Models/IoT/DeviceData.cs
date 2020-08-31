// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceData.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT
{
    using System.Collections.Generic;

    public class DeviceData
    {
        public string DeviceName { get; set; }
        public string LocationName { get; set; }
        public DeviceType DeviceType { get; set; }
        public string Hierarchy { get; set; }
        public State DeviceState { get; set; }
        public string PanelName { get; set; }
        public string ColoName { get; set; }
        
        public string PrimaryParent { get; set; }
        public string SecondaryParent { get; set; }
        public string MaintenanceParent { get; set; }
        public string RedundantDeviceNames { get; set; }
        
        public double? Amperage { get; set; }
        public double? Voltage { get; set; }
        public double? RatedCapacity { get; set; }
        public double? DeRatedCapacity { get; set; }
        public double? PowerFactor { get; set; }
        public double? DeRatingFactor { get; set; }
        public double? KwRating { get; set; }
        public double? KvaRating { get; set; }
        public double? ItCapacity { get; set; }
        public double? MaxitCapacity { get; set; }
        
        public string DataType { get; set; }
        public string DriverName { get; set; }
        public string ConnectionName { get; set; }
        public string IpAddress { get; set; }
        
        public List<ReadingStats> ReadingStats { get; set; }
        public List<LastReading> LastReadings { get; set; }
        public List<DataPoint> DataPoints { get; set; }
    }
}