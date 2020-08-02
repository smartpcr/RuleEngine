// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterRuleTestData.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    public class FilterRuleTestData
    {
        public ValidationRule ValidationRule { get; set; }
        public EvaluationContext EvaluationContext { get; set; }
    }

    public class EvaluationContext
    {
        public string ContextType { get; set; }
        public ContextDataRow[] Rows { get; set; }
    }

    public class ContextDataRow
    {
        public string Id { get; set; }
        public ContextField[] Fields { get; set; }
    }

    public class ContextField
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }


    public class TestResult
    {
        public TestResult(string id, bool passed)
        {
            Id = id;
            Passed = passed;
        }

        public TestResult(string id, string error)
        {
            Id = id;
            Passed = false;
            Error = error;
        }

        public string Id { get; set; }
        public bool Passed { get; set; }
        public string Error { get; set; }
    }
}