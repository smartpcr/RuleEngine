namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;
    using Models;

    public class BlobRepoFactory
    {
        private readonly ILogger<BlobRepoFactory> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly ILoggerFactory loggerFactory;

        private readonly ConcurrentDictionary<string, object>
            _repositories = new ConcurrentDictionary<string, object>();

        public BlobRepoFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            this.loggerFactory = loggerFactory;
            logger = this.loggerFactory.CreateLogger<BlobRepoFactory>();
        }

        public IBlobRepository<T> CreateRepository<T>() where T : BaseEntity, new()
        {
            if (_repositories.TryGetValue(typeof(T).Name, out var found) && found is IBlobRepository<T> repo) return repo;

            logger.LogInformation($"Creating blob repo for type: {typeof(T).Name}");
            IBlobRepository<T> blobRepo = new BlobRepository<T>(serviceProvider, loggerFactory);
            _repositories.AddOrUpdate(typeof(T).Name, blobRepo, (k, v) => blobRepo);
            return blobRepo;
        }
    }
}