// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterParser_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using FluentAssertions;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;
    using Parsers;

    public partial class FilterParser_feature : FeatureFixture
    {
        private JToken jsonExpr;
        private IConditionExpression expression;
        
        private void A_leaf_expression(string json)
        {
            jsonExpr = JToken.Parse(json);
        }

        private void I_parse_json_expression()
        {
            var parser = new ExpressionParser();
            expression = parser.Parse(jsonExpr);
        }
        
        private void I_parse_json_expression_and_it_should_throw(Type exceptionType, string errorMessage)
        {
            var parser = new ExpressionParser();
            Action act = () =>
            {
                expression = parser.Parse(jsonExpr);
            };
            act.Should().Throw<Exception>().Where(e => e.GetType() == exceptionType);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                act.Should().Throw<Exception>().Where(e => e.Message.Contains(errorMessage));
            }
        }

        private void Parsed_expression_should_be(IConditionExpression expected)
        {
            ShouldBeEquivalent(expression, expected);
        }
        
        private void ShouldBeEquivalent(IConditionExpression actual, IConditionExpression expected)
        {
            if (expected == null) actual.Should().BeNull();
            else
            {
                actual.Should().NotBeNull();
                switch (expected)
                {
                    case LeafExpression expectedLeaf:
                    {
                        var actualLeaf = actual as LeafExpression;
                        actualLeaf.Should().NotBeNull();
                        ShouldBeEquivalent(actualLeaf, expectedLeaf);
                        break;
                    }
                    case AllOfExpression expectedAll:
                    {
                        var actualAll = actual as AllOfExpression;
                        actualAll.Should().NotBeNull();
                        ShouldBeEquivalent(actualAll, expectedAll);
                        break;
                    }
                    case AnyOfExpression expectedAny:
                    {
                        var actualAny = actual as AnyOfExpression;
                        actualAny.Should().NotBeNull();
                        ShouldBeEquivalent(actualAny, expectedAny);
                        break;
                    }
                    case NotExpression expectedNot:
                    {
                        var actualNot = actual as NotExpression;
                        actualNot.Should().NotBeNull();
                        ShouldBeEquivalent(actualNot, expectedNot);
                        break;
                    }
                }
            }
        }

        private static void ShouldBeEquivalent(LeafExpression actual, LeafExpression expected)
        {
            actual.Should().BeEquivalentTo(expected);
        }
        
        private void ShouldBeEquivalent(AllOfExpression actual, AllOfExpression expected)
        {
            actual.AllOf.Length.Should().Be(expected.AllOf.Length);
            for (var i = 0; i < actual.AllOf.Length; i++)
            {
                var actualChild = actual.AllOf[i];
                var expectedChild = expected.AllOf[i];
                ShouldBeEquivalent(actualChild, expectedChild);
            }
        }
        
        private void ShouldBeEquivalent(AnyOfExpression actual, AnyOfExpression expected)
        {
            actual.AnyOf.Length.Should().Be(expected.AnyOf.Length);
            for (var i = 0; i < actual.AnyOf.Length; i++)
            {
                var actualChild = actual.AnyOf[i];
                var expectedChild = expected.AnyOf[i];
                ShouldBeEquivalent(actualChild, expectedChild);
            }
        }
        
        private void ShouldBeEquivalent(NotExpression actual, NotExpression expected)
        {
            ShouldBeEquivalent(actual.Not, expected.Not);
        }
    }
}