// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceSumOfCurrentEvaluator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Linq;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using Microsoft.Extensions.Logging;
    using Pipelines;

    public class PowerDeviceSumOfCurrentEvaluator
        : BasePayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>
    {
        private readonly ILogger<PowerDeviceSumOfCurrentEvaluator> logger;

        public PowerDeviceSumOfCurrentEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider)
        {
            logger = loggerFactory.CreateLogger<PowerDeviceSumOfCurrentEvaluator>();
        }

        protected override DeviceValidationResult Transform(PowerDevice payload, PipelineExecutionContext context)
        {
            var result = new DeviceValidationResult
            {
                DeviceName = payload.DeviceName,
                ExecutionTime = DateTime.UtcNow,
                Score = 0,
                RunId = context.RunId,
                JobId = context.JobId,
                ValidationRuleId = GetType().Name
            };

            try
            {
                if (payload.Children.Any() &&
                    payload.Amperage.HasValue &&
                    payload.Children.Any(c => c.Amperage.HasValue))
                {
                    context.AddTotalFiltered(1);
                    var sumOfCurrentForChildren = payload.Children.Where(cd => cd.Amperage.HasValue)
                        .Select(cd => cd.Amperage.Value).Sum();
                    if ((double) sumOfCurrentForChildren < 0.9 * (double) payload.Amperage.Value ||
                        (double) sumOfCurrentForChildren > 1.1 * (double) payload.Amperage.Value)
                        result.Assert = false;
                    else
                        result.Assert = true;
                    result.Score = result.Assert == true ? 1.0M : -1.0M;

                    context.AddTotalEvaluated(1);
                    context.Scores.Add((payload.DeviceName, GetType().Name, result.Score.Value));
                    if (context.TotalEvaluated % 100 == 0)
                        logger.LogInformation($"total validated: {context.TotalEvaluated} by {GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed on rule {GetType().Name} and device {payload.DeviceName}, {ex.Message}");
                result.Error = ex.Message;
                context.AddTotalFailed(1);
            }

            return result;
        }

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }
    }
}