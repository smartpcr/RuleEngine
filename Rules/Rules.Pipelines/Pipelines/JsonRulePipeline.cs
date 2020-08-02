// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationPipeline.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Pipelines
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Common.Telemetry;
    using DataCenterHealth.Models.Jobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Producers;

    public class JsonRulePipeline : IPipeline<DeviceValidationPayload, PipelineExecutionContext, DeviceValidationJob>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ILogger<JsonRulePipeline> logger;
        private readonly IPipelineFactory pipelineFactory;
        private readonly IPayloadProducer<DeviceValidationPayload, DeviceValidationJob> producer;
        private readonly PipelineSettings settings;

        public JsonRulePipeline(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<JsonRulePipeline>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            settings = configuration.GetConfiguredSettings<PipelineSettings>();
            pipelineFactory = serviceProvider.GetRequiredService<IPipelineFactory>();
            producer = serviceProvider
                .GetRequiredService<IPayloadProducer<DeviceValidationPayload, DeviceValidationJob>>();
        }

        public string Name => GetType().Name;

        public async Task<PipelineExecutionContext> ExecuteAsync(string runId, DeviceValidationJob job, CancellationToken cancellationToken)
        {
            using var scope = appTelemetry.StartOperation(this);

            var context = new PipelineExecutionContext(job.DcName, runId, job.Id);
            var pipelineBlocks = pipelineFactory.CreateJsonRulePipeline(context, cancellationToken);
            var producerBlock =
                pipelineBlocks.First(b => b.activityType == PipelineActivityType.Producer).block as
                    BufferBlock<DeviceValidationPayload>;
            if (producerBlock == null)
            {
                throw new InvalidOperationException("Unable to find producer block");
            }

            logger.LogInformation($"started running pipeline of {pipelineBlocks.Count} tasks at {DateTime.Now}");
            var watch = Stopwatch.StartNew();

            var payloads = await producer.Produce(context, job, cancellationToken);
            foreach (var payload in payloads)
            {
                var totalRetry = 0;
                var sent = producerBlock.Post(payload);
                if (sent) context.AddTotalSent(1);
                while (!sent && totalRetry < settings.MaxRetryCount)
                {
                    totalRetry++;
                    await Task.Delay(settings.WaitSpan, cancellationToken);
                    sent = producerBlock.Post(payload);
                    if (sent) context.AddTotalSent(1);
                }

                if (!sent) throw new Exception($"Failed to produce payload in pipeline after {totalRetry} retries");

                if (context.TotalSent % settings.MaxBufferCapacity == 0)
                    logger.LogInformation($"total of {context.TotalSent} payload produced");
            }

            logger.LogInformation($"total of {context.TotalSent} payload produced");

            producerBlock.Complete();

            while (pipelineBlocks.FirstOrDefault(b => !b.block.Completion.IsCompleted).block != null)
            {
                var pendingBlock = pipelineBlocks.FirstOrDefault(b => !b.block.Completion.IsCompleted).block;
                if (pendingBlock != null)
                {
                    logger.LogInformation($"queue: {pendingBlock}");
                    await Task.Delay(settings.WaitSpan, cancellationToken);
                }
            }

            await Task.WhenAll(pipelineBlocks.Select(b => b.block.Completion));

            watch.Stop();
            context.Span = watch.Elapsed;
            logger.LogInformation(
                $"Pipeline finished, sent: {context.TotalSent}, received: {context.TotalReceived}, validated: {context.TotalEvaluated}, saved: {context.TotalSaved}, span: {watch.Elapsed}");
            return context;
        }
    }
}