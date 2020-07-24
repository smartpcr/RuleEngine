// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KustoClientFactory.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using Auth;
    using Config;
    using global::Kusto.Data;
    using global::Kusto.Data.Common;
    using global::Kusto.Data.Net.Client;
    using global::Kusto.Ingest;
    using Microsoft.Extensions.Configuration;

    public class ClientFactory
    {
        public ClientFactory(IConfiguration configuration, KustoSettings kustoSettings = null)
        {
            var aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            kustoSettings = kustoSettings ?? configuration.GetConfiguredSettings<KustoSettings>();
            var authBuilder = new AadTokenProvider(aadSettings);
            var clientSecretCert = authBuilder.GetClientSecretOrCert();
            KustoConnectionStringBuilder kcsb;
            if (clientSecretCert.secret != null)
                kcsb = new KustoConnectionStringBuilder($"{kustoSettings.ClusterUrl}")
                    .WithAadApplicationKeyAuthentication(
                        aadSettings.ClientId,
                        clientSecretCert.secret,
                        aadSettings.Authority);
            else
                kcsb = new KustoConnectionStringBuilder($"{kustoSettings.ClusterUrl}")
                    .WithAadApplicationCertificateAuthentication(
                        aadSettings.ClientId,
                        clientSecretCert.cert,
                        aadSettings.Authority);
            QueryQueryClient = KustoClientFactory.CreateCslQueryProvider(kcsb);
            AdminClient = KustoClientFactory.CreateCslAdminProvider(kcsb);
            IngestClient = KustoIngestFactory.CreateDirectIngestClient(kcsb);
        }

        public ICslQueryProvider QueryQueryClient { get; }

        public ICslAdminProvider AdminClient { get; }

        public IKustoIngestClient IngestClient { get; }
    }
}