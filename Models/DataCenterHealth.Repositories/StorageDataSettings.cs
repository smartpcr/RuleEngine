namespace DataCenterHealth.Repositories
{
    using Common.Storage;
    using Models.Devices;

    public class StorageDataSettings
    {
        [MappedModel(typeof(ZenonEventStats))]
        public BlobStorageSettings ZenonEventStats { get; set; }
    }
}