namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBlobRepository<T> where T : class, new()
    {
        Task<IEnumerable<string>> ListBlobs(DateTime? timeFilter, CancellationToken cancel);
        Task<T> Get(string blobName, CancellationToken cancel);
        Task<IEnumerable<T>> GetAll(CancellationToken cancel);
        Task<long> Count(CancellationToken cancel);
        Task<DateTime> GetLastModificationTime(CancellationToken cancel);
    }
}