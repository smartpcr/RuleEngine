// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidation_feature.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using LightBDD.Framework;
    using LightBDD.Framework.Scenarios;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [FeatureDescription(
        @"In order to create validation rule
As a developer
I want to evaluate json rule against power device")]
    [TestCategory("json-rule")]
    [TestClass]
    public partial class DeviceValidation_feature
    {
        [Scenario]
        public void power_consumption_check()
        {
            Runner.RunScenario(
                given => A_device("device_with_zenon_events"),
                when => I_use_json_rule("power_consumption_check"),
                then => Filter_results_should_be(true),
                then => Assert_results_should_be(true));
        }

        [Scenario]
        public void staleness_check()
        {
            Runner.RunScenario(
                given => A_device("device_with_zenon_events"),
                when => I_use_json_rule("staleness_check"),
                then => Filter_results_should_be(true),
                then => Assert_results_should_be(true));
        }

        [Scenario]
        public void should_be_able_to_call_where_with_contains()
        {
            IConditionExpression filterExpr = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "LastReadings.Where(DataPoint, contains, 'Pwr.kW tot')",
                        Operator = Operator.NotIsEmpty,
                        Right = ""
                    }
                }
            };

            Runner.RunScenario(
                given => A_device("device_with_zenon_events"),
                when => A_filter_condition(filterExpr),
                then => Context_should_pass_filter(true));
        }
    }
}