//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="KustoRepoFactory.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;
    using Models;

    public class KustoRepoFactory
    {
        private readonly ConcurrentDictionary<string, object>
            _repositories = new ConcurrentDictionary<string, object>();

        private readonly ILogger<KustoRepoFactory> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly IServiceProvider serviceProvider;

        public KustoRepoFactory(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.serviceProvider = serviceProvider;
            logger = this.loggerFactory.CreateLogger<KustoRepoFactory>();
        }

        public IKustoRepo<T> CreateRepository<T>() where T : BaseEntity, new()
        {
            if (_repositories.TryGetValue(typeof(T).Name, out var found) && found is IKustoRepo<T> repo) return repo;

            logger.LogInformation($"Creating doc db repo for type: {typeof(T).Name}");
            IKustoRepo<T> repository = new KustoRepo<T>(serviceProvider, loggerFactory);
            _repositories.AddOrUpdate(typeof(T).Name, repository, (k, v) => repository);
            return repository;
        }
    }
}