namespace DataCenterHealth.Entities.DataType
{
    using System;
    using System.Collections.Generic;
    using DataCenterHealth.Models;
    using Newtonsoft.Json;

    [CosmosReader("power-reference-prod", "power-reference-db", "DataSchema", "power-reference-prod-authkey", "name")]
    [CosmosWriter("xd-dev", "metadata", "data_schema", "xd-dev-authkey", "name", "name")]
    public class DataSchema : BaseEntity
    {
        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public int PollIntervalMs { get; set; }
        public int PollTimeoutMs { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public List<DataPointSchema> DataPoints { get; set; }
    }

    public class DataPointSchema
    {
        public string Name { get; set; }
        public string OemName { get; set; }
        public string DataType { get; set; }
        public string Address { get; set; }
        public int Bit { get; set; }
        public int Offset { get; set; }
        public int Multiplier { get; set; }
        public string Unit { get; set; }
    }
}