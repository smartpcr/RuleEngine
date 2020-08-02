namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Config;
    using Common.DocDb;
    using Common.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class BlobRepository<T> : IBlobRepository<T> where T: class, new()
    {
        private readonly ILogger<BlobRepository<T>> logger;
        private readonly IBlobClient client;

        public BlobRepository(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<BlobRepository<T>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var storageSettings = configuration.GetConfiguredSettings<StorageDataSettings>();
            var prop = storageSettings.GetType().GetProperties()
                .FirstOrDefault(p =>
                {
                    var customAttr = p.GetCustomAttribute<MappedModelAttribute>();
                    if (customAttr != null && customAttr.ModelType == typeof(T)) return true;

                    return false;
                });
            if (prop == null) throw new Exception($"Missing backend mapping for model: {typeof(T).Name}");

            var blobStorageSettings = prop.GetValue(storageSettings) as BlobStorageSettings;
            if (blobStorageSettings == null)
            {
                throw new Exception($"Missing backend mapping for model: {typeof(T).Name}");
            }

            client = new BlobClient(serviceProvider, loggerFactory, new OptionsWrapper<BlobStorageSettings>(blobStorageSettings));
        }

        public async Task<IEnumerable<string>> ListBlobs(DateTime? timeFilter, CancellationToken cancel)
        {
            return await client.ListBlobNamesAsync(timeFilter, cancel);
        }

        public async Task<T> Get(string blobName, CancellationToken cancel)
        {
            return await client.GetAsync<T>(blobName, cancel);
        }

        public async Task<IEnumerable<T>> GetAll(CancellationToken cancel)
        {
            return await client.GetAllAsync<T>(null, null, cancel);
        }

        public async Task<long> Count(CancellationToken cancel)
        {
            return await client.CountAsync<T>(null, null, cancel);
        }

        public async Task<DateTime> GetLastModificationTime(CancellationToken cancel)
        {
            return await client.GetLastModificationTime(cancel);
        }
    }
}