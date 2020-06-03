// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionEvaluator_feature.cs" company="Microsoft Corporation">
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
        @"In order to enrich validate rules
As a developer
I want to have built-in functions in rule expression")]
    [TestCategory("function")]
    [TestClass]
    public partial class FunctionEvaluator_feature
    {
        [Scenario]
        public void should_be_able_to_get_count_on_obj_array()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
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
        public void should_be_able_to_get_count_on_string_array()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Titles.Count()",
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
        public void should_be_able_to_get_count_on_string_list()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Hobbies.Count()",
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
        public void should_be_able_to_get_dinstinct_count_on_string_list()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Titles.DistinctCount()",
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
        public void should_be_able_to_call_sum()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(Age).Sum()",
                        Operator = Operator.Equals,
                        Right = "40"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_sum_with_arg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Sum(Age)",
                        Operator = Operator.Equals,
                        Right = "40"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_avg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(Age).Average()",
                        Operator = Operator.Equals,
                        Right = "20"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_avg_with_arg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Average(Age)",
                        Operator = Operator.Equals,
                        Right = "20"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_max()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(Age).Max()",
                        Operator = Operator.Equals,
                        Right = "26"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_max_with_arg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Max(Age)",
                        Operator = Operator.Equals,
                        Right = "26"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_min()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(Age).Min()",
                        Operator = Operator.Equals,
                        Right = "14"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_min_with_arg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Min(Age)",
                        Operator = Operator.Equals,
                        Right = "14"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_compare_two_expressions()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(Age).Sum()",
                        Operator = Operator.LessThan,
                        Right = "Age",
                        RightSideIsExpression = true
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_select_on_nested_field()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(FirstName)",
                        Operator = Operator.AllIn,
                        Right = "Tiffany,Barron"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_function_ago()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "BirthDate",
                        Operator = Operator.GreaterThan,
                        Right = "Ago(50000d)",
                        RightSideIsExpression = true
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