// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceEvaluator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Cache;
    using Common.Telemetry;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Pipelines;
    using Rules.Expressions;
    using Rules.Expressions.Eval;
    using Rules.Expressions.Evaluators;
    using Rules.Expressions.Parsers;

    public class PowerDeviceEvaluator
        : BasePayloadTransformer<DeviceValidationPayload, DeviceValidationResult, DeviceValidationJob>
    {
        private readonly IAppTelemetry appTelemetry;
        private readonly ICacheProvider cache;
        private readonly JsonSerializerSettings errorSerializerSettings;
        private readonly ILogger<PowerDeviceEvaluator> logger;
        private readonly object syncObj = new object();

        private readonly IDocDbRepository<ValidationRule> validationRuleRepo;

        private ConcurrentDictionary<
            string,
            (Func<PowerDevice, bool> filter,
            Func<PowerDevice, bool> assert,
            Dictionary<string, Func<PowerDevice, string>> evidenceCollector,
            Dictionary<string, Func<PowerDevice, string>> expectationCollector,
            Dictionary<string, Func<PowerDevice, double>> getScore)> filterAndEvaluationRules;

        public PowerDeviceEvaluator(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(
            serviceProvider)
        {
            logger = loggerFactory.CreateLogger<PowerDeviceEvaluator>();
            appTelemetry = serviceProvider.GetRequiredService<IAppTelemetry>();
            var repoFactory = serviceProvider.GetRequiredService<RepositoryFactory>();
            validationRuleRepo = repoFactory.CreateRepository<ValidationRule>();
            cache = serviceProvider.GetRequiredService<ICacheProvider>();
            errorSerializerSettings = new JsonSerializerSettings
            {
                MaxDepth = 3,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
            errorSerializerSettings.Converters.Add(new StringEnumConverter());

            EnsureRules();
        }

        protected override DeviceValidationResult Transform(DeviceValidationPayload input, PipelineExecutionContext context)
        {
            var result = new DeviceValidationResult
            {
                DeviceName = input.Device.DeviceName,
                ValidationRuleId = input.Rule.Id,
                ExecutionTime = DateTime.UtcNow,
                RunId = context.RunId,
                JobId = context.JobId,
                Score = 0
            };
            if (filterAndEvaluationRules.TryGetValue(input.Rule.Id, out var rule))
            {
                try
                {
                    var filterResult = rule.filter(input.Device);
                    if (filterResult)
                    {
                        context.AddTotalFiltered(1);
                        result.Assert = rule.assert(input.Device);
                        var score = 1.0M;

                        if (result.Assert == false)
                        {
                            try
                            {
                                if (rule.evidenceCollector != null)
                                {
                                    var evidences = new List<DeviceValidationEvidence>();
                                    foreach (var field in rule.evidenceCollector.Keys)
                                    {
                                        var getEvidence = rule.evidenceCollector[field];
                                        var getExpectation = rule.expectationCollector[field];
                                        var getScore = rule.getScore?.ContainsKey(field) == true
                                            ? rule.getScore[field]
                                            : null;
                                        var actual = getEvidence(input.Device);
                                        var expected= getExpectation(input.Device);
                                        var fieldScore = getScore?.Invoke(input.Device) ?? 0.0;
                                        evidences.Add(new DeviceValidationEvidence
                                        {
                                            PropertyPath = field,
                                            Expected = expected,
                                            Actual = actual,
                                            Score = fieldScore
                                        });
                                    }

                                    result.Error = JsonConvert.SerializeObject(evidences, errorSerializerSettings);
                                    score = (decimal) evidences.Average(f => f.Score);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to serizlize device context");
                            }
                        }

                        result.Score = score;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        $"Failed on rule {input.Rule.Name} and device {input.Device.DeviceName}, {ex.Message}");
                    result.Error = ex.Message;
                    context.AddTotalFailed(1);
                }
            }
            else
            {
                logger.LogError($"Unable to find rule with name: '{input.Rule.Name}'");
                appTelemetry.RecordMetric(
                    $"{GetType().Name}-missingrule",
                    1,
                    ("ruleId", input.Rule.Id),
                    ("deviceName", input.Device.DeviceName));
                result.Error = "no rules found";
            }

            if (result.Assert.HasValue)
            {
                context.AddTotalEvaluated(1);
                context.Scores.Add((input.Device.DeviceName, input.Rule.Id, result.Score.Value));
                if (context.TotalEvaluated % 100 == 0)
                    logger.LogInformation($"total validated: {context.TotalEvaluated} by {GetType().Name}");
            }

            return result;
        }

        protected override void LogInformation(string message)
        {
            logger.LogInformation(message);
        }

        private void EnsureRules()
        {
            using var scope = appTelemetry.StartOperation(this);
            var cancel = new CancellationToken();

            if (filterAndEvaluationRules == null)
            {
                lock (syncObj)
                {
                    if (filterAndEvaluationRules == null)
                    {
                        logger.LogInformation("populating validation rules...");
                        var parser = new ExpressionParser();
                        IExpressionBuilder builder = new ExpressionBuilder();
                        filterAndEvaluationRules = new ConcurrentDictionary<
                            string, (
                            Func<PowerDevice, bool> filter,
                            Func<PowerDevice, bool> assert,
                            Dictionary<string, Func<PowerDevice, string>> evidenceCollector,
                            Dictionary<string, Func<PowerDevice, string>> expectationCollector,
                            Dictionary<string, Func<PowerDevice, double>> getScore)>();

                        var jsonRules = cache.GetOrUpdateAsync(
                            $"list-{nameof(ValidationRule)}-jsonrule",
                            async () => await validationRuleRepo.GetLastModificationTime(null, cancel),
                            async () =>
                            {
                                var validationRules = await validationRuleRepo.Query($"c.type = '{RuleType.JsonRule.ToString()}'");
                                return validationRules.ToList();
                            },
                            cancel).GetAwaiter().GetResult();
                        logger.LogInformation($"total of {jsonRules.Count} rules for rule set");

                        logger.LogInformation("compiling rule expressions...");
                        foreach (var rule in jsonRules)
                        {
                            Func<PowerDevice, bool> filter = d => true;
                            Func<PowerDevice, bool> assert;
                            Dictionary<string, Func<PowerDevice, string>> evidenceCollectors;
                            Dictionary<string, Func<PowerDevice, string>> expectationCollectors;
                            Dictionary<string, Func<PowerDevice, double>> getScores;
                            try
                            {
                                var expression = JObject.Parse(rule.WhenExpression);
                                var conditions = parser.Parse(expression);
                                filter = builder.Build<PowerDevice>(conditions);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(
                                    $"Invalid [WHEN] expression for rule: {rule.Id}, expression: \n{rule.WhenExpression}\nerror: {ex.Message}");
                            }

                            try
                            {
                                var expression = JObject.Parse(rule.IfExpression);
                                var conditions = parser.Parse(expression);
                                assert = builder.Build<PowerDevice>(conditions);
                                evidenceCollectors = GetRuleEvidenceCollector(rule);
                                expectationCollectors = GetRuleExpectationCollector(rule);
                                getScores = GetRuleScores(rule);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(
                                    $"Invalid [IF] expression for rule: {rule.Id}, expression: \n{rule.IfExpression}\nerror: {ex.Message}");
                                continue;
                            }

                            filterAndEvaluationRules.AddOrUpdate(
                                rule.Id,
                                (filter, assert, evidenceCollectors, expectationCollectors, getScores),
                                (k, v) => (filter, assert, evidenceCollectors, expectationCollectors, getScores));
                        }

                        logger.LogInformation(
                            $"compilation finished, total of {filterAndEvaluationRules.Count} rules generated");
                    }
                }
            }
        }

        private Dictionary<string, Func<PowerDevice, string>> GetRuleEvidenceCollector(ValidationRule rule)
        {
            var collectors = new Dictionary<string, Func<PowerDevice, string>>();
            var assertCondition = rule.IfExpression;
            if (!string.IsNullOrEmpty(assertCondition))
                try
                {
                    var token = JToken.Parse(assertCondition);
                    var parser = new ExpressionParser();
                    var filterExpressionTree = parser.Parse(token);
                    var leafEvaluators = new List<LeafExpression>();
                    PopulateLeafFieldEvaluators(filterExpressionTree, leafEvaluators);
                    foreach (var leafEval in leafEvaluators)
                    {
                        if (!collectors.ContainsKey(leafEval.Left))
                        {
                            var evidenceCollector = leafEval.GetEvidence<PowerDevice>();
                            collectors.Add(leafEval.Left, evidenceCollector);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        $"Failed to parse assert condition, id={rule.Id}, name={rule.Name}, expression={rule.IfExpression}");
                }

            return collectors;
        }

        private Dictionary<string, Func<PowerDevice, string>> GetRuleExpectationCollector(ValidationRule rule)
        {
            var collectors = new Dictionary<string, Func<PowerDevice, string>>();
            var assertCondition = rule.IfExpression;
            if (!string.IsNullOrEmpty(assertCondition))
                try
                {
                    var token = JToken.Parse(assertCondition);
                    var parser = new ExpressionParser();
                    var filterExpressionTree = parser.Parse(token);
                    var leafEvaluators = new List<LeafExpression>();
                    PopulateLeafFieldEvaluators(filterExpressionTree, leafEvaluators);
                    foreach (var leafEval in leafEvaluators)
                    {
                        if (!collectors.ContainsKey(leafEval.Left))
                        {
                            var expectationCollector = leafEval.GetExpectation<PowerDevice>();
                            collectors.Add(leafEval.Left, expectationCollector);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        $"Failed to parse assert condition, id={rule.Id}, name={rule.Name}, expression={rule.IfExpression}");
                }

            return collectors;
        }

        private Dictionary<string, Func<PowerDevice, double>> GetRuleScores(ValidationRule rule)
        {
            var scoringAlgorithms = new Dictionary<string, Func<PowerDevice, double>>();
            var assertCondition = rule.IfExpression;
            if (!string.IsNullOrEmpty(assertCondition))
                try
                {
                    var token = JToken.Parse(assertCondition);
                    var parser = new ExpressionParser();
                    var filterExpressionTree = parser.Parse(token);
                    var leafEvaluators = new List<LeafExpression>();
                    PopulateLeafFieldEvaluators(filterExpressionTree, leafEvaluators);
                    foreach (var leafEval in leafEvaluators)
                    {
                        if (!scoringAlgorithms.ContainsKey(leafEval.Left))
                        {
                            var getScore = leafEval.GetScore<PowerDevice>();
                            scoringAlgorithms.Add(leafEval.Left, getScore);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        $"Failed to parse assert condition, id={rule.Id}, name={rule.Name}, expression={rule.IfExpression}");
                }

            return scoringAlgorithms;
        }

        private void PopulateLeafFieldEvaluators(IConditionExpression filterExpressionTree,
            List<LeafExpression> leafEvaluators)
        {
            if (filterExpressionTree is LeafExpression leaf)
                leafEvaluators.Add(leaf);
            else if (filterExpressionTree is AllOfExpression allOf)
                foreach (var leafExpr in allOf.AllOf)
                    PopulateLeafFieldEvaluators(leafExpr, leafEvaluators);
            else if (filterExpressionTree is AnyOfExpression anyOf)
                foreach (var leafExpr in anyOf.AnyOf)
                    PopulateLeafFieldEvaluators(leafExpr, leafEvaluators);
        }
    }
}