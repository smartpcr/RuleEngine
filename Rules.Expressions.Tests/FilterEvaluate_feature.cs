// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterEvaluate_feature.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using Contexts;
    using LightBDD.Framework;
    using LightBDD.Framework.Scenarios;
    using LightBDD.MsTest2;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [FeatureDescription(
        @"In order to validate rules
As a developer
I want to evaluate rules against strongly typed context")]
    [TestCategory("evaluate")]
    [TestClass]
    public partial class FilterEvaluate_feature
    {
        [Scenario]
        public void verify_context_with_equals_filter_returns_true()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("City", Operator.Equals, "Redmond"),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_number_filter_returns_true()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("Population", Operator.GreaterThan, "100000"),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_number_filter_returns_false()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("AvgIncome", Operator.LessOrEqual, "100000"),
                then => Evaluation_results_should_be(false));
        }
        
        [Scenario]
        public void verify_context_with_in_filter_returns_true()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("State", Operator.In, "CA,AZ,WA"),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_in_filter_trim_white_spaces_returns_true()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("State", Operator.In, "CA, AZ, WA, , DC"),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_in_filter_returns_false()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("State", Operator.In, "CA,AZ"),
                then => Evaluation_results_should_be(false));
        }
        
        [Scenario]
        public void verify_context_with_not_in_filter_returns_true()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("State", Operator.NotIn, "CA,AZ"),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_null_field_filter_returns_false()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("Street", Operator.Contains, "Main"),
                then => Evaluation_results_should_be(false));
        }
        
        [Scenario]
        public void verify_context_with_contains_filter_returns_true()
        {
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_condition("City", Operator.Contains, "mond"),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_composite_filters_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new NotExpression()
                    {
                        Not = new LeafExpression()
                        {
                            Left = "Country",
                            Operator = Operator.Equals,
                            Right = "Canada"
                        }
                    }, 
                    new LeafExpression()
                    {
                        Left = "City",
                        Operator = Operator.Equals,
                        Right = "Redmond"
                    }, 
                    new LeafExpression()
                    {
                        Left = "State",
                        Operator = Operator.Equals,
                        Right = "WA"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Location>("redmond"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }

        [Scenario]
        public void verify_context_with_nested_properties_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "FirstName",
                        Operator = Operator.Equals,
                        Right = "Donald"
                    }, 
                    new LeafExpression()
                    {
                        Left = "LastName",
                        Operator = Operator.Equals,
                        Right = "Trump"
                    }, 
                    new LeafExpression()
                    {
                        Left = "Spouse.FirstName",
                        Operator = Operator.Equals,
                        Right = "Melania"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_array_contains_filter_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "FirstName",
                        Operator = Operator.Equals,
                        Right = "Donald"
                    }, 
                    new LeafExpression()
                    {
                        Left = "Hobbies",
                        Operator = Operator.Contains,
                        Right = "Golf"
                    }, 
                    new LeafExpression()
                    {
                        Left = "Spouse.FirstName",
                        Operator = Operator.NotEquals,
                        Right = "Ivanka"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_indexed_array_filter_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "FirstName",
                        Operator = Operator.Equals,
                        Right = "Donald"
                    }, 
                    new LeafExpression()
                    {
                        Left = "Hobbies",
                        Operator = Operator.Contains,
                        Right = "Golf"
                    }, 
                    new LeafExpression()
                    {
                        Left = "Spouse.FirstName",
                        Operator = Operator.StartsWith,
                        Right = "Mel"
                    },
                    new LeafExpression()
                    {
                        Left = "Children[0].FirstName",
                        Operator = Operator.Equals,
                        Right = "Tiffany"
                    },
                    new LeafExpression()
                    {
                        Left = "Titles[1]",
                        Operator = Operator.NotEquals,
                        Right = "Scientist"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_enum_field_in_filter_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "FirstName",
                        Operator = Operator.Equals,
                        Right = "Donald"
                    },
                    new LeafExpression()
                    {
                        Left = "Race",
                        Operator = Operator.In,
                        Right = "Black,White"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_date_field_filter_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "FirstName",
                        Operator = Operator.Equals,
                        Right = "Donald"
                    },
                    new LeafExpression()
                    {
                        Left = "BirthDate",
                        Operator = Operator.LessThan,
                        Right = "2020-04-07T15:47:54.760654-07:00"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_expression_on_each_side_returns_true()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children[0].LastName",
                        Operator = Operator.Equals,
                        Right = "Children[1].LastName",
                        RightSideIsExpression = true
                    },
                    new LeafExpression()
                    {
                        Left = "Children.Count()",
                        Operator = Operator.Equals,
                        Right = "2"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void verify_context_with_diff_check()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children[0].Age",
                        Operator = Operator.DiffWithinPct,
                        Right = "Children[1].Age",
                        RightSideIsExpression = true,
                        OperatorArgs = new []{"100"}
                    },
                    new LeafExpression()
                    {
                        Left = "Children.Count()",
                        Operator = Operator.Equals,
                        Right = "2"
                    }
                }
            };
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
    }
}