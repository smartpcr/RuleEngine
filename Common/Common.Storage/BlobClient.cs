// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlobClient.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
    using Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    public class BlobClient : IBlobClient
    {
        private readonly BlobContainerClient containerClient;
        private readonly BlobServiceClient blobService;
        private readonly ILogger<BlobClient> logger;
        private readonly Func<string, BlobContainerClient> createClient;

        public BlobClient(
            IConfiguration config,
            ILoggerFactory loggerFactory,
            IOptions<BlobStorageSettings> blobStorageSettings)
        {
            logger = loggerFactory.CreateLogger<BlobClient>();
            var settings = blobStorageSettings != null
                ? blobStorageSettings.Value
                : config.GetConfiguredSettings<BlobStorageSettings>();
            logger.LogInformation(
                $"accessing blob (account={settings.Account}, container={settings.Container}) using default azure credential");
            var factory = new BlobContainerFactory(config, loggerFactory, settings);
            containerClient = factory.ContainerClient;
            blobService = factory.BlobService;
            createClient = factory.CreateContainerClient;

            if (containerClient == null)
            {
                var error = $"failed to create blobContainerClient: {settings.Account}/{settings.Container}";
                logger.LogError(error);
                throw new Exception(error);
            }
        }

        private BlobClient(BlobContainerClient containerClient, ILogger<BlobClient> logger)
        {
            this.containerClient = containerClient;
            this.logger = logger;
        }

        public IBlobClient SwitchContainer(string containerName)
        {
            var newClient = createClient(containerName);
            return new BlobClient(newClient, logger);
        }

        public IEnumerable<string> ListContainersAsync(string prefix, CancellationToken cancel)
        {
            var containerNames = new List<string>();
            var containers = blobService.GetBlobContainers(prefix: prefix, cancellationToken: cancel);
            using var containerEnumerator = containers.GetEnumerator();
            while (containerEnumerator.MoveNext())
            {
                if (containerEnumerator.Current != null)
                {
                    containerNames.Add(containerEnumerator.Current.Name);
                }
            }

            return containerNames;
        }

        public string CurrentContainerName => containerClient.Name;

        public async Task<IEnumerable<string>> ListBlobNamesAsync(DateTime? timeFilter, CancellationToken cancel)
        {
            var blobs = containerClient.GetBlobsAsync().GetAsyncEnumerator(cancel);
            var output = new List<string>();
            while (await blobs.MoveNextAsync())
            {
                if (timeFilter.HasValue && blobs.Current.Properties.LastModified.HasValue)
                {
                    if (blobs.Current.Properties.LastModified.Value.Date > timeFilter.Value)
                    {
                        output.Add(blobs.Current.Name);
                    }
                }
                else
                {
                    output.Add(blobs.Current.Name);
                }
            }
            return output;
        }

        public async Task<T> GetAsync<T>(string blobName, CancellationToken cancel)
        {
            logger.LogInformation($"getting blob {blobName}...");
            var blobClient = containerClient.GetBlobClient(blobName);
            var downloadInfo = await blobClient.DownloadAsync(cancel);
            var tempFilePath = Path.GetTempFileName();
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            await using var fs = File.OpenWrite(tempFilePath);
            await downloadInfo.Value.Content.CopyToAsync(fs, cancel);
            var json = Encoding.UTF8.GetString(await File.ReadAllBytesAsync(tempFilePath, cancel));
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public async Task<object> GetAsync(Type modelType, string blobName, CancellationToken cancel)
        {
            logger.LogInformation($"getting blob {blobName}...");
            var blobClient = containerClient.GetBlobClient(blobName);
            var downloadInfo = await blobClient.DownloadAsync(cancel);
            var tempFilePath = Path.GetTempFileName();
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            await using var fs = File.OpenWrite(tempFilePath);
            await downloadInfo.Value.Content.CopyToAsync(fs, cancel);
            var json = Encoding.UTF8.GetString(await File.ReadAllBytesAsync(tempFilePath, cancel));
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            var obj = JsonConvert.DeserializeObject(json, modelType);
            return obj;
        }

        public async Task<List<T>> GetAllAsync<T>(string prefix, Func<string, bool> filter, CancellationToken cancellationToken)
        {
            var blobs = containerClient.GetBlobsAsync(prefix: prefix).GetAsyncEnumerator(cancellationToken);
            var output = new List<T>();
            while (await blobs.MoveNextAsync())
            {
                if (filter == null || filter(blobs.Current.Name))
                {
                    var blobClient = containerClient.GetBlobClient(blobs.Current.Name);
                    var blobContent = await blobClient.DownloadAsync(cancellationToken);
                    using var reader = new StreamReader(blobContent.Value.Content);
                    var json = await reader.ReadToEndAsync();
                    output.Add(JsonConvert.DeserializeObject<T>(json));
                }
            }

            return output;
        }

        public async Task GetAllAsync(
            Type modelType,
            string prefix,
            Func<string, bool> filter,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            int batchSize = 100,
            CancellationToken cancellationToken = default)
        {
            var blobs = containerClient.GetBlobsAsync(prefix: prefix).GetAsyncEnumerator(cancellationToken);
            var batch = new List<object>();
            while (await blobs.MoveNextAsync())
            {
                if (filter == null || filter(blobs.Current.Name))
                {
                    var blobClient = containerClient.GetBlobClient(blobs.Current.Name);
                    var blobContent = await blobClient.DownloadAsync(cancellationToken);
                    using var reader = new StreamReader(blobContent.Value.Content);
                    var json = await reader.ReadToEndAsync();
                    batch.Add(JsonConvert.DeserializeObject(json, modelType));

                    if (batch.Count >= batchSize)
                    {
                        await onBatchReceived(batch, cancellationToken);
                        batch = new List<object>();
                    }
                }
            }

            if (batch.Count > 0)
            {
                await onBatchReceived(batch, cancellationToken);
                batch.Clear();
            }
        }

        public async Task UploadAsync(string blobFolder, string blobName, string blobContent, CancellationToken cancellationToken)
        {
            var blobPath = string.IsNullOrEmpty(blobFolder) ? blobName : $"{blobFolder}/{blobName}";
            logger.LogInformation($"uploading {blobPath}...");
            var blobClient = containerClient.GetBlobClient(blobPath);
            var uploadResponse = await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(blobContent)),
                cancellationToken);
            logger.LogInformation($"uploaded blob: {blobPath}, modified on: {uploadResponse.Value.LastModified}");
        }

        public async Task UploadBatchAsync<T>(string blobFolder, Func<T, string> getName, IList<T> list, CancellationToken cancellationToken)
        {
            logger.LogInformation($"uploading {list.Count} files to {blobFolder}...");
            foreach (var item in list)
            {
                var blobName = getName(item);
                var blobPath = $"{blobFolder}/{blobName}";
                var blobClient = containerClient.GetBlobClient(blobPath);
                var blobContent = JsonConvert.SerializeObject(item);
                await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(blobContent)), cancellationToken);
            }

            logger.LogInformation($"uploaded {list.Count} files to {blobFolder}.");
        }

        public async Task Upsert(string blobName, byte[] content, CancellationToken token)
        {
            logger.LogInformation($"upsert blob: {blobName}, length: {content.Length}");
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: token);
            await blobClient.UploadAsync(new MemoryStream(content), token);
        }

        public async Task<string> DownloadAsync(string blobFolder, string blobName, string localFolder, CancellationToken cancellationToken)
        {
            var blobPath = string.IsNullOrEmpty(blobFolder) ? blobName : $"{blobFolder}/{blobName}";
            logger.LogInformation($"downloading {blobPath}...");
            var blobClient = containerClient.GetBlobClient(blobPath);
            var downloadInfo = await blobClient.DownloadAsync(cancellationToken);
            var filePath = Path.Combine(localFolder, blobName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await using var fs = File.OpenWrite(filePath);
            await downloadInfo.Value.Content.CopyToAsync(fs, cancellationToken);
            logger.LogInformation($"blob written to {filePath}");
            return filePath;
        }

        public async Task<long> CountAsync<T>(string prefix, Func<T, bool> filter, CancellationToken cancellationToken)
        {
            var blobs = containerClient.GetBlobsAsync(prefix: prefix).GetAsyncEnumerator(cancellationToken);
            var count = 0;
            while (await blobs.MoveNextAsync())
            {
                if (filter == null)
                {
                    count++;
                }
                else
                {
                    var blobClient = containerClient.GetBlobClient(blobs.Current.Name);
                    var blobContent = await blobClient.DownloadAsync(cancellationToken);
                    using var reader = new StreamReader(blobContent.Value.Content);
                    var json = await reader.ReadToEndAsync();
                    var item = JsonConvert.DeserializeObject<T>(json);
                    if (filter(item)) count++;
                }
            }

            return count;
        }

        public async Task<long> CountAsync(string prefix, CancellationToken cancel)
        {
            var blobs = containerClient.GetBlobsAsync(prefix: prefix).GetAsyncEnumerator(cancel);
            var count = 0;
            while (await blobs.MoveNextAsync())
            {
                count++;
            }
            return count;
        }

        public async Task<IList<T>> TryAcquireLease<T>(string blobFolder, int take, Action<T> update, TimeSpan timeout)
        {
            logger.LogInformation($"trying to acquire lease on blobs in folder: {blobFolder}...");
            var blobs = containerClient.GetBlobsAsync(prefix: blobFolder).GetAsyncEnumerator();

            timeout = timeout == default ? TimeSpan.FromMinutes(5) : timeout;
            var output = new Dictionary<string, T>();
            try
            {
                while (await blobs.MoveNextAsync())
                {
                    var blobClient = containerClient.GetBlobClient(blobs.Current.Name);
                    var leaseClient = blobClient.GetBlobLeaseClient();
                    await leaseClient.AcquireAsync(timeout);
                    var blobContent = await blobClient.DownloadAsync();
                    using var reader = new StreamReader(blobContent.Value.Content);
                    var json = await reader.ReadToEndAsync();
                    var item = JsonConvert.DeserializeObject<T>(json);
                    if (update != null)
                    {
                        update(item);
                        await blobClient.UploadAsync(
                            new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item))),
                            new CancellationToken());
                    }

                    output.Add(blobs.Current.Name, item);
                    if (take == 0 || output.Count >= take) break;
                }

                return output.Values.ToList();
            }
            catch
            {
                logger.LogInformation($"lease rejected on blobs within folder: {blobFolder}...");
                foreach (var blobName in output.Keys) await ReleaseLease(blobName);
                return new List<T>();
            }
        }

        public async Task ReleaseLease(string blobName)
        {
            logger.LogInformation($"trying to acquire lease on blob: {blobName}...");
            var blobClient = containerClient.GetBlobClient(blobName);
            var leaseClient = blobClient.GetBlobLeaseClient();
            await leaseClient.ReleaseAsync();
        }

        public async Task DeleteBlobs(string blobFolder, CancellationToken cancellationToken)
        {
            var blobs = containerClient.GetBlobsAsync(prefix: blobFolder).GetAsyncEnumerator(cancellationToken);
            var blobNames = new List<string>();
            while (await blobs.MoveNextAsync()) blobNames.Add(blobs.Current.Name);

            foreach (var blobName in blobNames)
            {
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots,
                    cancellationToken: cancellationToken);
            }
        }

        public async Task Delete(string blobName, CancellationToken token)
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: token);
        }

        public async Task<BlobInfo> GetBlobInfo(string blobName, CancellationToken cancel)
        {
            logger.LogInformation($"trying to get blob: {blobName}...");
            var blobClient = containerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync(cancel)) return null;

            var props = await blobClient.GetPropertiesAsync(null, cancel);
            var blobInfo = new BlobInfo
            {
                Name = blobName,
                Size = props.Value.ContentLength,
                CreatedOn = props.Value.CreatedOn,
                IsLeased = props.Value.LeaseState != LeaseState.Available &&
                           props.Value.LeaseState != LeaseState.Expired,
                TimeToLive = default // not supported
            };

            return blobInfo;
        }

        public async Task<DateTime> GetLastModificationTime(CancellationToken cancel)
        {
            var blobs = containerClient.GetBlobsAsync().GetAsyncEnumerator(cancel);
            var lastModificationTime = DateTimeOffset.MinValue;
            while (await blobs.MoveNextAsync())
            {
                if (blobs.Current.Properties.LastModified.HasValue && blobs.Current.Properties.LastModified.Value > lastModificationTime)
                {
                    lastModificationTime = blobs.Current.Properties.LastModified.Value;
                }
            }

            return lastModificationTime.DateTime;
        }
    }
}