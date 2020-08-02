// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompletenessEvaluator.cs" company="Microsoft Corporation">
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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;

    public class CompletenessEvaluator : CodeRuleEvaluator
    {
        private readonly ILogger<CompletenessEvaluator> logger;
        private readonly IAppTelemetry appTelemetry;

        public CompletenessEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<CompletenessEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
        }

        public override ContextErrorCode CodeRuleErrorCode => ContextErrorCode.Complete;
        public override string RuleName => "Completeness";

        protected override void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context)
        {
            var checkName = "completeness check";
            logger.LogDebug($"running {checkName} on device {payload.DeviceName}...");
            if (context.EnabledDataCenters.Count > 0 && !context.EnabledDataCenters.Contains(payload.DcName))
            {
                logger.LogDebug($"Skip {checkName} for device {payload.DeviceName}: dc not argus-enabled");
                return;
            }

            var currentDevice = context.DeviceLookup.ContainsKey(payload.DeviceName)
                ? context.DeviceLookup[payload.DeviceName]
                : null;
            if (currentDevice == null)
            {
                logger.LogDebug($"Skip {checkName} for device {payload.DeviceName}: device not found");
                return;
            }

            var dataPoints = context.ZenonDataPointLookup.ContainsKey(payload.DeviceName)
                ? context.ZenonDataPointLookup[payload.DeviceName]
                : null;
            if (dataPoints == null || dataPoints.Count == 0)
            {
                logger.LogDebug($"Skip {checkName} check for device {payload.DeviceName}: no data point");
                return;
            }

            var lastReadings = context.ZenonLastReadingLookup.ContainsKey(payload.DeviceName)
                ? context.ZenonLastReadingLookup[payload.DeviceName]
                : null;

            var dataPointsWithNoReading = new List<string>();
            var dataPointsWithReading = new List<string>();
            if (lastReadings != null && lastReadings.Count > 0)
            {
                foreach (var dataPoint in dataPoints)
                {
                    var reading = lastReadings.FirstOrDefault(r => r.DataPoint.Equals(dataPoint.DataPoint, StringComparison.OrdinalIgnoreCase));
                    if (reading != null)
                    {
                        dataPointsWithReading.Add(dataPoint.DataPoint);
                    }
                    else
                    {
                        dataPointsWithNoReading.Add(dataPoint.DataPoint);
                    }
                }
            }
            else
            {
                dataPointsWithNoReading.AddRange(dataPoints.Select(dp => dp.DataPoint));
            }

            if (dataPointsWithNoReading.Count > 0)
            {
                appTelemetry.RecordMetric(
                    $"${checkName}-error",
                    1,
                    ("deviceName", currentDevice.DeviceName),
                    ("dcName", context.DcName),
                    ("dataPointsWithoutValue", dataPointsWithNoReading.Count.ToString()));
                logger.LogDebug($"{checkName} failed for device {currentDevice.DeviceName}");
            }

            CodeRuleEvidence evidence = dataPointsWithNoReading.Count > 0
                ? new CodeRuleEvidence
                {
                    Actual = $"{string.Join(",", dataPointsWithReading)}",
                    Expected = $"{string.Join(",", dataPoints.Select(dp=>dp.DataPoint))}",
                    Passed = false,
                    Score = (double)dataPointsWithReading.Count / dataPoints.Count,
                    ErrorCode = ContextErrorCode.Complete,
                    PropertyPath = "LastReading"
                }
                : new CodeRuleEvidence
                {
                    Actual =  $"{string.Join(",", dataPointsWithReading)}",
                    Expected = $"{string.Join(",", dataPoints.Select(dp=>dp.DataPoint))}",
                    Passed = true,
                    Score = 1,
                    ErrorCode = ContextErrorCode.Complete,
                    PropertyPath = "LastReading"
                };

            if (payload.ContextErrors == null)
                payload.ContextErrors = new List<CodeRuleEvidence>(){evidence};
            else
                payload.ContextErrors.Add(evidence);
        }
    }
}