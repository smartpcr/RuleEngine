// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RepositoryFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Concurrent;
    using global::Repositories;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Models;

    public class RepositoryFactory
    {
        private readonly ILogger<RepositoryFactory> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly ILoggerFactory loggerFactory;

        private readonly ConcurrentDictionary<string, object>
            _repositories = new ConcurrentDictionary<string, object>();

        public RepositoryFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            this.loggerFactory = loggerFactory;
            logger = this.loggerFactory.CreateLogger<RepositoryFactory>();
        }

        public IDocDbRepository<T> CreateRepository<T>() where T : BaseEntity, new()
        {
            if (_repositories.TryGetValue(typeof(T).Name, out var found) && found is IDocDbRepository<T> repo) return repo;

            logger.LogInformation($"Creating doc db repo for type: {typeof(T).Name}");
            IDocDbRepository<T> docDbRepository = new DocDbRepository<T>(serviceProvider, loggerFactory);
            _repositories.AddOrUpdate(typeof(T).Name, docDbRepository, (k, v) => docDbRepository);
            return docDbRepository;
        }
    }
}