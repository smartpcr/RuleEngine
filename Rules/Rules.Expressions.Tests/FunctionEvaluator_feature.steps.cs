// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionEvaluator_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Contexts;
    using Evaluators;
    using LightBDD.Framework;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TestData;

    public partial class FunctionEvaluator_feature : FeatureFixture
    {
        private object evaluationContext;
        private IConditionExpression conditionExpression;
        
        private void An_evaluation_context<T>(string cityName)
        {
            evaluationContext = new JsonFixtureFile($"{cityName}.json").JObjectOf<T>();
            StepExecution.Current.Comment($"Current context \n{typeof(T).Name}:\n{evaluationContext.FormatObject()}\n");
        }
        
        private void I_evaluate_context_with_filter(IConditionExpression filter)
        {
            conditionExpression = filter;
            StepExecution.Current.Comment($"Current filter:\n{conditionExpression.FormatObject()}\n");
        }
        
        private void Evaluation_results_should_be(Verifiable<bool> expected)
        {
            var builder = new ExpressionBuilder();
            bool actual = false;
            JArray evidence = null;
            if (evaluationContext is Location location)
            {
                var lambda = builder.Build<Location>(conditionExpression);
                actual = lambda(location);
                evidence = GetEvidence(conditionExpression, location);
            }
            else if (evaluationContext is Person person)
            {
                var lambda = builder.Build<Person>(conditionExpression);
                actual = lambda(person);
                evidence = GetEvidence(conditionExpression, person);
            }
            else if (evaluationContext is TreeNode treeNode)
            {
                var lambda = builder.Build<TreeNode>(conditionExpression);
                actual = lambda(treeNode);
                evidence = GetEvidence(conditionExpression, treeNode);
            }
            else
            {
                Assert.Fail($"context type '{evaluationContext.GetType().Name}' is not supported");
            }

            if (evidence != null)
            {
                StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            }

            expected.SetActual(actual);
        }

        private JArray GetEvidence<T>(IConditionExpression condition, T instance)
        {
            var leafEvaluators = new List<LeafExpression>();
            PopulateLeafFieldEvaluators(condition, leafEvaluators);
            var array = new JArray();
            foreach(var leafExpr in leafEvaluators)
            {
                var ctxParameter = Expression.Parameter(typeof(T), "ctx");
                var leftExpression = ctxParameter.BuildExpression(leafExpr.Left);
                var lambda = Expression.Lambda(leftExpression, ctxParameter);
                var getValue = lambda.Compile();
                var actualObj = getValue.DynamicInvoke(instance);

                string expected = leafExpr.Right;
                if (leafExpr.RightSideIsExpression)
                {
                    var rightExpression = ctxParameter.BuildExpression(leafExpr.Right);
                    lambda = Expression.Lambda(rightExpression, ctxParameter);
                    getValue = lambda.Compile();
                    var expectedObj = getValue.DynamicInvoke(instance);
                    expected = JsonConvert.SerializeObject(expectedObj);
                }

                var evidence = new
                {
                    left = leafExpr.Left,
                    actual = actualObj,
                    expected
                };
                array.Add(JToken.FromObject(evidence));
            }

            return array;
        }
        
        private void PopulateLeafFieldEvaluators(IConditionExpression condition,
            List<LeafExpression> leafEvaluators)
        {
            if (condition is LeafExpression leaf)
                leafEvaluators.Add(leaf);
            else if (condition is AllOfExpression allOf)
                foreach (var leafExpr in allOf.AllOf)
                    PopulateLeafFieldEvaluators(leafExpr, leafEvaluators);
            else if (condition is AnyOfExpression anyOf)
                foreach (var leafExpr in anyOf.AnyOf)
                    PopulateLeafFieldEvaluators(leafExpr, leafEvaluators);
        }
    }
}