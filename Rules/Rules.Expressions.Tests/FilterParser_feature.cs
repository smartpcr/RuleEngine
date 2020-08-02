// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterParser_feature.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using System.ComponentModel;
    using LightBDD.Framework;
    using LightBDD.Framework.Scenarios;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [FeatureDescription(
        @"In order to create rule
As a developer
I want be able to parse json expression")]
    [Label("parser")]
    [TestCategory("parser")]
    [TestClass]
    public partial class FilterParser_feature
    {
        [Scenario]
        [Label("single_filter")]
        public void Accept_single_filter()
        {
            var json = $@"
{{
    ""left"": ""DeviceType"",
    ""operator"": ""Equals"",
    ""right"": ""Breaker""
}}";
            var expected = new LeafExpression()
            {
                Left = "DeviceType",
                Operator = Operator.Equals,
                Right = "Breaker",
                RightSideIsExpression = false
            };

            Runner.RunScenario(
                given => A_leaf_expression(json),
                when => I_parse_json_expression(),
                then => Parsed_expression_should_be(expected));
        }

        [Scenario]
        [Label("multiple_filter")]
        public void Accept_multiple_filters()
        {
            var json = $@"
{{
    ""allOf"": [
        {{
            ""left"": ""DeviceType"",
            ""operator"": ""Equals"",
            ""right"": ""Breaker""
        }},
        {{
            ""left"": ""Hierarchy"",
            ""operator"": ""In"",
            ""right"": ""ATS""
        }}
    ]
}}";
            var expected = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "DeviceType",
                        Operator = Operator.Equals,
                        Right = "Breaker",
                        RightSideIsExpression = false
                    },
                    new LeafExpression()
                    {
                        Left = "Hierarchy",
                        Operator = Operator.In,
                        Right = "ATS",
                        RightSideIsExpression = false
                    }
                }
            };

            Runner.RunScenario(
                given => A_leaf_expression(json),
                when => I_parse_json_expression(),
                then => Parsed_expression_should_be(expected));
        }

        [Scenario]
        [Label("mixed_filter")]
        public void Accept_mixed_logical_operators()
        {
            var json = $@"
{{
    ""anyOf"": [ 
        {{
            ""allOf"": [
                {{
                    ""left"": ""DeviceType"",
                    ""operator"": ""Equals"",
                    ""right"": ""Breaker""
                }},
                {{
                    ""left"": ""Hierarchy"",
                    ""operator"": ""In"",
                    ""right"": ""ATS""
                }}
            ],
        }},
        {{
            ""left"": ""DeviceState"",
            ""operator"": ""NotIn"",
            ""right"": ""NormallyOpen,NormallyClosed""
        }}
    ]
}}";
            var expected = new AnyOfExpression()
            {
                AnyOf = new IConditionExpression[]
                {
                    new AllOfExpression()
                    {
                        AllOf = new IConditionExpression[]
                        {
                            new LeafExpression()
                            {
                                Left = "DeviceType",
                                Operator = Operator.Equals,
                                Right = "Breaker",
                                RightSideIsExpression = false
                            },
                            new LeafExpression()
                            {
                                Left = "Hierarchy",
                                Operator = Operator.In,
                                Right = "ATS",
                                RightSideIsExpression = false
                            }
                        }
                    },
                    new LeafExpression()
                    {
                        Left = "DeviceState",
                        Operator = Operator.NotIn,
                        Right = "NormallyOpen,NormallyClosed"
                    }
                }
            };

            Runner.RunScenario(
                given => A_leaf_expression(json),
                when => I_parse_json_expression(),
                then => Parsed_expression_should_be(expected));
        }

        [Scenario]
        [Label("missing_left_side")]
        public void Missing_left_side()
        {
            var json = @"
{
    ""operator"": ""equals"",
    ""right"": """"
}";
            Runner.RunScenario(
                given => A_leaf_expression(json),
                when => I_parse_json_expression_and_it_should_throw(typeof(FormatException), "Expression provided does NOT contain the required fields"));
        }
        
        [Scenario]
        [Label("missing_operator")]
        public void Missing_operator()
        {
            var json = @"
{
    ""left"": ""field"",
    ""right"": """"
}";
            Runner.RunScenario(
                given => A_leaf_expression(json),
                when => I_parse_json_expression_and_it_should_throw(typeof(FormatException), "Expression provided does NOT contain the required fields"));
        }
        
        [Scenario]
        [Label("operators")]
        [DataRow("DeviceType", "equals", "Breaker", false, Operator.Equals)]
        [DataRow("DeviceType", "notEquals", "Breaker", false, Operator.NotEquals)]
        [DataRow("DeviceType", "greaterThan", "Breaker", false, Operator.GreaterThan)]
        [DataRow("DeviceType", "greaterOrEqual", "Breaker", false, Operator.GreaterOrEqual)]
        [DataRow("DeviceType", "lessThan", "Breaker", false, Operator.LessThan)]
        [DataRow("DeviceType", "lessOrEqual", "Breaker", false, Operator.LessOrEqual)]
        [DataRow("DeviceType", "contains", "Breaker", false, Operator.Contains)]
        [DataRow("DeviceType", "notContains", "Breaker", false, Operator.NotContains)]
        [DataRow("DeviceType", "containsAll", "Breaker", false, Operator.ContainsAll)]
        [DataRow("DeviceType", "notContainsAll", "Breaker", false, Operator.NotContainsAll)]
        [DataRow("DeviceType", "startsWith", "Breaker", false, Operator.StartsWith)]
        [DataRow("DeviceType", "notStartsWith", "Breaker", false, Operator.NotStartsWith)]
        [DataRow("DeviceType", "in", "Breaker", false, Operator.In)]
        [DataRow("DeviceType", "notIn", "Breaker", false, Operator.NotIn)]
        [DataRow("DeviceType", "allIn", "Breaker", false, Operator.AllIn)]
        [DataRow("DeviceType", "notAllIn", "Breaker", false, Operator.NotAllIn)]
        [DataRow("DeviceType", "anyIn", "Breaker", false, Operator.AnyIn)]
        [DataRow("DeviceType", "notAnyIn", "Breaker", false, Operator.NotAnyIn)]
        [DataRow("DeviceType", "isNull", "Breaker", false, Operator.IsNull)]
        [DataRow("DeviceType", "notIsNull", "Breaker", false, Operator.NotIsNull)]
        [DataRow("DeviceType", "isEmpty", "Breaker", false, Operator.IsEmpty)]
        [DataRow("DeviceType", "notIsEmpty", "Breaker", false, Operator.NotIsEmpty)]
        [DataRow("DeviceType", "invalidOperator", "Breaker", false, Operator.NotIsEmpty, typeof(JsonSerializationException), "Error converting value \"invalidOperator\"")]
        public void Should_support_list_of_operators(string left, string actualOp, string right, bool rightIsExpr,
            Operator expectedOp, Type exType = null, string errorMessage = null)
        {
            var boolStr = rightIsExpr ? "true" : "false";
            var json = $@"
{{
    ""left"": ""{left}"",
    ""operator"": ""{actualOp}"",
    ""right"": ""{right}"",
    ""RightSideIsExpression"": {boolStr}
}}";
            var expected = new LeafExpression()
            {
                Left = left,
                Operator = expectedOp,
                Right = right,
                RightSideIsExpression = rightIsExpr
            };

            if (exType != null)
            {
                Runner.RunScenario(
                    given => A_leaf_expression(json),
                    when => I_parse_json_expression_and_it_should_throw(exType, errorMessage));
            }
            else
            {
                Runner.RunScenario(
                    given => A_leaf_expression(json),
                    when => I_parse_json_expression(),
                    then => Parsed_expression_should_be(expected));
            }
        }
    }
}