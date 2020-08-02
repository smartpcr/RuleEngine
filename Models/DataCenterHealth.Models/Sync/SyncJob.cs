namespace DataCenterHealth.Models.Sync
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using DataCenterHealth.Models.Summaries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [TrackExecution(true, ExecutionType.MetaDataSync)]
    public class SyncJob : BaseEntity
    {
        public string SyncSettingId { get; set; }
        public string Name { get; set; }
        public string EntityTypeName { get; set; }
        public string IdField { get; set; }
        public string TimestampField { get; set; }

        [EnumDataType(typeof(DataStorageType))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataStorageType SourceType { get; set; }
        [EnumDataType(typeof(DataStorageType))]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataStorageType TargetType { get; set; }
        [EnumDataType(typeof(SyncStrategy))]
        [JsonConverter(typeof(StringEnumConverter))]
        public SyncStrategy Strategy { get; set; }

        public KustoReaderSettings KustoReader { get; set; }
        public CosmosReaderSettings CosmosReader { get; set; }
        public BlobReaderSettings BlobReader { get; set; }
        public CosmosWriterSettings CosmosWriter { get; set; }
        public bool? Succeed { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TimeSpan? Span { get; set; }
        public string Error { get; set; }
        public int? TotalAdded { get; set; }
        public int? TotalUpdated { get; set; }
        public int? TotalDeleted { get; set; }
    }
}