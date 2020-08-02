namespace DataCenterHealth.Models.Sync
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using DataCenterHealth.Models.Summaries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [TrackChange(true, ChangeType.SyncSettings)]
    public class SyncSetting : TrackableEntity
    {
        public string Name { get; set; }
        public string Schedule { get; set; }
        public bool Enabled { get; set; }
        public string EntityTypeName { get; set; }
        [EnumDataType(typeof(DataStorageType))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataStorageType SourceType { get; set; }
        [EnumDataType(typeof(DataStorageType))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataStorageType TargetType { get; set; }

        public KustoReaderSettings KustoReader { get; set; }
        public CosmosReaderSettings CosmosReader { get; set; }
        public BlobReaderSettings BlobReader { get; set; }
        public CosmosWriterSettings CosmosWriter { get; set; }

        public DateTime? LastRunTime { get; set; }
        public string IdField { get; set; }
        public string TimestampField { get; set; }
        [EnumDataType(typeof(SyncStrategy))]
        [JsonConverter(typeof(StringEnumConverter))]
        public SyncStrategy Strategy { get; set; }
    }

    public enum SyncStrategy
    {
        Delta,
        Refresh
    }
}