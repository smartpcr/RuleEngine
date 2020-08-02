// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDevicePayloadBroadcast.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Broadcasts
{
    using System;
    using DataCenterHealth.Models.Devices;
    using Microsoft.Extensions.Logging;
    using Pipelines;

    public class PowerDevicePayloadBroadcast : BasePayloadBroadcast<PowerDevice>
    {
        private readonly ILogger<PowerDevicePayloadBroadcast> logger;

        public PowerDevicePayloadBroadcast(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(serviceProvider, PipelineType.CodeRulePipeline)
        {
            logger = loggerFactory.CreateLogger<PowerDevicePayloadBroadcast>();
            logger.LogInformation($"total of {TotalConsumers} consumers configured for broadcast block");
        }

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }
    }
}