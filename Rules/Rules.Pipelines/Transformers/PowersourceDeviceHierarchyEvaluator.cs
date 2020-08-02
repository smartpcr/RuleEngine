// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowersourceDeviceHierarchyEvaluator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Traversals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;

    public class PowersourceDeviceHierarchyEvaluator : CodeRuleEvaluator
    {
        private readonly ILogger<PowersourceDeviceHierarchyEvaluator> logger;
        private readonly IAppTelemetry appTelemetry;

        public PowersourceDeviceHierarchyEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) :
            base(serviceProvider, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<PowersourceDeviceHierarchyEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
        }

        public override ContextErrorCode CodeRuleErrorCode => ContextErrorCode.PowersourceDeviceInWrongHierarchy;
        public override string RuleName => "power source outbound device in UTS/GEN hierarchy";

        protected override void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context)
        {
            var checkName = "power source parent hierarchy check";
            logger.LogDebug($"Started {checkName} for device {payload.DeviceName}...");

            var allowedHierarchiesForPowersourceDevices = new List<string>
            {
                DeviceHierarchies.UTS_Facility,
                DeviceHierarchies.UTS_Campus,
                DeviceHierarchies.GEN
            };
            var expectedValues = string.Join(",", allowedHierarchiesForPowersourceDevices);

            var currentDevice = context.DeviceLookup.ContainsKey(payload.DeviceName)
                ? context.DeviceLookup[payload.DeviceName]
                : null;
            if (currentDevice == null)
            {
                logger.LogDebug($"Skip {checkName} for device {payload.DeviceName}: device not found");
                return;
            }

            if (currentDevice.DeviceType == DeviceType.Zenon)
            {
                logger.LogDebug($"device {payload.DeviceName} is zenon type, skip {checkName}");
                return;
            }

            var deviceDetail = DeviceHierarchyDeviceTraversal.ToDetail(currentDevice, context.RelationLookup);
            if (deviceDetail.Hierarchy.DirectUpstreamDeviceList?.Any(a => a.AssociationType == AssociationType.PowerSource) != true)
            {
                logger.LogDebug($"device {deviceDetail.General.DeviceName} doesn't have power source parent, skip {checkName}");
                return;
            }

            var inCorrectHierarchy = allowedHierarchiesForPowersourceDevices.Contains(deviceDetail.General.Hierarchy);
            if (!inCorrectHierarchy)
            {
                appTelemetry.RecordMetric(
                    "powersource-hierarchy-error",
                    1,
                    ("deviceName", deviceDetail.General.DeviceName),
                    ("dcName", context.DcName));
                logger.LogWarning($"{checkName} failed for {deviceDetail.General.DeviceName}");
            }

            var evidence = inCorrectHierarchy
                ? new CodeRuleEvidence
                {
                    Actual = deviceDetail.General.Hierarchy,
                    Expected = expectedValues,
                    Passed = true,
                    Score = 1,
                    ErrorCode = ContextErrorCode.PowersourceDeviceInWrongHierarchy,
                    PropertyPath = "Hierarchy"
                }
                : new CodeRuleEvidence
                {
                    Actual = deviceDetail.General.Hierarchy,
                    Expected = expectedValues,
                    Passed = false,
                    Score = 0,
                    ErrorCode = ContextErrorCode.PowersourceDeviceInWrongHierarchy,
                    PropertyPath = "Hierarchy"
                };

            if (payload.ContextErrors == null)
                payload.ContextErrors = new List<CodeRuleEvidence>(){evidence};
            else
                payload.ContextErrors.Add(evidence);
        }
    }
}