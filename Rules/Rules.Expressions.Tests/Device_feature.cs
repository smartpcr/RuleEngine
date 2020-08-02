// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Context_feature.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System;
    using LightBDD.Framework;
    using LightBDD.Framework.Scenarios;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [FeatureDescription(
        @"In order to validate devices
As a developer
I want to able to evaluate all properties of device")]
    [TestCategory("device")]
    [TestClass]
    public partial class Device_feature
    {
        [Scenario]
        [DataRow("DeviceType", "in", "Breaker", true, 1.0)]
        [DataRow("deviceState", "equals", "NormallyClosed", true, 1.0)]
        [DataRow("hierarchy", "equals", "LVS-Colo", true, 1.0)]
        [DataRow("ampRating", "isNull", "", true, 1.0)]
        [DataRow("primaryParent", "contains", "AMS05-COLO1-UPS01", true, 1.0)]
        [DataRow("panelName", "contains", "AMS05-COLO1-UPS01", true, 1.0)]
        [DataRow("amperage", "greaterThan", "1250", false, 1.0)] // score is 1 because there is no defference
        [DataRow("voltage", "equals", "400", true, 1.0)]
        [DataRow("ratedCapacity", "lessThan", "900", true, 1.0)]
        [DataRow("deRatedCapacity", "lessOrEqual", "866.025403784439", true, 1.0)]
        [DataRow("dataType", "equals", "Sentron WL", true, 1.0)]
        public void Should_be_able_to_validate_device_props(string left, string actualOp, string right, bool shouldPass, double expectedScore)
        {
            var op = (Operator) Enum.Parse(typeof(Operator), actualOp, true);
            Runner.RunScenario(
                given => A_device("device_with_relations"),
                when => I_evaluate_device_with_condition(left, op, right),
                then => Evaluation_results_should_be(shouldPass),
                and => Evidence_should_produce_a_score(expectedScore));
        }

        [Scenario]
        [DataRow("primaryParentDevice.deviceType", "in", "Breaker", true, 1.0)]
        [DataRow("primaryParentDevice", "notIsNull", "", true, 1.0)]
        [DataRow("children.Count()", "equals", "0", true, 1.0)]
        [DataRow("children", "isEmpty", "", true, 1.0)]
        [DataRow("rootDevice.hierarchy", "in", "MVS-SubStation", true, 1.0)]
        [DataRow("rootDevice", "isNull", "", false, 0.0)]
        [DataRow("siblingDevices.Count", "equals", "0", true, 1.0)]
        public void Should_be_able_to_validate_relations(string left, string actualOp, string right, bool shouldPass, double expectedScore)
        {
            var op = (Operator) Enum.Parse(typeof(Operator), actualOp, true);
            Runner.RunScenario(
                given => A_device("device_with_relations"),
                when => I_evaluate_device_with_condition(left, op, right),
                then => Evaluation_results_should_be(shouldPass),
                and => Evidence_should_produce_a_score(expectedScore));
        }

        [Scenario]
        [DataRow("DataPoints.Count()", "equals", "18", true, 1.0)]
        [DataRow("DataPoints.Where(dataPoint, Equals, Volts.Vcn).Count()", "equals", "1", true, 1.0)]
        [DataRow("DataPoints.Where(dataPoint, Equals, Volts.Vcn).First().pollInterval", "equals", "60000", true, 1.0)]
        public void Should_be_able_to_validate_data_points(string left, string actualOp, string right, bool shouldPass, double expectedScore)
        {
            var op = (Operator) Enum.Parse(typeof(Operator), actualOp, true);
            Runner.RunScenario(
                given => A_device("device_with_data_points"),
                when => I_evaluate_device_with_condition(left, op, right),
                then => Evaluation_results_should_be(shouldPass),
                and => Evidence_should_produce_a_score(expectedScore));
        }

        [Scenario]
        [DataRow("readingStats.Count()", "equals", "11", true, 1.0)]
        [DataRow("readingStats.Select(DataPoint).Count()", "equals", "11", true, 1.0)]
        [DataRow("readingStats.Select(DataPoint)", "allIn", "Amps.Ia,Amps.Ib,Amps.Ic,Energy.kWh,Freq.Freq,Pwr.kW tot,Status.Close,Status.Trip,Volt.Vab,Volt.Vbc,Volt.Vca", true, 1.0)]
        [DataRow("readingStats.Select(DataPoint)", "containsAll", "Amps.Ia,Amps.Ib,Amps.Ic", true, 1.0)]
        [DataRow("readingStats.Where(dataPoint, Equals, Amps.Ia).Count()", "equals", "1", true, 1.0)]
        [DataRow("readingStats.Where(dataPoint, Equals, Amps.Ia).First().Avg", "equals", "729.20", true, 1.0)]
        [DataRow("readingStats.Where(dataPoint, Equals, Amps.Ia).First().Avg", "diffWithinPct", "750", true, 1.0, "10")]
        public void Should_be_able_to_validate_zenon_events(string left, string actualOp, string right, bool shouldPass, double expectedScore, params string[] additionalArgs)
        {
            var op = (Operator) Enum.Parse(typeof(Operator), actualOp, true);
            Runner.RunScenario(
                given => A_device("device_with_zenon_events"),
                when => I_evaluate_device_with_condition(left, op, right, additionalArgs),
                then => Evaluation_results_should_be(shouldPass),
                and => Evidence_should_produce_a_score(expectedScore));
        }
    }
}