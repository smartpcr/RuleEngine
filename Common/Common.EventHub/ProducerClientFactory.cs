// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubClientFactory.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.EventHub
{
    using System;
    using System.Threading.Tasks;
    using Auth;
    using Azure.Identity;
    using Azure.Messaging.EventHubs.Producer;
    using Config;
    using KeyVault;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ProducerClientFactory
    {
        private readonly AadSettings aadSettings;
        private readonly EventHubSettings hubSettings;
        private readonly ILogger<ProducerClientFactory> logger;
        private readonly VaultSettings vaultSettings;

        public ProducerClientFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ProducerClientFactory>();
            hubSettings = configuration.GetConfiguredSettings<EventHubSettings>();
            aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();

            if (!TryCreateClientUsingMsi())
                if (!TryCreateClientUsingSpn())
                    if (!TryCreateClientFromKeyVault() &&
                        !string.IsNullOrEmpty(hubSettings.ConnectionStringSecretName))
                    {
                        if (!string.IsNullOrEmpty(hubSettings.ConnectionStringSecretName))
                        {
                            if (!TryCreateClientUsingConnStr()) throw new Exception("failed to create queue client");
                        }
                        else
                        {
                            throw new Exception("Invalid queue settings");
                        }
                    }
        }

        public EventHubProducerClient Producer { get; private set; }

        /// <summary>
        ///     running app/svc/pod/vm is assigned an identity (user-assigned, system-assigned)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingMsi()
        {
            logger.LogInformation("trying to access hub using msi...");
            try
            {
                Producer = new EventHubProducerClient(hubSettings.Namespace, hubSettings.HubName,
                    new DefaultAzureCredential());
                var partitionKeys = Producer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                logger.LogInformation(
                    $"Succeed to access hub using msi, partition keys: {string.Join(",", partitionKeys)}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to access hub {hubSettings.Namespace}/{hubSettings.HubName} using msi");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access storage, secret must be provided for spn authentication
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingSpn()
        {
            logger.LogInformation("trying to access hub using spn...");
            try
            {
                var authBuilder = new AadTokenProvider(aadSettings);
                var accessToken = authBuilder.GetAccessTokenAsync("https://storage.azure.com/").GetAwaiter()
                    .GetResult();
                var tokenCredential =
                    new ClientSecretCredential(aadSettings.TenantId, aadSettings.ClientId, accessToken);
                Producer = new EventHubProducerClient(hubSettings.Namespace, hubSettings.HubName, tokenCredential);
                var partitionKeys = Producer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                logger.LogInformation(
                    $"Succeed to access hub using spn, partition keys: {string.Join(",", partitionKeys)}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "faield to access hub using spn...");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access key vault, then retrieve sas/conn string for storage
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientFromKeyVault()
        {
            logger.LogInformation("trying to access hub from kv...");
            try
            {
                IKeyVaultClient kvClient;
                if (string.IsNullOrEmpty(aadSettings.ClientCertFile) &&
                    string.IsNullOrEmpty(aadSettings.ClientSecretFile))
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    kvClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(
                            azureServiceTokenProvider.KeyVaultTokenCallback));
                }
                else
                {
                    var authBuilder = new AadTokenProvider(aadSettings);

                    Task<string> AuthCallback(string authority, string resource, string scope)
                    {
                        return authBuilder.GetAccessTokenAsync(resource);
                    }

                    kvClient = new KeyVaultClient(AuthCallback);
                }

                var connStrSecret = kvClient
                    .GetSecretAsync(vaultSettings.VaultUrl, hubSettings.ConnectionStringSecretName).Result;
                Producer = new EventHubProducerClient(connStrSecret.Value, hubSettings.HubName);
                var partitionKeys = Producer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                logger.LogInformation(
                    $"Succeed to access hub using conn str from kv, partition keys: {string.Join(",", partitionKeys)}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "faield to access hub from kv...");
                return false;
            }
        }

        /// <summary>
        ///     connection string is provided as env variable (most unsecure)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingConnStr()
        {
            logger.LogInformation("trying to access hub using connection string...");
            try
            {
                var storageConnectionString = Environment.GetEnvironmentVariable(hubSettings.ConnectionStringEnvName);
                if (!string.IsNullOrEmpty(storageConnectionString))
                {
                    Producer = new EventHubProducerClient(storageConnectionString, hubSettings.HubName);
                    var partitionKeys = Producer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                    logger.LogInformation(
                        $"Succeed to access hub using conn str from env, partition keys: {string.Join(",", partitionKeys)}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "trying to access hub using connection string...");
                return false;
            }
        }
    }
}