namespace DataCenterHealth.Entities.Parsers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Storage;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class BlobParserFactory
    {
        private readonly ILogger<BlobParserFactory> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly ILoggerFactory loggerFactory;
        private readonly ConcurrentDictionary<string, IBlobParserFactory> parserFactories
            = new ConcurrentDictionary<string, IBlobParserFactory>();

        public BlobParserFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<BlobParserFactory>();
            InitiateParserFactories();
        }

        public List<string> GetAllParserTypes()
        {
            return parserFactories.Keys.ToList();
        }

        public IBlobParser CreateParser(string parserTypeName, BlobStorageSettings blobStorageSettings)
        {
            if (parserFactories.TryGetValue(parserTypeName, out var found))
            {
                var blobClient = new BlobClient(serviceProvider, loggerFactory, new OptionsWrapper<BlobStorageSettings>(blobStorageSettings));
                return found.Create(blobClient, serviceProvider, loggerFactory);
            }

            logger.LogError($"failed to find blob parser for type {parserTypeName}");
            return null;
        }

        private void InitiateParserFactories()
        {
            var interfaceName = typeof(IBlobParserFactory).FullName;
            if (interfaceName == null) return;
            var factories = typeof(IBlobParserFactory).Assembly.GetTypes()
                .Where(t => t.GetInterface(interfaceName) != null)
                .Where(t => t.GetConstructor(new Type[0]) != null)
                .ToList();
            foreach (var factory in factories)
            {
                if (Activator.CreateInstance(factory) is IBlobParserFactory factoryInstance)
                {
                    parserFactories.AddOrUpdate(factoryInstance.TypeName, factoryInstance, (k, v) => factoryInstance);
                }
            }
        }
    }
}