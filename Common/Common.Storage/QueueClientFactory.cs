// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueClientFactory.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Common.Storage
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Auth;
    using Azure.Identity;
    using Azure.Storage.Queues;
    using Config;
    using KeyVault;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class QueueClientFactory
    {
        private readonly AadSettings aadSettings;
        private readonly ILogger<QueueClientFactory> logger;
        private readonly QueueSettings queueSettings;
        private readonly VaultSettings vaultSettings;

        public QueueClientFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<QueueClientFactory>();
            queueSettings = configuration.GetConfiguredSettings<QueueSettings>();
            aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();

            if (!TryCreateClientUsingMsi())
                if (!TryCreateClientUsingSpn())
                    if (!TryCreateClientFromKeyVault())
                    {
                        if (!string.IsNullOrEmpty(queueSettings.ConnectionStringSecretName))
                        {
                            if (!TryCreateClientUsingConnStr()) throw new Exception("failed to create queue client");
                        }
                        else
                        {
                            throw new Exception("Invalid queue settings");
                        }
                    }
        }

        public QueueClient QueueClient { get; private set; }

        public QueueClient DeadLetterQueueClient { get; private set; }

        /// <summary>
        ///     running app/svc/pod/vm is assigned an identity (user-assigned, system-assigned)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingMsi()
        {
            logger.LogInformation("trying to access queue using msi...");
            try
            {
                var queueServiceClient = new QueueServiceClient(new Uri(queueSettings.AccountServiceUrl),
                    new DefaultAzureCredential());
                VerifyQueueServiceClient(queueServiceClient, queueSettings.QueueName);
                QueueClient = queueServiceClient.GetQueueClient(queueSettings.QueueName);
                DeadLetterQueueClient = queueServiceClient.GetQueueClient(queueSettings.DeadLetterQueueName);
                logger.LogInformation("Succeed to access queue using msi");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    $"failed to access queue {queueSettings.Account}/{queueSettings.QueueName} using msi. \nerror{ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access storage, secret must be provided for spn authentication
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingSpn()
        {
            logger.LogInformation("trying to access queue using spn...");
            try
            {
                var authBuilder = new AadTokenProvider(aadSettings);
                var clientCredential = authBuilder.GetClientCredential();
                QueueServiceClient queueServiceClient;
                if (clientCredential.secretCredential != null)
                    queueServiceClient = new QueueServiceClient(new Uri(queueSettings.AccountServiceUrl),
                        clientCredential.secretCredential);
                else
                    queueServiceClient = new QueueServiceClient(new Uri(queueSettings.AccountServiceUrl),
                        clientCredential.certCredential);
                VerifyQueueServiceClient(queueServiceClient, queueSettings.QueueName);

                QueueClient = queueServiceClient.GetQueueClient(queueSettings.QueueName);
                DeadLetterQueueClient = queueServiceClient.GetQueueClient(queueSettings.DeadLetterQueueName);
                using var availableQueues = queueServiceClient.GetQueues().GetEnumerator();
                while (availableQueues.MoveNext())
                    if (availableQueues.Current?.Name == queueSettings.QueueName)
                    {
                        logger.LogInformation("Succeed to access queue using spn");
                        return true;
                    }

                logger.LogInformation($"Unabe to find queue with name {queueSettings.QueueName}");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"faield to access queue using spn. \nerror{ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access key vault, then retrieve sas/conn string for storage
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientFromKeyVault()
        {
            if (!string.IsNullOrEmpty(queueSettings.ConnectionStringSecretName))
            {
                logger.LogInformation("trying to access queue from kv...");
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
                        .GetSecretAsync(vaultSettings.VaultUrl, queueSettings.ConnectionStringSecretName).Result;
                    var queueServiceClient = new QueueServiceClient(connStrSecret.Value, new QueueClientOptions());
                    VerifyQueueServiceClient(queueServiceClient, queueSettings.QueueName);

                    QueueClient = queueServiceClient.GetQueueClient(queueSettings.QueueName);
                    DeadLetterQueueClient = queueServiceClient.GetQueueClient(queueSettings.DeadLetterQueueName);
                    logger.LogInformation("Succeed to access queue using connstr from key vault");
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError($"faield to access queue from kv. \nerror{ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        ///     connection string is provided as env variable (most unsecure)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateClientUsingConnStr()
        {
            logger.LogInformation("trying to access queue using connection string...");
            try
            {
                var storageConnectionString = Environment.GetEnvironmentVariable(queueSettings.ConnectionStringEnvName);
                if (!string.IsNullOrEmpty(storageConnectionString))
                {
                    var queueServiceClient = new QueueServiceClient(storageConnectionString, new QueueClientOptions());
                    VerifyQueueServiceClient(queueServiceClient, queueSettings.QueueName);

                    QueueClient = queueServiceClient.GetQueueClient(queueSettings.QueueName);
                    DeadLetterQueueClient = queueServiceClient.GetQueueClient(queueSettings.DeadLetterQueueName);
                    logger.LogInformation("Succeed to access queue using connstr from env");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "trying to access queue using connection string...");
                return false;
            }
        }

        private void VerifyQueueServiceClient(QueueServiceClient queueServiceClient, string queueName)
        {
            try
            {
                using var availableQueues = queueServiceClient.GetQueues().GetEnumerator();
                var foundQueue = false;
                while (availableQueues.MoveNext())
                    if (availableQueues.Current?.Name == queueName)
                    {
                        foundQueue = true;
                        logger.LogInformation("Succeed to access queue using spn");
                        break;
                    }

                if (!foundQueue)
                {
                    var error = $"Unabe to find queue with name {queueSettings.QueueName}";
                    logger.LogError(error);
                    throw new Exception(error);
                }

                var queueClientToTest = queueServiceClient.GetQueueClient(queueName);
                var properties = queueClientToTest.GetProperties();
                var queueLength = properties.Value.ApproximateMessagesCount;
                logger.LogInformation($"queue {queueClientToTest.Name} have {queueLength} messages");
                var fetchPropStatus = properties.GetRawResponse().Status;
                if (!IsSuccessStatusCode(fetchPropStatus)) throw new Exception("Failed to fetch queue properties");

                var testQueueName = "test";
                var testQueueClient = queueServiceClient.GetQueueClient(testQueueName);
                var createQueueResponse = testQueueClient.Create();
                if (!IsSuccessStatusCode(createQueueResponse.Status))
                    throw new Exception("Failed to create test queue");

                var sendMsgResponse = testQueueClient.SendMessage("test");
                var statusCode = sendMsgResponse.GetRawResponse().Status;
                if (!IsSuccessStatusCode(statusCode)) throw new Exception("Failed to send message to test queue");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to storage queue");
                throw;
            }
        }

        private bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode == (int) HttpStatusCode.OK ||
                   statusCode == (int) HttpStatusCode.Created ||
                   statusCode == (int) HttpStatusCode.Accepted ||
                   statusCode == (int) HttpStatusCode.NoContent;
        }
    }
}