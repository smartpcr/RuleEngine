// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocDbSettings.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.DocDb
{
    using System;

    public class DocDbSettings
    {
        public string Account { get; set; }
        public string Db { get; set; }
        public string Collection { get; set; }
        public string AuthKeySecret { get; set; }
        public bool CollectMetrics { get; set; }
        public Uri AccountUri => new Uri($"https://{Account}.documents.azure.com:443/");
    }
}