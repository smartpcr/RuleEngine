// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KustoSettings.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using Auth;

    public class KustoSettings
    {
        private string clusterUrl;
        public string ClusterName { get; set; }
        public string RegionName { get; set; }
        public string DbName { get; set; }
        public string TableName { get; set; }
        public AuthMode AuthMode { get; set; } = AuthMode.SPN;

        public string ClusterUrl
        {
            get =>
                clusterUrl ?? (string.IsNullOrEmpty(RegionName)
                    ? $"https://{ClusterName}.kusto.windows.net"
                    : $"https://{ClusterName}.{RegionName}.kusto.windows.net");
            set => clusterUrl = value;
        }
    }
}