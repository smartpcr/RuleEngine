// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StalenessEvaluator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Collections.Generic;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Rules;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Validations.Pipelines;

    public class StalenessEvaluator : CodeRuleEvaluator
    {
        private readonly ILogger<StalenessEvaluator> logger;
        private readonly IAppTelemetry appTelemetry;
        private const int defaultMaxEventTimeWindowInMin = 10;
        private const int eventLatencyInMin = 15;

        public StalenessEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<StalenessEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
        }

        public override ContextErrorCode CodeRuleErrorCode => ContextErrorCode.Stale;
        public override string RuleName => "Staleness";
        protected override void EvaluateDevice(PowerDevice payload, PipelineExecutionContext context)
        {
            var allowedChannelTypes = new List<string> {"Volt", "Amps", "Energy"};
            var checkName = $"staleness check-{string.Join(",", allowedChannelTypes)}";
            logger.LogDebug($"running {checkName} on device {payload.DeviceName}...");
            var currentDevice = context.DeviceLookup.ContainsKey(payload.DeviceName)
                ? context.DeviceLookup[payload.DeviceName]
                : null;
            if (currentDevice == null)
            {
                logger.LogDebug($"Skip {checkName} for device {payload.DeviceName}: device not found");
                return;
            }

            var lastReadings = context.ZenonLastReadingLookup.ContainsKey(payload.DeviceName)
                ? context.ZenonLastReadingLookup[payload.DeviceName]
                : null;
            if (lastReadings == null || lastReadings.Count == 0)
            {
                logger.LogDebug($"Skip {checkName} check for device {payload.DeviceName}: no data");
                return;
            }

            var eventTimeNoEarlierThan = DateTime.UtcNow - TimeSpan.FromMinutes(defaultMaxEventTimeWindowInMin + eventLatencyInMin);
            bool isStale = false;
            DateTime earliestReadingTime = DateTime.UtcNow;
            string dataPoint = null;
            foreach (var reading in lastReadings)
            {
                if (!string.IsNullOrEmpty(reading.ChannelType) &&
                    allowedChannelTypes.Contains(reading.ChannelType) &&
                    reading.PolledTime < earliestReadingTime)
                {
                    earliestReadingTime = reading.PolledTime;
                    dataPoint = reading.DataPoint;
                }
            }
            if (earliestReadingTime < eventTimeNoEarlierThan)
            {
                isStale = true;
                appTelemetry.RecordMetric(
                    $"{checkName}-error",
                    1,
                    ("deviceName", currentDevice.DeviceName),
                    ("dcName", context.DcName),
                    ("dataPoint", dataPoint));
                logger.LogWarning($"{checkName} failed for device {currentDevice.DeviceName}: datapoint={dataPoint}, LastChangeEventTime={earliestReadingTime}, threshold={eventTimeNoEarlierThan}");
            }

            CodeRuleEvidence evidence = isStale
                ? new CodeRuleEvidence
                {
                    Actual = $"{earliestReadingTime}",
                    Expected = $"{eventTimeNoEarlierThan}",
                    Passed = false,
                    Score = 0,
                    ErrorCode = ContextErrorCode.Stale,
                    PropertyPath = $"LastReading['{dataPoint}']"
                }
                : new CodeRuleEvidence
                {
                    Actual = $"{earliestReadingTime}",
                    Expected = $"{eventTimeNoEarlierThan}",
                    Passed = true,
                    Score = 1,
                    ErrorCode = ContextErrorCode.Stale,
                    PropertyPath = $"LastReading['{dataPoint}']"
                };

            if (payload.ContextErrors == null)
                payload.ContextErrors = new List<CodeRuleEvidence>(){evidence};
            else
                payload.ContextErrors.Add(evidence);
        }
    }
}