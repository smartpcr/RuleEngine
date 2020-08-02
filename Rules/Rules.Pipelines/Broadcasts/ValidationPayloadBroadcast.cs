// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceValidationPayloadBroadcast.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Broadcasts
{
    using System;
    using Microsoft.Extensions.Logging;
    using Models;
    using Pipelines;

    public class ValidationPayloadBroadcast : BasePayloadBroadcast<DeviceValidationPayload>
    {
        private readonly ILogger<ValidationPayloadBroadcast> logger;

        public ValidationPayloadBroadcast(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider, PipelineType.JsonRulePipeline)
        {
            logger = loggerFactory.CreateLogger<ValidationPayloadBroadcast>();
            logger.LogInformation($"total of {TotalConsumers} consumers configured for broadcast block");
        }

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }
    }
}