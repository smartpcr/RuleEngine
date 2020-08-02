// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceProducer.cs" company="Microsoft Corporation">
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
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;

    public class PowerDeviceProducer : BasePayloadProducer<PowerDevice, DeviceValidationJob>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly IContextProvider<PowerDevice> contextProvider;
        private readonly ILogger<PowerDeviceProducer> logger;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;

        public PowerDeviceProducer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider)
        {
            logger = loggerFactory.CreateLogger<PowerDeviceProducer>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            contextProvider = serviceProvider.GetRequiredService<IContextProvider<PowerDevice>>();
        }

        public override string Name => GetType().Name;

        public override async Task<IList<PowerDevice>> Produce(
            PipelineExecutionContext context,
            DeviceValidationJob validationJob,
            CancellationToken cancellationToken)
        {
            using var scope = appTelemetry.StartOperation(this, validationJob.ActivityId);

            List<PowerDevice> deviceList;
            if (validationJob.DeviceNames?.Count > 0)
            {
                var devices = await contextProvider.Provide(context, ContextProviderScope.Device, validationJob.DeviceNames, cancellationToken);
                deviceList = devices.ToList();
            }
            else
            {
                var devices = await contextProvider.Provide(context, ContextProviderScope.DC, new List<string>{ validationJob.DcName}, cancellationToken);
                deviceList = devices.ToList();
            }
            context.DeviceLookup = deviceList.ToDictionary(d => d.DeviceName);

            logger.LogInformation(
                $"Total of {deviceList.Count} power devices found for dc: {validationJob.DcName}");
            appTelemetry.RecordMetric(
                $"{GetType().Name}-devices",
                deviceList.Count,
                ("dcName", validationJob.DcName));

            var run = await runRepo.GetById(context.RunId);
            run.JobId = validationJob.Id;
            run.ExecutionTime = DateTime.UtcNow;
            run.TotalDevices = deviceList.Count;
            run.TotalPayloads = deviceList.Count;
            await runRepo.Update(run);

            appTelemetry.RecordMetric(
                $"{GetType().Name}-payloads",
                deviceList.Count,
                ("dcName", validationJob.DcName),
                ("ruleSetId", validationJob.RuleSetId));

            return deviceList;
        }
    }
}