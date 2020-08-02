// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsumerClientFactory.cs" company="Microsoft">
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
    using Azure.Messaging.EventHubs.Consumer;
    using Config;
    using KeyVault;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ConsumerClientFactory
    {
        private readonly AadSettings _aadSettings;
        private readonly string _consumerGroupName;
        private readonly EventHubSettings _hubSettings;
        private readonly ILogger<ProducerClientFactory> _logger;
        private readonly VaultSettings _vaultSettings;

        public ConsumerClientFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProducerClientFactory>();
            _hubSettings = configuration.GetConfiguredSettings<EventHubSettings>();
            _consumerGroupName = Consumer.ConsumerGroup ?? EventHubConsumerClient.DefaultConsumerGroupName;
            _aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            _vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
            _logger.LogInformation($"using consumer group: {_consumerGroupName}");

            if (!TryCreateClientUsingMsi())
                if (!TryCreateClientUsingSpn())
                    if (!TryCreateClientFromKeyVault() &&
                        !string.IsNullOrEmpty(_hubSettings.ConnectionStringSecretName))
                    {
                        if (!string.IsNullOrEmpty(_hubSettings.ConnectionStringSecretName))
                        {
                            if (!TryCreateClientUsingConnStr()) throw new Exception("failed to create queue client");
                        }
                        else
                        {
                            throw new Exception("Invalid queue settings");
                        }
                    }
        }

        public EventHubConsumerClient Consumer { get; private set; }

        /// <summary>
        ///     running app/svc/pod/vm is assigned an identity (user-assigned, system-assigned)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingMsi()
        {
            _logger.LogInformation("trying to access hub using msi...");
            try
            {
                Consumer = new EventHubConsumerClient(_consumerGroupName, _hubSettings.Namespace, _hubSettings.HubName,
                    new DefaultAzureCredential());
                var partitionKeys = Consumer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                _logger.LogInformation(
                    $"Succeed to access hub using msi, partition keys: {string.Join(",", partitionKeys)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"failed to access hub {_hubSettings.Namespace}/{_hubSettings.HubName} using msi");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access storage, secret must be provided for spn authentication
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingSpn()
        {
            _logger.LogInformation("trying to access hub using spn...");
            try
            {
                var authBuilder = new AadTokenProvider(_aadSettings);
                var accessToken = authBuilder.GetAccessTokenAsync("https://storage.azure.com/").GetAwaiter()
                    .GetResult();
                var tokenCredential =
                    new ClientSecretCredential(_aadSettings.TenantId, _aadSettings.ClientId, accessToken);
                Consumer = new EventHubConsumerClient(_consumerGroupName, _hubSettings.Namespace, _hubSettings.HubName,
                    tokenCredential);
                var partitionKeys = Consumer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                _logger.LogInformation(
                    $"Succeed to access hub using spn, partition keys: {string.Join(",", partitionKeys)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "faield to access hub using spn...");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access key vault, then retrieve sas/conn string for storage
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientFromKeyVault()
        {
            _logger.LogInformation("trying to access hub from kv...");
            try
            {
                var authBuilder = new AadTokenProvider(_aadSettings);

                Task<string> AuthCallback(string authority, string resource, string scope)
                {
                    return authBuilder.GetAccessTokenAsync(resource);
                }

                var kvClient = new KeyVaultClient(AuthCallback);
                var connStrSecret = kvClient
                    .GetSecretAsync(_vaultSettings.VaultUrl, _hubSettings.ConnectionStringSecretName).Result;
                Consumer = new EventHubConsumerClient(_consumerGroupName, connStrSecret.Value, _hubSettings.HubName);
                var partitionKeys = Consumer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                _logger.LogInformation(
                    $"Succeed to access hub using conn str from kv, partition keys: {string.Join(",", partitionKeys)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "faield to access hub from kv...");
                return false;
            }
        }

        /// <summary>
        ///     connection string is provided as env variable (most unsecure)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingConnStr()
        {
            _logger.LogInformation("trying to access hub using connection string...");
            try
            {
                var storageConnectionString = Environment.GetEnvironmentVariable(_hubSettings.ConnectionStringEnvName);
                if (!string.IsNullOrEmpty(storageConnectionString))
                {
                    Consumer = new EventHubConsumerClient(_consumerGroupName, storageConnectionString,
                        _hubSettings.HubName);
                    var partitionKeys = Consumer.GetPartitionIdsAsync().GetAwaiter().GetResult();
                    _logger.LogInformation(
                        $"Succeed to access hub using conn str from env, partition keys: {string.Join(",", partitionKeys)}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "trying to access hub using connection string...");
                return false;
            }
        }
    }
}