// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LeafDeviceParentHierarchyEvaluator.cs" company="Microsoft Corporation">
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

    public class LeafDeviceParentHierarchyEvaluator : CodeRuleEvaluator
    {
        private readonly ILogger<LeafDeviceParentHierarchyEvaluator> logger;
        private readonly IAppTelemetry appTelemetry;

        public LeafDeviceParentHierarchyEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) :
            base(serviceProvider, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<LeafDeviceParentHierarchyEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
        }

        public override ContextErrorCode CodeRuleErrorCode => ContextErrorCode.LeafDeviceParentInWrongHierarchy;
        public override string RuleName => "parent in GEN/UTS hierarchy";

        protected override void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context)
        {
            var checkName = "parent hierarchy check";
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

            if (currentDevice.DirectDownstreamDeviceList?.Count > 0)
            {
                logger.LogDebug($"device {payload.DeviceName} is not leaf, skip {checkName}");
                return;
            }

            var leafDeviceDetail = DeviceHierarchyDeviceTraversal.ToDetail(currentDevice, context.RelationLookup);
            var allParents = context.DeviceTraversal.FindAllParents(leafDeviceDetail, out _)?.ToList();
            var inCorrectHierarchy =
                allParents?.Any(p => allowedHierarchiesForPowersourceDevices.Contains(p.General.Hierarchy)) == true;
            var visitedHierarchies = allParents?.Any() == true
                ? string.Join(",", allParents.Select(p => p.General.Hierarchy))
                : "";
            if (!inCorrectHierarchy)
            {
                appTelemetry.RecordMetric(
                    "parent-hierarchy-error",
                    1,
                    ("leafDevice", currentDevice.DeviceName),
                    ("dcName", context.DcName));
                logger.LogWarning($"{checkName} failed for: {currentDevice.DeviceName}");
            }

            CodeRuleEvidence evidence = inCorrectHierarchy
                ? new CodeRuleEvidence
                {
                    PropertyPath = "Hierarchy",
                    Actual = visitedHierarchies,
                    Expected = expectedValues,
                    Passed = true,
                    Score = 1,
                    ErrorCode = ContextErrorCode.LeafDeviceParentInWrongHierarchy
                }
                : new CodeRuleEvidence
                {
                    PropertyPath = "Hierarchy",
                    Actual = visitedHierarchies,
                    Expected = expectedValues,
                    Passed = false,
                    Score = 0,
                    ErrorCode = ContextErrorCode.LeafDeviceParentInWrongHierarchy
                };

            if (payload.ContextErrors == null)
                payload.ContextErrors = new List<CodeRuleEvidence>(){evidence};
            else
                payload.ContextErrors.Add(evidence);
        }
    }
}