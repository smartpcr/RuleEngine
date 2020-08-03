// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportValidationResultToKusto.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Cache;
    using Common.Kusto;
    using Common.Telemetry;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ExportValidationResultToKusto : IExportValidationResultToKusto
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly IDocDbRepository<DeviceValidationJob> jobRepo;
        private readonly IKustoClient kustoClient;
        private readonly ILogger<ExportValidationResultToKusto> logger;
        private readonly IDocDbRepository<DeviceValidationResult> docDbRepository;
        private readonly IDocDbRepository<RuleSet> ruleSetRepo;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;
        private readonly IDocDbRepository<ValidationRule> validationRuleRepo;

        public ExportValidationResultToKusto(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ExportValidationResultToKusto>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            docDbRepository = repoFactory.CreateRepository<DeviceValidationResult>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            jobRepo = repoFactory.CreateRepository<DeviceValidationJob>();
            ruleSetRepo = repoFactory.CreateRepository<RuleSet>();
            validationRuleRepo = repoFactory.CreateRepository<ValidationRule>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            kustoClient = serviceProvider.GetRequiredService<IKustoClient>();
        }

        public async Task<int> ExportToKusto(string runId, CancellationToken cancel)
        {
            logger.LogInformation($"exporting validation results for run: {runId}");
            using var scope = appTelemetry.StartOperation(this);

            var validationResults = await cache.GetOrUpdateAsync(
                $"{nameof(DeviceValidationResult)}-list-{runId}",
                async () => await docDbRepository.GetLastModificationTime($"c.runId = '{runId}'", cancel),
                async () =>
                {
                    var results = await docDbRepository.Query($"c.runId = '{runId}'");
                    var resultList = results.ToList();
                    return resultList;
                },
                cancel);
            logger.LogInformation($"total of {validationResults.Count} results retrieved for run");

            var validationRun = await runRepo.GetById(runId);
            var validationJob = await jobRepo.GetById(validationRun.JobId);
            var validationRules = new List<ValidationRule>();
            var ruleSets = new List<RuleSet>();
            if (validationJob.RuleSetIds?.Any() == true)
            {
                var rules = await validationRuleRepo.Query("c.ruleSetId in ({0})", validationJob.RuleSetIds);
                validationRules.AddRange(rules);

                ruleSets = (await ruleSetRepo.Query("c.id in ({0})", validationJob.RuleSetIds)).ToList();
            }

            if (validationJob.RuleIds?.Any() == true)
            {
                var rules = await validationRuleRepo.Query("c.id in ({0})", validationJob.RuleIds);
                validationRules.AddRange(rules);
            }
            logger.LogInformation($"total of {validationRules.Count} rules retrieved for run");

            try
            {
                await kustoClient.BulkInsert(nameof(DeviceValidationResult), validationResults, IngestMode.AppendOnly, "Id", default);
                logger.LogInformation($"validation results are saved to kusto table '{nameof(DeviceValidationResult)}'");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to add records to kusto table {nameof(DeviceValidationResult)}");
            }

            try
            {
                await kustoClient.BulkInsert(nameof(ValidationRule), validationRules, IngestMode.InsertNew, "Id", default);
                logger.LogInformation($"validation rules are saved to kusto table '{nameof(ValidationRule)}'");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to add records to kusto table {nameof(ValidationRule)}");
            }

            try
            {
                await kustoClient.BulkInsert(nameof(DeviceValidationRun), new List<DeviceValidationRun> {validationRun},
                    IngestMode.InsertNew, "Id", default);
                logger.LogInformation($"validation run is saved to kusto table '{nameof(DeviceValidationRun)}'");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to add records to kusto table {nameof(DeviceValidationRun)}");
            }

            try
            {
                await kustoClient.BulkInsert(nameof(DeviceValidationJob), new List<DeviceValidationJob> {validationJob},
                    IngestMode.InsertNew, "Id", default);
                logger.LogInformation($"validation job is saved to kusto table '{nameof(DeviceValidationJob)}'");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to add records to kusto table {nameof(DeviceValidationJob)}");
            }

            try
            {
                await kustoClient.BulkInsert(nameof(RuleSet), ruleSets, IngestMode.InsertNew, "Id", default);
                logger.LogInformation($"rule set is saved to kusto table '{nameof(RuleSet)}'");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to add records to kusto table {nameof(RuleSet)}");
            }

            return validationResults.Count;
        }
    }
}