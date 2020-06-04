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
        public void should_be_able_to_select_with_nested_function_inside()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Select(Hobbies.OrderByDesc().First())",
                        Operator = Operator.AllIn,
                        Right = "Music,Soccer"
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
        
        [Scenario]
        public void should_be_able_to_call_function_where()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Where(FirstName, Equals, Tiffany).Count()",
                        Operator = Operator.Equals,
                        Right = "1"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_function_first()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.First(FirstName, Equals, Tiffany).Age",
                        Operator = Operator.GreaterThan,
                        Right = "25"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_first_with_no_arg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Hobbies.First()",
                        Operator = Operator.Equals,
                        Right = "Golf"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_function_last()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.Last(FirstName, Equals, Tiffany).Age",
                        Operator = Operator.GreaterThan,
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
        public void should_be_able_to_call_last_with_no_arg()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Hobbies.Last()",
                        Operator = Operator.In,
                        Right = "Golf,Tweeter"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_orderby()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.OrderBy(FirstName).First().FirstName",
                        Operator = Operator.Equals,
                        Right = "Barron"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_to_call_orderbydesc()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Children.OrderByDesc(FirstName).First().FirstName",
                        Operator = Operator.Equals,
                        Right = "Tiffany"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<Person>("donald_trump"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_be_able_traverse_along_link()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Traverse(Parent, Id)",
                        Operator = Operator.AllIn,
                        Right = "A,B,C"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<TreeNode>("correct_tree"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_stop_traverse_before_max_steps_reached()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Traverse(Parent, Id, 1)",
                        Operator = Operator.AllIn,
                        Right = "A,B"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<TreeNode>("correct_tree"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
        
        [Scenario]
        public void should_stop_traverse_at_loop()
        {
            IConditionExpression filter = new AllOfExpression()
            {
                AllOf = new IConditionExpression[]
                {
                    new LeafExpression()
                    {
                        Left = "Traverse(Parent, Id)",
                        Operator = Operator.ContainsAll,
                        Right = "A,B,C,!!"
                    }
                }
            };
            
            Runner.RunScenario(
                given => An_evaluation_context<TreeNode>("tree_with_loop"),
                when => I_evaluate_context_with_filter(filter),
                then => Evaluation_results_should_be(true));
        }
    }
}