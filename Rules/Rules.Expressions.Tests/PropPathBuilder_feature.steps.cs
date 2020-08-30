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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using LightBDD.Framework;
    using LightBDD.MsTest2;
    using Rules.Expressions.Helpers;
    using TestModels;
    using TestModels.IoT;

    public partial class PropPathBuilder_feature: FeatureFixture
    {
        private static readonly Regex PartSplitter = new Regex(@"\.(?![^(]*\))", RegexOptions.Compiled); // only matches dot (.) outside parenthesis
        private string startsWith;
        private List<PropertyPath> allPropPaths;
        private List<PropertyPath> nextParts;

        private void Current_path(string currentPath)
        {
            startsWith = currentPath;
            var propPathBuilder = new PropertyPathBuilder<Device>(
                new MockPropExpressionBuilder(),
                new MockPropValuesProvider());
            allPropPaths = propPathBuilder.AllPropPaths;
            nextParts = propPathBuilder.GetNextFieldPart(currentPath);
            StepExecution.Current.Comment($"Evidence\n{nextParts.FormatObject()}\n");
        }

        private void Depth_of_all_prop_path_should_be_not_be_larger_than(int allowedDepth)
        {
            var maxDepth = allPropPaths.Select(pt => PartSplitter.Split(pt.Path).Length).Max();
            maxDepth.Should().BeLessOrEqualTo(allowedDepth);
        }

        private void Total_of_all_prop_path_should_be_not_be_greater_than(int maxCount)
        {
            allPropPaths.Count.Should().BeLessOrEqualTo(maxCount);
        }

        private void Next_parts_should_contain(string expectedNextPart, Type expectedNextType)
        {
            nextParts.Should().NotBeNullOrEmpty();
            var currentPath = string.IsNullOrEmpty(startsWith) ? expectedNextPart : $"{startsWith}.{expectedNextPart}";
            var found = nextParts.FirstOrDefault(pt => pt.Path == currentPath);
            found.Should().NotBeNull();
            found.Type.Should().Be(expectedNextType);
        }
    }
}