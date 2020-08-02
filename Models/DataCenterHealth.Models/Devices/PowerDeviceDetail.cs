// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceDetail.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;
    using System.Collections.Generic;

    public class PowerDeviceDetail
    {
        public DeviceGeneral General { get; set; }
        public DeviceLocation Location { get; set; }
        public DeviceHierarchy Hierarchy { get; set; }
        public DeviceRating Rating { get; set; }
        public CopaConfig CopaConfig { get; set; }
        public List<DataPoint> DataPoints { get; set; }
        public DeviceMetrics Metrics { get; set; }
    }

    public class DeviceGeneral
    {
        public string DeviceName { get; set; }
        public string DcName { get; set; }
        public string DeviceType { get; set; }
        public string Hierarchy { get; set; }
        public string DeviceState { get; set; }
        public string OnboardingMode { get; set; }
        public string DataType { get; set; }
    }

    public class DeviceLocation
    {
        public string ColoName { get; set; }
        public string PanelName { get; set; }
    }

    public class DeviceHierarchy
    {
        public string DeviceName { get; set; }
        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
        public string PrimaryParent { get; set; }
        public string SecondaryParent { get; set; }
        public string MaintenanceParent { get; set; }
        public string PowerSourceParent { get; set; }
        public string RedundantDeviceNames { get; set; }
        public PowerDeviceDetail PrimaryParentDevice { get; set; }
        public PowerDeviceDetail SecondaryParentDevice { get; set; }
        public PowerDeviceDetail MaintenanceParentDevice { get; set; }
        public PowerDeviceDetail PowerSourceParentDevice { get; set; }
        public PowerDeviceDetail RedundantDevice { get; set; }
        public List<PowerDeviceDetail> AllParentDevices { get; set; }
        public PowerDeviceDetail RootDevice { get; set; }
        public List<PowerDeviceDetail> ChildDevices { get; set; }
        public List<PowerDeviceDetail> SiblingDevices { get; set; }
    }

    public class DeviceRating
    {
        public bool IsMonitorable { get; set; }
        public decimal? Amperage { get; set; }
        public decimal? Voltage { get; set; }
        public decimal? RatedCapacity { get; set; }
        public decimal? DeratedCapacity { get; set; }
        public decimal? MaxItCapacity { get; set; }
        public decimal? ItCapacity { get; set; }
        public decimal? AmpRating { get; set; }
        public decimal? VoltageRating { get; set; }
        public decimal? KwRating { get; set; }
        public decimal? KvaRating { get; set; }
        public decimal? PowerFactor { get; set; }
        public decimal? DeRatingFactor { get; set; }
    }


    public class DeviceMetrics
    {
        public List<ZenonRawEvent> Events { get; set; }
        public DateTime? LastEventTime { get; set; }
        public (DateTime Start, DateTime End) TimeWindow { get; set; }
    }
}