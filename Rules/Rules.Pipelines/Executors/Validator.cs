// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Validator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Validation;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Rules.Pipelines.Builders;

    public class Validator : IValidator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILoggerFactory loggerFactory;
        private readonly IAppTelemetry appTelemetry;
        private readonly ILogger<Validator> logger;
        private readonly PipelineFactory pipelineFactory;
        private readonly IDocDbRepository<ValidationRule> ruleRepo;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;
        private readonly PipelineSettings settings;

        public Validator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<Validator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            pipelineFactory = serviceProvider.GetRequiredService<PipelineFactory>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            ruleRepo = repoFactory.CreateRepository<ValidationRule>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }

        public async Task<DeviceValidationRun> Validate(DeviceValidationJob job, DeviceValidationRun run, CancellationToken cancel)
        {
            Func<RuleContext, List<ValidationRule>, EvaluationContext> createEvaluationContext = (ruleContext, ruleList) =>
            {
                ValidationContextScope scope = ValidationContextScope.Device;
                switch (ruleContext)
                {
                    case RuleContext.Channel:
                        scope = ValidationContextScope.Channel;
                        break;
                    case RuleContext.Global:
                        scope = ValidationContextScope.DC;
                        break;
                }

                var context = new EvaluationContext(
                    job.DcName,
                    job.DeviceNames,
                    run,
                    job.Id,
                    scope,
                    ruleList,
                    loggerFactory,
                    appTelemetry);
                return context;
            };

            var watch = Stopwatch.StartNew();
            try
            {
                var rules = await GetRules(job);
                var rulesByContext = rules.GroupBy(r => r.Context).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var ruleContext in rulesByContext.Keys)
                {
                    var ruleList = rulesByContext[ruleContext];
                    if (ruleList.Count == 0)
                    {
                        continue;
                    }

                    switch (ruleContext)
                    {
                        case RuleContext.Device:
                            var context = createEvaluationContext(ruleContext, ruleList);
                            await ValidateDevices(context, cancel);
                            break;
                        default:
                            throw new NotSupportedException($"validation against context scope '{ruleContext}' is not supported");
                    }
                }

                return run;
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "failed to run validation job");
                run.Succeed = false;
                run.Error = ex.Message;
                appTelemetry.RecordMetric(
                    $"{nameof(Validator)}-timeout",
                    1,
                    ("dcName", job.DcName),
                    ("ruleIds", string.Join(",", job.RuleIds)),
                    ("ruleSetIds", string.Join(",", job.RuleSetIds)));

                if (cancel.IsCancellationRequested)
                {
                    logger.LogError("Operation timed out.");
                }
                else if (cancel.IsCancellationRequested)
                {
                    logger.LogError("Cancelling per user request.");
                    cancel.ThrowIfCancellationRequested();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "failed to run validation job");
                run.Succeed = false;
                run.Error = ex.Message;

                appTelemetry.RecordMetric(
                    $"{nameof(Validator)}-error",
                    1,
                    ("dcName", job.DcName),
                    ("ruleIds", string.Join(",", job.RuleIds)),
                    ("ruleSetIds", string.Join(",", job.RuleSetIds)));
            }

            watch.Stop();
            run.TimeSpan = watch.Elapsed.ToString();
            await runRepo.Update(run);

            return run;
        }

        private async Task ValidateDevices(EvaluationContext context, CancellationToken cancel)
        {
            var deviceValidationPipeline = pipelineFactory.CreatePipeline<PowerDevice>(serviceProvider);
            var pipelineBlocks = deviceValidationPipeline.CreatePipeline(context, cancel);
            var watch = Stopwatch.StartNew();

            var payloads = await deviceValidationPipeline.Producer.Produce(context, cancel);
            foreach (var payload in payloads)
            {
                var totalRetry = 0;
                var sent = pipelineBlocks.ProducerBlock.Post(payload);
                if (sent) context.AddTotalSent(1);
                while (!sent && totalRetry < settings.MaxRetryCount)
                {
                    totalRetry++;
                    await Task.Delay(settings.WaitSpan, cancel);
                    sent = pipelineBlocks.ProducerBlock.Post(payload);
                    if (sent) context.AddTotalSent(1);
                }

                if (!sent) throw new Exception($"Failed to produce payload in pipeline after {totalRetry} retries");

                if (context.TotalSent % settings.MaxBufferCapacity == 0)
                    logger.LogInformation($"total of {context.TotalSent} payload produced");
            }

            logger.LogInformation($"total of {context.TotalSent} payload produced within {watch.Elapsed}");
            pipelineBlocks.ProducerBlock.Complete();

            while (deviceValidationPipeline.AllBlocks.FirstOrDefault(b => !b.Completion.IsCompleted) != null)
            {
                var pendingBlock = deviceValidationPipeline.AllBlocks.FirstOrDefault(b => !b.Completion.IsCompleted);
                if (pendingBlock != null)
                {
                    logger.LogInformation($"queue: {pendingBlock}");
                    await Task.Delay(settings.WaitSpan, cancel);
                }
            }

            await Task.WhenAll(deviceValidationPipeline.AllBlocks.Select(b => b.Completion));

            watch.Stop();
            context.Span = watch.Elapsed;
            logger.LogInformation(
                $"Pipeline finished, sent: {context.TotalSent}, received: {context.TotalReceived}, validated: {context.TotalEvaluated}, saved: {context.TotalSaved}, span: {watch.Elapsed}");

            var run = context.Run;
            run.FinishTime = DateTime.UtcNow;
            run.TotalPayloads += (run.TotalPayloads ?? 0) + context.TotalReceived;
            run.TotalEvaluated += (run.TotalEvaluated ?? 0) + context.TotalEvaluated;
            run.TotalResults += (run.TotalResults ?? 0) + context.TotalSaved;
            if (context.Scores.Any()) run.AverageScore = (decimal) context.Scores.Average(s => s.score);

            run.Succeed = true;
            logger.LogInformation($"saving pipeline summary to job: avg score: {run.AverageScore}");
            appTelemetry.RecordMetric(
                $"{GetType().Name}-received",
                context.TotalReceived,
                ("dcName", context.DcName));
            appTelemetry.RecordMetric(
                $"{GetType().Name}-evaluated",
                context.TotalEvaluated,
                ("dcName", context.DcName));
            appTelemetry.RecordMetric(
                $"{GetType().Name}-saved",
                context.TotalSaved,
                ("dcName", context.DcName));
        }

        private async Task<IEnumerable<ValidationRule>> GetRules(DeviceValidationJob job)
        {
            var validationRules = new List<ValidationRule>();
            if (job.RuleIds?.Any() == true)
            {
                var rules = await ruleRepo.Query("c.id in ({0})", job.RuleIds);
                validationRules.AddRange(rules);
            }
            else if (job.RuleSetIds?.Any() == true)
            {
                var rules = await ruleRepo.Query("c.ruleSetId in ({0})", job.RuleSetIds);
                validationRules.AddRange(rules);
            }

            if (validationRules.Count == 0)
            {
                throw new InvalidOperationException("Unable to get any rules for current validation job");
            }

            return validationRules;
        }
    }
}