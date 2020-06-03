// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterEvaluate_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using System.Collections.Generic;
    using Contexts;
    using Evaluators;
    using LightBDD.Framework;
    using LightBDD.Framework.Parameters;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using TestData;

    public partial class FilterEvaluate_feature : FeatureFixture
    {
        private object evaluationContext;
        private IConditionExpression conditionExpression;
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()},
            }
        };
        private readonly Func<object, string> FormatObject =
            o => JsonConvert.SerializeObject(o, Formatting.Indented, serializerSettings);

        
        private void An_evaluation_context<T>(string cityName)
        {
            evaluationContext = new JsonFixtureFile($"{cityName}.json").JObjectOf<T>();
            StepExecution.Current.Comment($"Current context \n{typeof(T).Name}:\n{FormatObject(evaluationContext)}\n");
        }

        private void I_evaluate_context_with_condition(string left, Operator op, string right)
        {
            conditionExpression = new LeafExpression()
            {
                Left = left,
                Operator = op,
                Right = right
            };
        }

        private void I_evaluate_context_with_filter(IConditionExpression filter)
        {
            conditionExpression = filter;
            StepExecution.Current.Comment($"Current filter:\n{JsonConvert.SerializeObject(conditionExpression, Formatting.Indented)}\n");
        }

        private void Evaluation_results_should_be(Verifiable<bool> expected)
        {
            var builder = new ExpressionBuilder();
            bool actual = false;
            if (evaluationContext is Location location)
            {
                var lambda = builder.Build<Location>(conditionExpression);
                actual = lambda(location);
            }
            else if (evaluationContext is Person person)
            {
                var lambda = builder.Build<Person>(conditionExpression);
                actual = lambda(person);
            }
            else
            {
                Assert.Fail($"context type '{evaluationContext.GetType().Name}' is not supported");
            }

            expected.SetActual(actual);
        }
    }
}