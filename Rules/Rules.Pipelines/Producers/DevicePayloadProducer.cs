// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DevicePayloadProducer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DevicePayloadProducer : BasePayloadProducer<PowerDevice>
    {
        private ILogger<DevicePayloadProducer> logger;
        private readonly IAppTelemetry appTelemetry;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;
        private readonly IContextProvider<PowerDevice> contextProvider;
        
        public DevicePayloadProducer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            logger = loggerFactory.CreateLogger<DevicePayloadProducer>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            contextProvider = serviceProvider.GetRequiredService<IContextProvider<PowerDevice>>();
        }

        public override async Task<IEnumerable<(PowerDevice Payload, ValidationRule Rule)>> Produce(EvaluationContext context, CancellationToken cancel)
        {
            if (context.Rules?.Any() != true)
            {
                throw new InvalidOperationException("rules are not initialized in evaluation contest");
            }
            
            using var scope = appTelemetry.StartOperation(this);
            logger.LogInformation($"total of {context.Rules.Count} validation rules are used");
            
            List<PowerDevice> deviceList;
            if (context.DeviceNames?.Count > 0)
            {
                var devices = await contextProvider.Provide(context, ValidationContextScope.Device, context.DeviceNames, cancel);
                deviceList = devices.ToList();
            }
            else
            {
                var devices = await contextProvider.Provide(context, ValidationContextScope.DC, new List<string> {context.DcName}, cancel);
                deviceList = devices.ToList();
            }
            context.SetDevices(deviceList.ToDictionary(d => d.DeviceName));
            logger.LogInformation($"total of {deviceList.Count} devices retrieved for validation");
            appTelemetry.RecordMetric(
                $"devices", 
                deviceList.Count,
                ("dcName", context.DcName));
            
            var deviceValidationPayloads = new List<(PowerDevice Payload, ValidationRule Rule)>();
            var run = context.Run;
            foreach (var device in deviceList)
            {
                deviceValidationPayloads.AddRange(context.Rules.Select(rule => (device, rule)));
            }

            run.JobId = context.JobId;
            run.ExecutionTime = DateTime.UtcNow;
            run.TotalDevices = deviceList.Count;
            run.TotalRules = context.Rules.Count;
            run.TotalPayloads = deviceValidationPayloads.Count;
            await runRepo.Update(run);

            appTelemetry.RecordMetric(
                "payloads",
                deviceValidationPayloads.Count,
                ("dcName", context.DcName),
                ("runId", context.Run.Id));

            return deviceValidationPayloads;
        }
    }
}