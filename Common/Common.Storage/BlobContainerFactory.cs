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
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Auth;
    using Azure.Identity;
    using Azure.Storage.Blobs;
    using Config;
    using KeyVault;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class BlobContainerFactory
    {
        private readonly AadSettings aadSettings;
        private readonly BlobStorageSettings blobSettings;
        private readonly ILogger<BlobContainerFactory> logger;
        private readonly VaultSettings vaultSettings;
        private readonly IKeyVaultClient kvClient;

        public BlobContainerFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory,
            BlobStorageSettings settings = null)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            blobSettings = settings ?? configuration.GetConfiguredSettings<BlobStorageSettings>();
            aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
            kvClient = serviceProvider.GetRequiredService<IKeyVaultClient>();
            logger = loggerFactory.CreateLogger<BlobContainerFactory>();

            switch (blobSettings.AuthMode)
            {
                case StorageAuthMode.Msi:
                    TryCreateUsingMsi();
                    break;
                case StorageAuthMode.Spn:
                    TryCreateUsingSpn();
                    break;
                case StorageAuthMode.SecretFromVault:
                    TryCreateFromKeyVault();
                    break;
                case StorageAuthMode.ConnStr:
                    TryCreateUsingConnStr();
                    break;
                default:
                    throw new NotSupportedException($"Storage auth mode: {blobSettings.AuthMode} is not supported");
            }
        }

        public BlobServiceClient BlobService { get; private set; }
        public BlobContainerClient ContainerClient { get; private set; }
        public Func<string, BlobContainerClient> CreateContainerClient { get; private set; }

        /// <summary>
        ///     running app/svc/pod/vm is assigned an identity (user-assigned, system-assigned)
        /// </summary>
        /// <returns></returns>
        private void TryCreateUsingMsi()
        {
            logger.LogInformation("trying to access blob using msi...");
            try
            {
                var containerClient = new BlobContainerClient(new Uri(blobSettings.ContainerEndpoint),
                    new DefaultAzureCredential());
                containerClient.CreateIfNotExists();

                TryRecreateTestBlob(containerClient);
                logger.LogInformation("Succeed to access blob using msi");
                ContainerClient = containerClient;
                BlobService = new BlobServiceClient(
                    new Uri($"https://{blobSettings.Account}.blob.core.windows.net/"),
                    new DefaultAzureCredential());
                CreateContainerClient = name =>
                    new BlobContainerClient(
                        new Uri($"https://{blobSettings.Account}.blob.core.windows.net/{name}"),
                        new DefaultAzureCredential());
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    $"failed to access blob {blobSettings.Account}/{blobSettings.Container} using msi\nerror: {ex.Message}");
            }
        }

        /// <summary>
        ///     using pre-configured spn to access storage, secret must be provided for spn authentication
        /// </summary>
        /// <returns></returns>
        private void TryCreateUsingSpn()
        {
            logger.LogInformation("trying to access blob using spn...");
            try
            {
                var authBuilder = new AadTokenProvider(aadSettings);
                Func<string, string> getSecretFromVault =
                    secretName => kvClient.GetSecretAsync(vaultSettings.VaultUrl, secretName).GetAwaiter().GetResult().Value;
                Func<string, X509Certificate2> getCertFromVault =
                    secretName => kvClient.GetX509CertificateAsync(vaultSettings.VaultUrl, secretName).GetAwaiter().GetResult();
                var clientCredential = authBuilder.GetClientCredential(getSecretFromVault, getCertFromVault);

                BlobContainerClient containerClient;
                if (clientCredential.secretCredential != null)
                {
                    containerClient = new BlobContainerClient(
                        new Uri(blobSettings.ContainerEndpoint),
                        clientCredential.secretCredential);
                    CreateContainerClient = name =>
                        new BlobContainerClient(
                            new Uri($"https://{blobSettings.Account}.blob.core.windows.net/{name}"),
                            clientCredential.secretCredential);
                    BlobService = new BlobServiceClient(
                        new Uri($"https://{blobSettings.Account}.blob.core.windows.net/"),
                        clientCredential.secretCredential);
                }
                else
                {
                    containerClient = new BlobContainerClient(
                        new Uri(blobSettings.ContainerEndpoint),
                        clientCredential.certCredential);
                    CreateContainerClient = name =>
                        new BlobContainerClient(
                            new Uri($"https://{blobSettings.Account}.blob.core.windows.net/{name}"),
                            clientCredential.certCredential);
                    BlobService = new BlobServiceClient(
                        new Uri($"https://{blobSettings.Account}.blob.core.windows.net/"),
                        clientCredential.certCredential);
                }

                TryRecreateTestBlob(containerClient);
                logger.LogInformation("Succeed to access blob using msi");
                ContainerClient = containerClient;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"faield to access blob using spn.\nerror:{ex.Message}");
            }
        }

        /// <summary>
        ///     using pre-configured spn to access key vault, then retrieve sas/conn string for storage
        /// </summary>
        /// <returns></returns>
        private void TryCreateFromKeyVault()
        {
            if (!string.IsNullOrEmpty(blobSettings.ConnectionStringSecretName))
            {
                logger.LogInformation("trying to access blob from kv...");
                try
                {
                    var connStrSecret = kvClient
                        .GetSecretAsync(vaultSettings.VaultUrl, blobSettings.ConnectionStringSecretName).Result;
                    var containerClient = new BlobContainerClient(connStrSecret.Value, blobSettings.Container);
                    containerClient.CreateIfNotExists();

                    TryRecreateTestBlob(containerClient);
                    logger.LogInformation("Succeed to access blob using msi");
                    ContainerClient = containerClient;
                    BlobService = new BlobServiceClient(connStrSecret.Value);
                    CreateContainerClient = name => new BlobContainerClient(connStrSecret.Value, name);
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"faield to access blob from kv. \nerror:{ex.Message}");
                    return;
                }
            }

            logger.LogWarning("vault secret for storage connection is not found");
        }

        /// <summary>
        ///     connection string is provided as env variable (most unsecure)
        /// </summary>
        /// <returns></returns>
        private void TryCreateUsingConnStr()
        {
            if (!string.IsNullOrEmpty(blobSettings.ConnectionStringEnvName))
            {
                logger.LogInformation("trying to access blob using connection string...");
                try
                {
                    var storageConnectionString =
                        Environment.GetEnvironmentVariable(blobSettings.ConnectionStringEnvName);
                    if (!string.IsNullOrEmpty(storageConnectionString))
                    {
                        var containerClient = new BlobContainerClient(storageConnectionString, blobSettings.Container);
                        containerClient.CreateIfNotExists();
                        TryRecreateTestBlob(containerClient);
                        ContainerClient = containerClient;
                        BlobService = new BlobServiceClient(storageConnectionString);
                        CreateContainerClient = name => new BlobContainerClient(storageConnectionString, name);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"trying to access blob using connection string. \nerror{ex.Message}");
                }
            }
        }

        private void TryRecreateTestBlob(BlobContainerClient containerClient)
        {
            var isContainerExists = containerClient.Exists();
            if (!isContainerExists.Value)
                throw new Exception("Blob container is either not created or authn/authz failed");

            var testBlob = containerClient.GetBlobClient("__test");
            testBlob.DeleteIfExists();
            var testData = JsonConvert.SerializeObject(new {name = "test"});
            testBlob.Upload(new MemoryStream(Encoding.UTF8.GetBytes(testData)));
            if (!testBlob.Exists()) throw new Exception("Unable to create blob");

            testBlob.Delete();
        }
    }
}