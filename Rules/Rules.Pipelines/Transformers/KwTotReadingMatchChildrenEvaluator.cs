// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KwTotReadingMatchChildrenEvaluator.cs" company="Microsoft Corporation">
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

    public class KwTotReadingMatchChildrenEvaluator : CodeRuleEvaluator
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<KwTotReadingMatchChildrenEvaluator> logger;
        private readonly IAppTelemetry appTelemetry;

        public KwTotReadingMatchChildrenEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) :
            base(serviceProvider, loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<KwTotReadingMatchChildrenEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
        }

        public override ContextErrorCode CodeRuleErrorCode => ContextErrorCode.KwTotReadingNotMatchChildren;
        public override string RuleName => "KwTot matches between parent and child";

        protected override void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context)
        {
            var dataPointName = "Pwr.kW tot";
            const double min = 0.001;
            const double max = 214748364.7;
            const double threshold = 10.0;

            logger.LogDebug($"Checking {dataPointName} for device {payload.DeviceName}");

            var currentDevice = context.DeviceLookup.ContainsKey(payload.DeviceName)
                ? context.DeviceLookup[payload.DeviceName]
                : null;
            if (currentDevice == null)
            {
                logger.LogDebug($"Skip {dataPointName} check for device {payload.DeviceName}: device not found");
                return;
            }

            var zenonEvent = context.ZenonStatsLookup.ContainsKey(payload.DeviceName)
                ? context.ZenonStatsLookup[payload.DeviceName].FirstOrDefault(
                    z => z.DataPoint.Equals(dataPointName, StringComparison.OrdinalIgnoreCase))
                : null;
            if (zenonEvent == null)
            {
                logger.LogDebug($"Skip {dataPointName} check for device {payload.DeviceName}: no data");
                return;
            }

            if (zenonEvent.Avg < min || zenonEvent.Avg > max)
            {
                logger.LogDebug($"Skip {dataPointName} check for device {payload.DeviceName}: value {zenonEvent.Avg} out of range");
                return;
            }

            var childDevices = context.DeviceTraversal.FindChildDevices(
                currentDevice,
                AssociationType.Primary)?.ToList();
            if (!(childDevices?.Count > 0))
            {
                logger.LogDebug($"Skip {dataPointName} check for device {payload.DeviceName}: no children");
                return;
            }

            double parentKwTotal = zenonEvent.Avg;
            double childKwTotal = 0;
            var childDevicesWithValue = new HashSet<string>();
            foreach (var child in childDevices)
            {
                if (context.ZenonStatsLookup.ContainsKey(child.DeviceName))
                {
                    var childZenonStats = context.ZenonStatsLookup[child.DeviceName];
                    var zenonDataPoint = childZenonStats.FirstOrDefault(zdp => zdp.DataPoint.Equals(dataPointName, StringComparison.OrdinalIgnoreCase));
                    if (zenonDataPoint != null && zenonDataPoint.Avg > min && zenonDataPoint.Avg < max)
                    {
                        childKwTotal += zenonDataPoint.Avg;
                        childDevicesWithValue.Add(child.DeviceName);
                    }
                }
            }

            var differencePct = Math.Min(100, Math.Abs(parentKwTotal - childKwTotal) / parentKwTotal * 100);
            if (differencePct > threshold)
            {
                appTelemetry.RecordMetric(
                    "kw-tot-match-children-sum-error",
                    1,
                    ("deviceName", currentDevice.DeviceName),
                    ("dcName", context.DcName));
                logger.LogWarning($"{dataPointName} check failed for: {currentDevice.DeviceName}");
            }

            CodeRuleEvidence evidence = differencePct > threshold
                ? new CodeRuleEvidence
                {
                    Actual = $"parent: {Math.Round(parentKwTotal, 2)}",
                    Expected = $"children: {Math.Round(childKwTotal, 2)}",
                    Passed = false,
                    Score = Math.Abs(Math.Round((100 - differencePct) / 100, 2)),
                    ErrorCode = ContextErrorCode.KwTotReadingNotMatchChildren,
                    PropertyPath = "ReadingStats['Pwr.KW tot']",
                    Remarks = $"child devices: [{string.Join(",", childDevicesWithValue)}]"
                }
                : new CodeRuleEvidence
                {
                    Actual = $"parent: {parentKwTotal}",
                    Expected = $"children: {childKwTotal}",
                    Passed = true,
                    Score = 1,
                    ErrorCode = ContextErrorCode.KwTotReadingNotMatchChildren,
                    PropertyPath = "ReadingStats['Pwr.KW tot']",
                    Remarks = $"child devices: [{string.Join(",", childDevicesWithValue)}]"
                };

            if (payload.ContextErrors == null)
                payload.ContextErrors = new List<CodeRuleEvidence>(){evidence};
            else
                payload.ContextErrors.Add(evidence);
        }
    }
}