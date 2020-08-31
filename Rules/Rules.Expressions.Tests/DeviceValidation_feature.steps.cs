// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidation_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Evidences;
    using LightBDD.Framework;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Models.IoT;
    using Models.Rules;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Rules.Expressions.Evaluators;
    using Rules.Expressions.Helpers;
    using Rules.Expressions.Parsers;
    using TestModels;
    using TestModels.IoT;

    public partial class DeviceValidation_feature : FeatureFixture
    {
        private Device evaluationContext;
        private IConditionExpression whenCondition;
        private Func<Device, bool> filter;
        private IConditionExpression ifCondition;
        private Func<Device, bool> assert;

        private void A_device(string testFileName)
        {
            evaluationContext = new JsonFixtureFile($"{testFileName}.json").JObjectOf<Device>();
            var random = new Random();
            foreach (var reading in evaluationContext.LastReadings)
            {
                if (reading.DataPoint.Contains("Pwr.kW tot", StringComparison.OrdinalIgnoreCase))
                {
                    var minutesAgo = random.Next(25);
                    reading.EventTime = DateTime.UtcNow.AddMinutes(0 - minutesAgo);
                }
            }
        }

        private void A_filter_condition(IConditionExpression filterCondition)
        {
            whenCondition = filterCondition;
            IExpressionEvaluator evaluator = new ExpressionEvaluator();
            filter = evaluator.Evaluate<Device>(whenCondition);
        }

        private void I_use_json_rule(string jsonRuleFile)
        {
            var parser = new ExpressionParser();
            IExpressionEvaluator evaluator = new ExpressionEvaluator();
            var rule = new JsonFixtureFile($"{jsonRuleFile}.json").JObjectOf<Rule>();
            var whenExpression = JObject.Parse(rule.WhenExpression);
            StepExecution.Current.Comment($"Current filter:\n{whenExpression.FormatObject()}\n");
            whenCondition = parser.Parse(whenExpression);
            filter = evaluator.Evaluate<Device>(whenCondition);

            var ifExpression = JObject.Parse(rule.IfExpression);
            StepExecution.Current.Comment($"Current assert:\n{ifExpression.FormatObject()}\n");
            ifCondition = parser.Parse(ifExpression);
            assert = evaluator.Evaluate<Device>(ifCondition);
        }

        private void Context_should_pass_filter(Verifiable<bool> expected)
        {
            var evidence = GetEvidence(whenCondition, evaluationContext);
            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            var actual = filter(evaluationContext);
            expected.SetActual(actual);
        }

        private void Filter_results_should_be(Verifiable<bool> expected)
        {
            var evidence = GetEvidence(whenCondition, evaluationContext);
            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            var actual = filter(evaluationContext);
            expected.SetActual(actual);
        }

        private void Assert_results_should_be(Verifiable<bool> expected)
        {
            var evidence = GetEvidence(ifCondition, evaluationContext);
            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            var actual = assert(evaluationContext);
            expected.SetActual(actual);
        }

        private JArray GetEvidence<T>(IConditionExpression condition, T instance)
        {
            var leafEvaluators = new List<LeafExpression>();
            condition.PopulateLeafFieldEvaluators(leafEvaluators);
            var array = new JArray();
            foreach(var leafExpr in leafEvaluators)
            {
                var ctxParameter = Expression.Parameter(typeof(T), "ctx");
                var leftExpression = ctxParameter.EvaluateExpression(leafExpr.Left);
                var lambda = Expression.Lambda(leftExpression, ctxParameter);
                var getValue = lambda.Compile();
                var actualObj = getValue.DynamicInvoke(instance);

                string expected = leafExpr.Right;
                if (leafExpr.RightSideIsExpression)
                {
                    var rightExpression = ctxParameter.EvaluateExpression(leafExpr.Right);
                    lambda = Expression.Lambda(rightExpression, ctxParameter);
                    getValue = lambda.Compile();
                    var expectedObj = getValue.DynamicInvoke(instance);
                    expected = JsonConvert.SerializeObject(expectedObj);
                }

                var evidence = new
                {
                    left = leafExpr.Left,
                    op = leafExpr.Operator.ToString(),
                    actual = actualObj,
                    expected
                };
                array.Add(JToken.FromObject(evidence));
            }

            return array;
        }
    }
}