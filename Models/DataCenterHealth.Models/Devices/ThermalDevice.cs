// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThermalDevice.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    public class ThermalDevice : Device
    {
        public string ProjectName { get; set; }
        public string ManufacturerDeviceId { get; set; }
        public uint OIDPosition { get; set; }
        public SensorType SensorType { get; set; }
        public string WatchDogSensorName { get; set; }
        public SensorPosition SensorPosition { get; set; }
        public SensorVersion SensorVersion { get; set; }
        public SensorLocation SensorLocation { get; set; }
        public SiteType SiteType { get; set; }
        public string Location { get; set; }
        public string ImpactArea { get; set; }
        public long ColoId { get; set; }
        public string Rack { get; set; }
        public string Switch { get; set; }
        public string PortIdentifier { get; set; }
        public string Netmask { get; set; }
        public string GatewayIPv4 { get; set; }
        public string DriverName { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int Prefix { get; set; }
    }
}