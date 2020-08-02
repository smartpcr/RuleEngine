namespace DataCenterHealth.Models.Sync
{
    using Common.DocDb;

    public class CosmosWriterSettings
    {
        public DocDbSettings DocDb { get; set; }
        public string CountBy { get; set; }
        public string UniqueField { get; set; }
    }
}