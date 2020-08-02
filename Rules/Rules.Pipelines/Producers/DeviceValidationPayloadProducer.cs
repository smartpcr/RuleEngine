// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationPayloadProducer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Cache;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Kusto.Cloud.Platform.Utils;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Rules.Validations.Pipelines;

    public class DeviceValidationPayloadProducer : BasePayloadProducer<DeviceValidationPayload, DeviceValidationJob>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly IContextProvider<PowerDevice> contextProvider;
        private readonly Microsoft.Extensions.Logging.ILogger<DeviceValidationPayloadProducer> logger;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;
        private readonly IDocDbRepository<ValidationRule> validationRuleRepo;

        public DeviceValidationPayloadProducer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider)
        {
            logger = loggerFactory.CreateLogger<DeviceValidationPayloadProducer>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            validationRuleRepo = repoFactory.CreateRepository<ValidationRule>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            contextProvider = serviceProvider.GetRequiredService<IContextProvider<PowerDevice>>();
        }

        public override string Name => GetType().Name;

        public override async Task<IList<DeviceValidationPayload>> Produce(
            PipelineExecutionContext context,
            DeviceValidationJob validationJob,
            CancellationToken cancellationToken)
        {
            using var scope = appTelemetry.StartOperation(this, validationJob.ActivityId);

            var validationRuleList = new List<ValidationRule>();
            if (!string.IsNullOrEmpty(validationJob.RuleId))
            {
                var rule = await validationRuleRepo.GetById(validationJob.RuleId);
                validationRuleList.Add(rule);
            }
            else
            {
                validationRuleList = await cache.GetOrUpdateAsync(
                    $"validationrules-{validationJob.RuleSetId}",
                    async () => await validationRuleRepo.GetLastModificationTime(
                        $"c.ruleSetId = '{validationJob.RuleSetId}'", cancellationToken),
                    async () =>
                    {
                        var validationRules =
                            await validationRuleRepo.Query($"c.ruleSetId = '{validationJob.RuleSetId}'");
                        return validationRules.ToList();
                    },
                    cancellationToken);
            }

            logger.LogInformation($"total of {validationRuleList.Count} rules for rule set");
            appTelemetry.RecordMetric(
                $"{nameof(DeviceValidationWorker)}-validationrules",
                validationRuleList.Count,
                ("ruleSetId", validationJob.RuleSetId));

            List<PowerDevice> deviceList;
            if (validationJob.DeviceNames?.Count > 0)
            {
                var devices = await contextProvider.Provide(context, ContextProviderScope.Device, validationJob.DeviceNames, cancellationToken);
                deviceList = devices.ToList();
            }
            else
            {
                var devices = await contextProvider.Provide(context, ContextProviderScope.DC, new List<string> {validationJob.DcName}, cancellationToken);
                deviceList = devices.ToList();
            }
            context.DeviceLookup = deviceList.ToDictionary(d => d.DeviceName);

            logger.LogInformation(
                $"Total of {deviceList.Count} power devices found for dc: {validationJob.DcName}");
            appTelemetry.RecordMetric(
                $"{nameof(DeviceValidationWorker)}-devices",
                deviceList.Count,
                ("dcName", validationJob.DcName));

            var deviceValidationPayloads = new List<DeviceValidationPayload>();
            var run = await runRepo.GetById(context.RunId);
            foreach (var device in deviceList)
            {
                deviceValidationPayloads.AddRange(validationRuleList.Select(
                    rule => new DeviceValidationPayload(device, rule, validationJob.Id, run.Id)));
            }

            run.JobId = validationJob.Id;
            run.ExecutionTime = DateTime.UtcNow;
            run.TotalDevices = deviceList.Count;
            run.TotalRules = validationRuleList.Count;
            run.TotalPayloads = deviceValidationPayloads.Count;
            await runRepo.Update(run);

            appTelemetry.RecordMetric(
                $"{nameof(DeviceValidationWorker)}-payloads",
                deviceValidationPayloads.Count,
                ("dcName", validationJob.DcName),
                ("ruleSetId", validationJob.RuleSetId));

            return deviceValidationPayloads;
        }
    }
}