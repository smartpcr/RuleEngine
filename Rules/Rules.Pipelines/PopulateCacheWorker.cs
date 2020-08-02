namespace Rules.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Cache;
    using Common.Config;
    using Common.Kusto;
    using Common.Telemetry;
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class PopulateCacheWorker : BackgroundService
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ILogger<PopulateCacheWorker> logger;
        private readonly ICacheProvider cache;
        private readonly IKustoClient kustoClient;
        private readonly IKustoRepo<ArgusEnabledDc> enabledDcRepo;
        private readonly IDocDbRepository<DeviceValidationResult> resultRepo;
        private readonly IDocDbRepository<DeviceValidationJob> jobRepo;
        private readonly IDocDbRepository<PowerDevice> deviceRepo;
        private readonly IDocDbRepository<ValidationRule> ruleRepo;
        private readonly IDocDbRepository<RuleSet> ruleSetRepo;
        private readonly ReportsSettings reportsSettings;
        private DateTime lastValidationRunTime = DateTime.MinValue;

        public PopulateCacheWorker(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<PopulateCacheWorker>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            kustoClient = serviceProvider.GetRequiredService<IKustoClient>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            resultRepo = repoFactory.CreateRepository<DeviceValidationResult>();
            jobRepo = repoFactory.CreateRepository<DeviceValidationJob>();
            deviceRepo = repoFactory.CreateRepository<PowerDevice>();
            ruleRepo = repoFactory.CreateRepository<ValidationRule>();
            ruleSetRepo = repoFactory.CreateRepository<RuleSet>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            reportsSettings = config.GetConfiguredSettings<ReportsSettings>();

            var kustoRepoFactory = serviceProvider.GetRequiredService<KustoRepoFactory>();
            enabledDcRepo = kustoRepoFactory.CreateRepository<ArgusEnabledDc>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PopulateDataCenterValidationSummary(stoppingToken);
                await PopulateValidationResults(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task PopulateDataCenterValidationSummary(CancellationToken cancel)
        {
            try
            {
                var scheduleNames = string.Join(",", reportsSettings.JobScheduleNames);
                var checkNewRunsQuery = $"getLastRunTime(\"{scheduleNames}\")";
                var ruleOrRuleSetIds = await GetRuleOrRuleSetIds(cancel);

                var queries = new List<string>()
                {
                    $"dataCenterValidationQuery(\"{scheduleNames}\")",
                    $"deviceTypeValidationQuery(\"{scheduleNames}\")",
                    $"hierarchyValidationQuery(\"{scheduleNames}\")"
                };
                foreach (var id in ruleOrRuleSetIds)
                {
                    queries.Add($"dataCenterValidationQueryWithRuleFilter(\"{scheduleNames}\", \"{id}\")");
                    queries.Add($"deviceTypeValidationQueryWithRuleFilter(\"{scheduleNames}\", \"{id}\")");
                    queries.Add($"hierarchyValidationQueryWithRuleFilter(\"{scheduleNames}\", \"{id}\")");
                }

                var haveNewRuns = await CheckNewRuns(checkNewRunsQuery, cancel);
                if (haveNewRuns)
                {
                    appTelemetry.RecordMetric($"{GetType().Name}-triggered", 1);
                    using var scope = appTelemetry.StartOperation(this);
                    foreach (var query in queries)
                    {
                        if (!cancel.IsCancellationRequested)
                        {
                            await PopulateKustoQueryResult(query, cancel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to populate cache: {ex.Message}");
            }
        }

        private async Task PopulateValidationResults(CancellationToken cancel)
        {
            try
            {
                string enabledDcQuery = "ArgusEnabledDc";
                var enabledDcList = await cache.GetOrUpdateAsync(
                    "enabled-dcnames",
                    async () => await enabledDcRepo.GetLastModificationTime(enabledDcQuery, cancel),
                    async () =>
                    {
                        var dcs = await enabledDcRepo.ExecuteQuery(enabledDcQuery, (reader) => reader.GetString(0), cancel);
                        return dcs.ToList();
                    }, cancel);
                logger.LogInformation($"total of {enabledDcList.Count} data centers are enabled for argus monitoring");

                var ruleOrRuleSetIds = await GetRuleOrRuleSetIds(cancel);

                foreach (var dcName in enabledDcList)
                {
                    foreach (var ruleOrRuleSetId in ruleOrRuleSetIds)
                    {
                        await PopulateValidationDetails(dcName, ruleOrRuleSetId, cancel);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to populate cache: {ex.Message}");
            }
        }

        private async Task<bool> CheckNewRuns(string query, CancellationToken cancel)
        {
            var results = await kustoClient.ExecuteQuery<LastRunResult>(query, TimeSpan.FromMinutes(30), cancel);
            var lastRunResults = results as LastRunResult[] ?? results.ToArray();
            if (lastRunResults.Any(result => result.LastRunTime > lastValidationRunTime))
            {
                lastValidationRunTime = lastRunResults.Max(r => r.LastRunTime);
                return true;
            }

            return false;
        }

        private async Task<List<string>> GetRuleOrRuleSetIds(CancellationToken cancel)
        {
            var ruleOrRuleSetIds = await cache.GetOrUpdateAsync(
                "coderule-ruleset-ids",
                async () => await ruleRepo.GetLastModificationTime(null, cancel),
                async () =>
                {
                    var ids = new List<string>();
                    var allRuleSets = await ruleSetRepo.GetAll();
                    foreach (var ruleSet in allRuleSets)
                    {
                        if (ruleSet.Type == RuleType.CodeRule)
                        {
                            var allCodeRules = await ruleRepo.Query($"c.ruleSetId = '{ruleSet.Id}'");
                            ids.AddRange(allCodeRules.Select(r=>r.Id));
                            ids.Add(ruleSet.Id);
                        }
                        else
                        {
                            ids.Add(ruleSet.Id);
                        }
                    }

                    return ids;
                }, cancel);
            logger.LogInformation($"total of {ruleOrRuleSetIds.Count} code rules and rule sets found");
            return ruleOrRuleSetIds;
        }

        private async Task PopulateKustoQueryResult(string query, CancellationToken cancel)
        {
            var cacheKey = $"list-{nameof(DataCenterValidation)}-{query.GetHashCode()}";
            var validationList = await cache.GetOrUpdateAsync(
                cacheKey,
                async () => await resultRepo.GetLastModificationTime(null, cancel),
                async () =>
                {
                    logger.LogInformation($"kusto query: {query}");
                    var validations = await kustoClient.ExecuteQuery<DataCenterValidation>(query, TimeSpan.FromMinutes(30), cancel);
                    return validations.ToList();
                },
                cancel);
            logger.LogInformation($"total of {validationList.Count} validations retrieved");
        }

        private async Task PopulateValidationDetails(string dcName, string ruleOrRuleSetId, CancellationToken cancel)
        {
            var deviceList = await cache.GetOrUpdateAsync(
                $"list-{nameof(PowerDevice)}-{dcName}",
                async () => await deviceRepo.GetLastModificationTime(null, default),
                async () =>
                {
                    var deviceQuery = "";
                    if (!string.IsNullOrEmpty(dcName)) deviceQuery = $"c.dcName = '{dcName}'";
                    var devices = string.IsNullOrEmpty(deviceQuery)
                        ? await deviceRepo.GetAll()
                        : await deviceRepo.Query(deviceQuery);
                    return devices.ToList();
                }, default);
            logger.LogInformation($"total of {deviceList.Count} devices retrieved");

            var request = $"Get /validationResults/filtered?dcName={dcName}&deviceType=undefined&hierarchy=undefined&ruIds={ruleOrRuleSetId}";
            var validationDetailList = await cache.GetOrUpdateAsync(
                $"list-{nameof(DeviceValidationDetail)}-{request.GetHashCode()}",
                async () => await resultRepo.GetLastModificationTime(null, cancel),
                async () =>
                {
                    var ruleIdList = new List<string>();
                    var ruleFilters = GetRuleFilters(ruleOrRuleSetId);
                    if (ruleFilters.ruleSetIds.Count > 0)
                    {
                        var rules = await ruleRepo.Query("c.ruleSetId in ({0})", ruleFilters.ruleSetIds);
                        ruleIdList = rules.Select(r => r.Id).ToList();
                        if (ruleFilters.ruleIds.Count > 0)
                        {
                            ruleIdList.AddRange(ruleFilters.ruleIds);
                            ruleIdList = ruleIdList.Distinct().ToList();
                        }
                    }
                    if (ruleFilters.ruleIds.Count > 0)
                    {
                        ruleIdList.AddRange(ruleFilters.ruleIds);
                    }

                    var latestRunIds = new List<string>();
                    var kustoQuery = $"getLatestValidationRuns('{dcName}','','','{string.Join(",", ruleIdList)}')";
                    var reader = await kustoClient.ExecuteReader(kustoQuery);
                    while (reader.Read())
                    {
                        latestRunIds.Add(reader["RunId"].ToString());
                    }
                    reader.Close();
                    logger.LogInformation($"total of {latestRunIds.Count} runs retrieved");

                    var resultsFromLatestRun = (await resultRepo.Query("c.runId in ({0})", latestRunIds)).ToList();
                    logger.LogInformation($"total of {resultsFromLatestRun.Count} results retrieved");

                    var uniqueRuleIds = resultsFromLatestRun.Select(r => r.ValidationRuleId).Distinct().ToList();
                    var validationRules = await ruleRepo.Query("c.id in ({0})", uniqueRuleIds);
                    var ruleLookup = validationRules.ToDictionary(r => r.Id);
                    logger.LogInformation($"total of {ruleLookup.Count} rules are applied");

                    var deviceLookup = deviceList.ToDictionary(d => d.DeviceName);
                    var details = resultsFromLatestRun
                        .Where(r => ruleIdList.Count == 0 || ruleIdList.Contains(r.ValidationRuleId))
                        .Select(r => ToDetail(r, deviceLookup, ruleLookup)).ToList();
                    return details;
                },
                cancel);
            logger.LogInformation($"total of {validationDetailList.Count} validation details found");
        }

        private (List<string> ruleSetIds, List<string> ruleIds) GetRuleFilters(string selectedRuleIds)
        {
            var ruleSetIds = new List<string>();
            var ruleIds = new List<string>();

            if (!string.IsNullOrEmpty(selectedRuleIds))
            {
                var ids = selectedRuleIds.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var id in ids)
                    if (id.IndexOf("/", StringComparison.OrdinalIgnoreCase) > 0 && id.Split(new[] {'/'}).Length == 2)
                    {
                        var ruleId = id.Substring(id.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                        ruleIds.Add(ruleId);
                    }
                    else
                    {
                        ruleSetIds.Add(id);
                    }
            }

            return (ruleSetIds, ruleIds);
        }

        private DeviceValidationDetail ToDetail(
            DeviceValidationResult result,
            Dictionary<string, PowerDevice> deviceLookup,
            Dictionary<string, ValidationRule> ruleLookup)
        {
            var detail = new DeviceValidationDetail
            {
                Assert = result.Assert,
                DeviceName = result.DeviceName,
                Score = result.Score,
                ExecutionTime = result.ExecutionTime,
                Evidences = new List<DeviceValidationEvidence>()
            };

            if (deviceLookup.ContainsKey(detail.DeviceName))
            {
                var device = deviceLookup[detail.DeviceName];
                detail.DcName = device.DcName;
                detail.DeviceType = device.DeviceType.ToString();
                detail.Hierarchy = device.Hierarchy;
                detail.DeviceState = device.DeviceState.ToString();
            }

            if (ruleLookup.ContainsKey(result.ValidationRuleId))
                detail.RuleName = ruleLookup[result.ValidationRuleId].Name;

            try
            {
                if (!string.IsNullOrEmpty(result.Error))
                {
                    if (result.Error.StartsWith("[{\"propertyPath\":") || result.Error.StartsWith("[{\"errorCode\":"))
                    {
                        var evidences = JsonConvert.DeserializeObject<List<DeviceValidationEvidence>>(result.Error);
                        detail.Evidences = evidences;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"failed to parse evidence: {ex.Message}");
            }

            return detail;
        }

        public class LastRunResult
        {
            public DateTime LastRunTime { get; set; }
        }
    }
}