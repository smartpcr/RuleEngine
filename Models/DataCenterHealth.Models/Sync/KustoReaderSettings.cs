namespace DataCenterHealth.Models.Sync
{
    using System.Collections.Generic;
    using Common.Kusto;

    public class KustoReaderSettings
    {
        public KustoSettings Kusto { get; set; }
        public string Query { get; set; }
        public List<string> OrderByFields { get; set; }
        public int ThrottlingSize { get; set; } = 500000;
    }
}