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
    using System.Text;
    using System.Threading.Tasks;
    using Auth;
    using Azure.Identity;
    using Azure.Storage.Blobs;
    using Config;
    using KeyVault;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class BlobContainerFactory
    {
        private readonly AadSettings aadSettings;
        private readonly BlobStorageSettings blobSettings;
        private readonly ILogger<BlobContainerFactory> logger;
        private readonly VaultSettings vaultSettings;

        public BlobContainerFactory(IConfiguration configuration, ILoggerFactory loggerFactory,
            BlobStorageSettings settings = null)
        {
            blobSettings = settings ?? configuration.GetConfiguredSettings<BlobStorageSettings>();
            aadSettings = configuration.GetConfiguredSettings<AadSettings>();
            vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
            logger = loggerFactory.CreateLogger<BlobContainerFactory>();

            if (!TryCreateUsingMsi())
                if (!TryCreateUsingSpn())
                    if (!TryCreateFromKeyVault())
                        TryCreateUsingConnStr();
        }

        public Azure.Storage.Blobs.BlobServiceClient BlobService { get; private set; }
        public BlobContainerClient ContainerClient { get; private set; }
        public Func<string, BlobContainerClient> CreateContainerClient { get; private set; }

        /// <summary>
        ///     running app/svc/pod/vm is assigned an identity (user-assigned, system-assigned)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateUsingMsi()
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
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    $"failed to access blob {blobSettings.Account}/{blobSettings.Container} using msi\nerror: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access storage, secret must be provided for spn authentication
        /// </summary>
        /// <returns></returns>
        private bool TryCreateUsingSpn()
        {
            logger.LogInformation("trying to access blob using spn...");
            try
            {
                var authBuilder = new AadTokenProvider(aadSettings);
                var clientCredential = authBuilder.GetClientCredential();

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

                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"faield to access blob using spn.\nerror:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        ///     using pre-configured spn to access key vault, then retrieve sas/conn string for storage
        /// </summary>
        /// <returns></returns>
        private bool TryCreateFromKeyVault()
        {
            if (!string.IsNullOrEmpty(blobSettings.ConnectionStringSecretName))
            {
                logger.LogInformation("trying to access blob from kv...");
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
                        .GetSecretAsync(vaultSettings.VaultUrl, blobSettings.ConnectionStringSecretName).Result;
                    var containerClient = new BlobContainerClient(connStrSecret.Value, blobSettings.Container);
                    containerClient.CreateIfNotExists();

                    TryRecreateTestBlob(containerClient);
                    logger.LogInformation("Succeed to access blob using msi");
                    ContainerClient = containerClient;
                    BlobService = new BlobServiceClient(connStrSecret.Value);
                    CreateContainerClient = name => new BlobContainerClient(connStrSecret.Value, name);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"faield to access blob from kv. \nerror:{ex.Message}");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        ///     connection string is provided as env variable (most unsecure)
        /// </summary>
        /// <returns></returns>
        private bool TryCreateUsingConnStr()
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
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"trying to access blob using connection string. \nerror{ex.Message}");
                    return false;
                }
            }

            return false;
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