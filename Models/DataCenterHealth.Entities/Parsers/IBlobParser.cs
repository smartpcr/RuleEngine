namespace DataCenterHealth.Entities.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Storage;
    using Microsoft.Extensions.Logging;

    public interface IBlobParser
    {
        Type InputType { get; }
        Type OutputType { get; }
        Task<IEnumerable<string>> ListContainersAsync(CancellationToken cancel);
        Task<IEnumerable<string>> ListBlobNamesAsync(string containerName, DateTime? timeFilter, CancellationToken cancel);
        Task<IEnumerable<object>> ParseBlobAsync(string containerName, string blobName, CancellationToken cancel);
    }

    public interface IBlobParserFactory
    {
        string TypeName { get; }
        IBlobParser Create(IBlobClient blobClient, IServiceProvider serviceProvider, ILoggerFactory loggerFactory);
    }
}