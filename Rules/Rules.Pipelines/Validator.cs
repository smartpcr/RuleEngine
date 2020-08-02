namespace Rules.Validations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Pipelines;

    public class Validator : IValidator
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly IPipeline<PowerDevice, PipelineExecutionContext, DeviceValidationJob> codeRulePipeline;
        private readonly IPipeline<DeviceValidationPayload, PipelineExecutionContext, DeviceValidationJob> jsonRulePipeline;
        private readonly IPipeline<DataCenterValidationPayload, PipelineExecutionContext, DataCenterValidationJob> dcPipeline;

        private readonly ILogger<Validator> logger;
        private readonly IDocDbRepository<ValidationRule> ruleRepo;
        private readonly IDocDbRepository<RuleSet> ruleSetRepo;
        private readonly IDocDbRepository<DeviceValidationRun> runRepo;
        private readonly IDocDbRepository<DataCenterValidationRun> dcRunRepo;

        public Validator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<Validator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();

            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            ruleSetRepo = repoFactory.CreateRepository<RuleSet>();
            ruleRepo = repoFactory.CreateRepository<ValidationRule>();
            runRepo = repoFactory.CreateRepository<DeviceValidationRun>();
            dcRunRepo = repoFactory.CreateRepository<DataCenterValidationRun>();

            jsonRulePipeline = serviceProvider.GetRequiredService<
                IPipeline<DeviceValidationPayload, PipelineExecutionContext, DeviceValidationJob>>();
            codeRulePipeline = serviceProvider.GetRequiredService<
                IPipeline<PowerDevice, PipelineExecutionContext, DeviceValidationJob>>();
            dcPipeline = serviceProvider.GetRequiredService<
                IPipeline<DataCenterValidationPayload, PipelineExecutionContext, DataCenterValidationJob>>();
        }

        public async Task<DeviceValidationRun> ValidateDevices(
            DeviceValidationJob job,
            DeviceValidationRun run,
            CancellationToken cancel)
        {
            try
            {
                PipelineExecutionContext context;
                RuleSet ruleSet;
                if (!string.IsNullOrEmpty(job.RuleId))
                {
                    var rule = await ruleRepo.GetById(job.RuleId);
                    ruleSet = await ruleSetRepo.GetById(rule.RuleSetId);
                }
                else
                {
                    ruleSet = await ruleSetRepo.GetById(job.RuleSetId);
                }

                if (ruleSet.Type == RuleType.CodeRule)
                    context = await codeRulePipeline.ExecuteAsync(run.Id, job, cancel);
                else
                    context = await jsonRulePipeline.ExecuteAsync(run.Id, job, cancel);

                // payload, execution time is updated in producer, retrieve it again
                run = await runRepo.GetById(run.Id);
                run.FinishTime = DateTime.UtcNow;
                run.TotalPayloads = context.TotalReceived;
                run.TotalEvaluated = context.TotalEvaluated;
                run.TotalResults = context.TotalSaved;
                run.TimeSpan = context.Span.ToString();
                if (context.Scores.Any()) run.AverageScore = context.Scores.Average(s => s.score);

                run.Succeed = true;
                logger.LogInformation($"saving pipeline summary to job: avg score: {run.AverageScore}");
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-received",
                    context.TotalReceived,
                    ("dcName", job.DcName),
                    ("ruleSetId", job.RuleSetId));
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-evaluated",
                    context.TotalEvaluated,
                    ("dcName", job.DcName),
                    ("ruleSetId", job.RuleSetId));
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-saved",
                    context.TotalSaved,
                    ("dcName", job.DcName),
                    ("ruleSetId", job.RuleSetId));
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "failed to run validation job");
                run.Succeed = false;
                run.Error = ex.Message;
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-timeout",
                    1,
                    ("dcName", job.DcName),
                    ("ruleSetId", job.RuleSetId));

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
                    $"{nameof(DeviceValidationWorker)}-error",
                    1,
                    ("dcName", job.DcName),
                    ("ruleSetId", job.RuleSetId));
            }

            await runRepo.Update(run);

            return run;
        }

        public async Task<DataCenterValidationRun> ValidateDataCenter(
            DataCenterValidationJob job,
            DataCenterValidationRun run,
            CancellationToken cancel)
        {
            try
            {
                run.ExecutionTime = DateTime.UtcNow;
                var context = await dcPipeline.ExecuteAsync(run.Id, job, cancel);
                run = await dcRunRepo.GetById(run.Id);
                run.FinishTime = DateTime.UtcNow;
                run.TimeSpan = context.Span.ToString();
                if (context.Scores.Any()) run.AverageScore = context.Scores.Average(s => s.score);

                run.Succeed = true;
                logger.LogInformation($"saving pipeline summary to job: avg score: {run.AverageScore}");
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-received",
                    context.TotalReceived,
                    ("dcName", job.DcName));
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-evaluated",
                    context.TotalEvaluated,
                    ("dcName", job.DcName));
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-saved",
                    context.TotalSaved,
                    ("dcName", job.DcName));
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "failed to run validation job");
                run.Succeed = false;
                run.Error = ex.Message;
                appTelemetry.RecordMetric(
                    $"{nameof(DeviceValidationWorker)}-timeout",
                    1,
                    ("dcName", job.DcName));

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
                    $"{nameof(DeviceValidationWorker)}-error",
                    1,
                    ("dcName", job.DcName));
            }
            finally
            {
                await dcRunRepo.Update(run);
            }

            return run;
        }
    }
}