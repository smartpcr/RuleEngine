namespace DataCenterHealth.Entities
{
    using DataCenterHealth.Models.Sync;

    public class SyncableEntity
    {
        public string EntityName { get; set; }
        public DataStorageType SourceType { get; set; }
        public DataStorageType TargetType { get; set; }
        public CosmosReaderSettings CosmosReaderSettings { get; set; }
        public BlobReaderSettings BlobReaderSettings { get; set; }
        public KustoReaderSettings KustoReaderSettings { get; set; }
        public CosmosWriterSettings CosmosWriterSettings { get; set; }
    }
}