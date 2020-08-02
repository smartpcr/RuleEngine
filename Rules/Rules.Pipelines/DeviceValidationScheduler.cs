// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationScheduler.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Config;
    using Common.Storage;
    using Common.Telemetry;
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NCrontab;

    public class DeviceValidationScheduler : BackgroundService
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly IDocDbRepository<DataCenter> dataCenterRepo;
        private readonly IDocDbRepository<DeviceValidationJob> jobRepo;
        private readonly ILogger<DeviceValidationScheduler> logger;
        private readonly IQueueClient<DeviceValidationJob> queueClient;
        private readonly IDocDbRepository<RuleSet> ruleSetRepo;
        private readonly IDocDbRepository<DeviceValidationSchedule> scheduleRepo;
        private readonly SchedulerSettings settings;
        private List<string> allDcNames;
        private DateTime lastCheckTime = DateTime.UtcNow.AddDays(-1);

        public DeviceValidationScheduler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DeviceValidationScheduler>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            scheduleRepo = repoFactory.CreateRepository<DeviceValidationSchedule>();
            jobRepo = repoFactory.CreateRepository<DeviceValidationJob>();
            ruleSetRepo = repoFactory.CreateRepository<RuleSet>();
            dataCenterRepo = repoFactory.CreateRepository<DataCenter>();
            queueClient = serviceProvider.GetRequiredService<IQueueClient<DeviceValidationJob>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<SchedulerSettings>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var allDataCenters = await dataCenterRepo.GetAll();
            allDcNames = allDataCenters.Select(dc => dc.DcName).ToList();
            var allRuleSets = await ruleSetRepo.GetAll();
            var ruleSetList = allRuleSets.ToList();

            while (!stoppingToken.IsCancellationRequested)
            {
                var checkSchedule = CrontabSchedule.Parse(settings.CheckFrequency);
                var nextCheckTime = checkSchedule.GetNextOccurrence(lastCheckTime);
                logger.LogInformation(
                    $"last check time: {lastCheckTime}, calculated next check time: {nextCheckTime}, current time: {DateTime.Now}");

                try
                {
                    if (nextCheckTime < DateTime.Now)
                    {
                        appTelemetry.RecordMetric($"{GetType().Name}-check", 1);

                        var allSchedules = await scheduleRepo.ObtainLease(stoppingToken);
                        var enabledSchedules = allSchedules.Where(s => s.Enabled).ToList();
                        logger.LogInformation($"total {enabledSchedules.Count} schedules found");
                        appTelemetry.RecordMetric($"{GetType().Name}-totalSchedules", enabledSchedules.Count);

                        foreach (var schedule in enabledSchedules)
                        {
                            try
                            {
                                var shouldScheduleJob = false;
                                if (schedule.LastRunTime == null)
                                {
                                    shouldScheduleJob = true;
                                }
                                else
                                {
                                    var jobSchedule = CrontabSchedule.Parse(schedule.Frequency);
                                    var nextRunTime = jobSchedule.GetNextOccurrence(schedule.LastRunTime.Value);
                                    logger.LogInformation(
                                        $"schedule {schedule.Name}, last run time: {schedule.LastRunTime.Value}, calculated next run time: {nextRunTime}");
                                    if (nextRunTime < DateTime.Now && settings.CompensateMissedSchedules)
                                        shouldScheduleJob = true;
                                }

                                logger.LogInformation(
                                    $"schedule '{schedule.Name}', last run time: {schedule.LastRunTime}, schedule jobs: {shouldScheduleJob}");
                                if (shouldScheduleJob)
                                {
                                    appTelemetry.RecordMetric(
                                        $"{GetType().Name}-createSchedule",
                                        1,
                                        ("name", schedule.Name));

                                    var scheduledDcNames =
                                        schedule.DcNames.Any() && schedule.DcNames.All(dc => dc.ToLower() != "all")
                                            ? schedule.DcNames
                                            : allDcNames;
                                    var scheduledRuleSets = ruleSetList.Where(rs => schedule.RuleSetNames.Contains(rs.Name))
                                        .Select(rs => rs.Id).ToList();
                                    if (scheduledDcNames.Count > 0 && scheduledRuleSets.Count > 0)
                                    {
                                        var jobs = (
                                            from dcName in scheduledDcNames
                                            from ruleSetId in scheduledRuleSets
                                            select new DeviceValidationJob
                                            {
                                                ScheduleName = schedule.Name,
                                                DcName = dcName,
                                                RuleSetId = ruleSetId,
                                                SubmissionTime = DateTime.Now,
                                                SubmittedBy = GetType().Name
                                            }).ToList();
                                        logger.LogInformation(
                                            $"total of {jobs.Count} jobs will be created for schedule: {schedule.Name}");
                                        appTelemetry.RecordMetric(
                                            $"{GetType().Name}-createJobs",
                                            jobs.Count,
                                            ("name", schedule.Name));

                                        await QueueJobs(jobs, stoppingToken);
                                        schedule.LastRunTime = DateTime.Now;
                                        await scheduleRepo.Update(schedule);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, $"Failed creating schedule, name: {schedule.Name}");
                                appTelemetry.RecordMetric(
                                    $"{GetType().Name}-createScheduleError",
                                    1,
                                    ("name", schedule.Name));
                            }
                        }

                        foreach (var schedule in allSchedules)
                        {
                            await scheduleRepo.ReleaseLease(schedule.Id, stoppingToken);
                        }

                        lastCheckTime = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    appTelemetry.RecordMetric($"{GetType().Name}-error", 1);
                    logger.LogError(ex, $"Failed to schedule job, {ex.Message}");
                }

                await Task.Delay(settings.SleepInterval, stoppingToken);
            }
        }

        private async Task QueueJobs(List<DeviceValidationJob> jobs, CancellationToken cancel)
        {
            foreach (var job in jobs)
            {
                var createdJob = await jobRepo.Create(job);
                var receipt = await queueClient.Enqueue(createdJob, cancel);
                logger.LogInformation($"validation job '{createdJob.Id}' is queued with receipt: {receipt.MessageId}");
                appTelemetry.RecordMetric(
                    $"{GetType().Name}-queuejob",
                    1,
                    ("dcName", job.DcName),
                    ("ruleSetId", job.RuleSetId));
            }
        }
    }
}