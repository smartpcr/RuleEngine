namespace Common.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBlobClient
    {
        /// <summary>
        /// create a new instance of client
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        IBlobClient SwitchContainer(string containerName);

        IEnumerable<string> ListContainersAsync(string prefix, CancellationToken cancel);

        string CurrentContainerName { get; }

        Task<IEnumerable<string>> ListBlobNamesAsync(DateTime? timeFilter, CancellationToken cancel);

        Task<T> GetAsync<T>(string blobName, CancellationToken cancel);

        Task<object> GetAsync(Type modelType, string blobName, CancellationToken cancel);

        Task<List<T>> GetAllAsync<T>(string prefix, Func<string, bool> filter, CancellationToken cancellationToken);

        Task GetAllAsync(
            Type modelType,
            string prefix,
            Func<string, bool> filter,
            Func<IList<object>, CancellationToken, Task> onBatchReceived,
            int batchSize = 100,
            CancellationToken cancellationToken = default);

        Task UploadAsync(string blobFolder, string blobName, string blobContent, CancellationToken cancellationToken);

        Task UploadBatchAsync<T>(string blobFolder, Func<T, string> getName, IList<T> list,
            CancellationToken cancellationToken);

        Task Upsert(string blobName, byte[] content, CancellationToken token);

        Task<string> DownloadAsync(string blobFolder, string blobName, string localFolder, CancellationToken cancellationToken);

        Task<long> CountAsync<T>(string prefix, Func<T, bool> filter, CancellationToken cancellationToken);
        Task<long> CountAsync(string prefix, CancellationToken cancel);

        Task<IList<T>> TryAcquireLease<T>(string blobFolder, int take, Action<T> update, TimeSpan timeout);

        Task ReleaseLease(string blobName);

        Task DeleteBlobs(string blobFolder, CancellationToken cancellationToken);

        Task Delete(string blobName, CancellationToken token);

        Task<BlobInfo> GetBlobInfo(string blobName, CancellationToken cancel);

        Task<DateTime> GetLastModificationTime(CancellationToken cancel);
    }
}