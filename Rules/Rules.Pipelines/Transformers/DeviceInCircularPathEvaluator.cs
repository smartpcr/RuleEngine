// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceInCircularPathEvaluator.cs" company="Microsoft Corporation">
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

    public class DeviceInCircularPathEvaluator : CodeRuleEvaluator
    {
        private readonly ILogger<DeviceInCircularPathEvaluator> logger;
        private readonly IAppTelemetry appTelemetry;

        public DeviceInCircularPathEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DeviceInCircularPathEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
        }

        public override ContextErrorCode CodeRuleErrorCode => ContextErrorCode.DeviceInCircularPath;
        public override string RuleName => "circular path in hierarchy";

        protected override void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context)
        {
            logger.LogDebug("Checking circular path from leaf device to root device");
            var leaf = context.DeviceLookup.ContainsKey(payload.DeviceName)
                ? context.DeviceLookup[payload.DeviceName]
                : null;
            if (leaf == null || leaf.DirectDownstreamDeviceList?.Count > 0)
            {
                logger.LogDebug($"device {payload.DeviceName} is not leaf, skip circular path check");
                return;
            }

            var leafDeviceDetail = DeviceHierarchyDeviceTraversal.ToDetail(leaf, context.RelationLookup);
            var allParents = context.DeviceTraversal.FindAllParents(leafDeviceDetail, out var devicesInLoop)?.ToList() ?? new List<PowerDeviceDetail>();
            var allParentDeviceNames = allParents.Select(p => p.General.DeviceName).ToList();
            var haveCircularPath = devicesInLoop.Count > 0;

            var evidence = haveCircularPath
                ? new CodeRuleEvidence
                {
                    PropertyPath = "Parent",
                    Actual = $"device in loop: {string.Join(",", devicesInLoop)}",
                    Expected = $"all devices: {string.Join(",", allParentDeviceNames)}",
                    Passed = false,
                    Score = 0,
                    ErrorCode = ContextErrorCode.DeviceInCircularPath
                }
                : new CodeRuleEvidence
                {
                    PropertyPath = "Parent",
                    Actual = "device in loop: (empty)",
                    Expected = $"all devices: {string.Join(",", allParentDeviceNames)}",
                    Passed = true,
                    Score = 1,
                    ErrorCode = ContextErrorCode.DeviceInCircularPath
                };

            if (payload.ContextErrors == null)
                payload.ContextErrors = new List<CodeRuleEvidence>(){evidence};
            else
                payload.ContextErrors.Add(evidence);

        }
    }
}