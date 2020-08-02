// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThermalDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Devices
{
    using DataCenterHealth.Models;

    [CosmosReader("power-reference-prod", "power-reference-db", "ThermalDevice", "power-reference-prod-authkey", "Name")]
    [CosmosWriter("xd-dev", "metadata", "thermal_device", "xd-dev-authkey", "name", "name")]
    [TrackChange(true)]
    public class ThermalDevice : BaseEntity
    {
        public string Name { get; set; }
        public string DcName { get; set; }
        public long DcCode { get; set; }
        public string ProjectName { get; set; }
        public string ManufactureDeviceId { get; set; }
        public int OIDPosition { get; set; }
        public int SensorType { get; set; }
        public string WatchDogSensorName { get; set; }
        public int SensorPosition { get; set; }
        public int SensorVersion { get; set; }
        public int SensorLocation { get; set; }
        public int SiteType { get; set; }
        public string Location { get; set; }
        public int ColoId { get; set; }
        public string Rack { get; set; }
        public string Switch { get; set; }
        public string PortIdentifier { get; set; }
        public string Netmask { get; set; }
        public string GatewayIPV4 { get; set; }
        public string DriverName { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int Prefix { get; set; }
        public int OnboardingMode { get; set; }
    }
}