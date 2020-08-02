namespace DataCenterHealth.Models.Sync
{
    using Common.Storage;

    public class BlobReaderSettings
    {
        public BlobStorageSettings Blob { get; set; }
        public string BlobParserTypeName { get; set; }
    }
}