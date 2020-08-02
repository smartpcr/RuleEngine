namespace DataCenterHealth.Entities.Devices
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using DataCenterHealth.Entities.DataType;
    using DataCenterHealth.Entities.Hierarchy;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [CosmosReader("power-reference-prod", "power-reference-db", "PowerDevice", "power-reference-prod-authkey", "Name")]
    [CosmosWriter("xd-dev", "metadata", "mechanical_device", "xd-dev-authkey", "name", "name")]
    [TrackChange(true)]
    public class PowerDevice : BaseEntity
    {
        public string DeviceName { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                DeviceName = value;
            }
        }
        public string DeviceType { get; set; }
        public string DeviceState { get; set; }
        public string DcName { get; set; }
        public long DcCode { get; set; }
        public string ColoName { get; set; }
        public decimal? AmpRating { get; set; }
        public decimal? VoltageRating { get; set; }
        public decimal? KwRating { get; set; }
        public decimal? KvaRating { get; set; }
        public decimal? RatedCapacity { get; set; }
        public decimal? DeratedCapacity { get; set; }
        public decimal? DeployedCapacity { get; set; }
        public decimal? ReservedCapacity { get; set; }
        public int XCoordination { get; set; }
        public int YCoordination { get; set; }
        public string ParentDeviceName { get; set; }
        public string Hierarchy { get; set; }
        public decimal? PowerFactor { get; set; }
        [EnumDataType(typeof(CommunicationProtocol))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CommunicationProtocol CopaConfigType { get; set; }
        public CopaConfig CopaConfig { get; set; }
        public string Tile { get; set; }
        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
    }
}