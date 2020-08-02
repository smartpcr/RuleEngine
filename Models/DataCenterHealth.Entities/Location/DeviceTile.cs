namespace DataCenterHealth.Entities.Location
{
    using System.Collections.Generic;
    using DataCenterHealth.Models;
    using Newtonsoft.Json;

    [CosmosReader("power-reference-prod", "power-reference-db", "DeviceTiles", "power-reference-prod-authkey", "DeviceName")]
    [CosmosWriter("xd-dev", "metadata", "device_tile", "xd-dev-authkey", "deviceName", "deviceName")]
    [TrackChange(true)]
    public class DeviceTile : BaseEntity
    {
        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }

        public long DcCode { get; set; }

        public List<string> Tiles { get; set; }
    }
}