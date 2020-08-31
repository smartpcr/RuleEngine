// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropPathBuilder_feature_steps.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Builders;
    using Evaluators;
    using FluentAssertions;
    using LightBDD.MsTest2;
    using Models.IoT;
    using TestModels.IoT;

    public partial class PropPathBuilder_feature: FeatureFixture
    {
        private PropertyPathBuilder<Device> propPathBuilder;
        private string startsWith;

        private void Current_path(string currentPath)
        {
            startsWith = currentPath;
            propPathBuilder = new PropertyPathBuilder<Device>(new MockPropValuesProvider());
        }

        private void Next_parts_should_contain(string expectedNextPart, Type expectedNextType, string args)
        {
            var nextParts = propPathBuilder.Next(startsWith);
            nextParts.Should().NotBeNullOrEmpty();
            var found = nextParts.FirstOrDefault(pt => pt.Path == expectedNextPart);
            found.Should().NotBeNull();
            var argValues = args.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            found!.ArgumentCount.Should().Be(argValues.Length);
            if (argValues.Length > 0)
            {
                var contextType = typeof(Device);
                var contextParameter = Expression.Parameter(contextType, "ctx");
                var functionExpr = expectedNextPart.Replace("()", $"({args})");
                var currentPath = string.IsNullOrEmpty(startsWith) ? functionExpr : $"{startsWith}.{functionExpr}";
                var targetExpression = contextParameter.EvaluateExpression(currentPath);
                targetExpression.Type.Should().Be(expectedNextType);
            }
            else
            {
                found!.Type.Should().Be(expectedNextType);
            }
        }
    }
}