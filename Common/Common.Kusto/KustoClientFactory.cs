// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KustoClientFactory.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Auth;
    using Common.KeyVault;
    using Config;
    using global::Kusto.Data;
    using global::Kusto.Data.Common;
    using global::Kusto.Ingest;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class KustoClientFactory
    {
        public KustoClientFactory(IServiceProvider serviceProvider, KustoSettings kustoSettings = null)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            kustoSettings = kustoSettings ?? configuration.GetConfiguredSettings<KustoSettings>();
            var vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
            var kvClient = serviceProvider.GetRequiredService<IKeyVaultClient>();
            Func<string, string> getSecretFromVault =
                secretName => kvClient.GetSecretAsync(vaultSettings.VaultUrl, secretName).GetAwaiter().GetResult().Value;;
            Func<string, X509Certificate2> getCertFromVault =
                secretName => kvClient.GetX509CertificateAsync(vaultSettings.VaultUrl, secretName).GetAwaiter().GetResult();
            var authBuilder = new AadTokenProvider(aadSettings);
            var clientSecretCert = authBuilder.GetClientSecretOrCert(getSecretFromVault, getCertFromVault);
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
            QueryQueryClient = global::Kusto.Data.Net.Client.KustoClientFactory.CreateCslQueryProvider(kcsb);
            AdminClient = global::Kusto.Data.Net.Client.KustoClientFactory.CreateCslAdminProvider(kcsb);
            IngestClient = KustoIngestFactory.CreateDirectIngestClient(kcsb);
        }

        public ICslQueryProvider QueryQueryClient { get; }

        public ICslAdminProvider AdminClient { get; }

        public IKustoIngestClient IngestClient { get; }
    }
}