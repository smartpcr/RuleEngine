// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropPathBuilder_feature.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using System.Collections.Generic;
    using LightBDD.Framework;
    using LightBDD.Framework.Scenarios;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models.IoT;

    [FeatureDescription(
        @"In order to build validation expression
As a developer
I want to be able to discover property paths of device")]
    [TestCategory("device")]
    [TestClass]
    public partial class PropPathBuilder_feature
    {
        [Scenario]
        [DataRow("", "DeviceType", typeof(DeviceType))]
        [DataRow("", "Hierarchy", typeof(string))]
        [DataRow("", "KvaRating", typeof(double?))]
        [DataRow("", "PrimaryParentDevice", typeof(DeviceData))]
        public void Should_be_able_to_discover_simple_prop(string current, string expectedNextPart, Type expectedNextType, string args = "")
        {
            Runner.RunScenario(
                given => Current_path(current),
                then => Next_parts_should_contain(expectedNextPart, expectedNextType, args));
        }

        [Scenario]
        [DataRow("", "Children", typeof(List<DeviceData>))]
        [DataRow("Children", "Count()", typeof(int))]
        [DataRow("ReadingStats", "Select(DataPoint)", typeof(IEnumerable<string>))]
        [DataRow("ReadingStats.Select(DataPoint)", "Count()", typeof(int))]
        [DataRow("ReadingStats", "OrderByDesc(Avg)", typeof(IEnumerable<ReadingStats>))]
        [DataRow("LastReadings", "Where()", typeof(IEnumerable<LastReading>), "DataPoint, Equals, 'Pwr.kW tot'")]
        [DataRow("LastReadings", "Select(Value)", typeof(IEnumerable<double>))]
        public void Should_be_able_to_discover_collection_prop(string current, string expectedNextPart, Type expectedNextType, string args = "")
        {
            Runner.RunScenario(
                given => Current_path(current),
                then => Next_parts_should_contain(expectedNextPart, expectedNextType, args));
        }

        [Scenario]
        [DataRow("ReadingStats.OrderByDesc(Avg)", "First()", typeof(ReadingStats))]
        public void Should_be_able_to_order_and_pick_collections(string current, string expectedNextPart, Type expectedNextType, string args = "")
        {
            Runner.RunScenario(
                given => Current_path(current),
                then => Next_parts_should_contain(expectedNextPart, expectedNextType, args));
        }

        [Scenario]
        public void Should_be_able_to_use_where_within_collections()
        {
            Runner.RunScenario(
                given => Current_path("ReadingStats.OrderByDesc(Avg)"),
                then => Next_parts_should_contain("Where()", typeof(IEnumerable<ReadingStats>), "DataPoint, Equals, 'Pwr.kW tot'"));
        }

        [Scenario]
        public void Should_be_able_to_use_number_aggregate_within_collections()
        {
            Runner.RunScenario(
                given => Current_path("ReadingStats.OrderByDesc(Avg).Where(DataPoint, Equals, 'Pwr.kW tot')"),
                then => Next_parts_should_contain("Sum(Avg)", typeof(double), ""));
        }

        [Scenario]
        public void Should_be_able_to_select_nested_collection_within_collections()
        {
            Runner.RunScenario(
                given => Current_path("Children.SelectMany(ReadingStats).Where(DataPoint, Equals, 'Pwr.kW tot')"),
                then => Next_parts_should_contain("Sum(Avg)", typeof(double), ""));
        }

        [Scenario]
        public void Should_be_able_to_traverse_recursive_props()
        {
            Runner.RunScenario(
                given => Current_path("Traverse(PrimaryParent)"),
                then => Next_parts_should_contain("Last()", typeof(string), ""));
        }
    }
}