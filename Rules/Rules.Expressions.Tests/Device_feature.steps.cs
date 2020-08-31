// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Device_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System.Linq;
    using Evidences;
    using LightBDD.Framework;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Models.IoT;
    using Rules.Expressions;
    using Rules.Expressions.Evaluators;
    using TestModels;
    using TestModels.IoT;

    public partial class Device_feature : FeatureFixture
    {
        private Device evaluationContext;
        private IConditionExpression conditionExpression;

        private void A_device(string testFileName)
        {
            evaluationContext = new JsonFixtureFile($"{testFileName}.json").JObjectOf<Device>();
        }

        private void I_evaluate_device_with_condition(string left, Operator op, string right, params string[] additionalArgs)
        {
            conditionExpression = new LeafExpression()
            {
                Left = left,
                Operator = op,
                Right = right,
                OperatorArgs = additionalArgs
            };
        }

        private void Evaluation_results_should_be(Verifiable<bool> expected)
        {
            var builder = new ExpressionEvaluator();
            var lambda = builder.Evaluate<Device>(conditionExpression);
            var actual = lambda(evaluationContext);
            expected.SetActual(actual);
        }

        private void Evidence_should_produce_a_score(Verifiable<double> expected)
        {
            var evidence = conditionExpression.GetEvidence(evaluationContext);
            StepExecution.Current.Comment($"Evidence\n{evidence.FormatObject()}\n");
            var score = evidence.Average(e => e.Score);
            expected.SetActual(score);
        }
    }
}