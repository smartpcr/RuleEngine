// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationRule.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    using DataCenterHealth.Models.Summaries;

    [TrackChange(true, ChangeType.ValidationRule)]
    public class ValidationRule : Rule
    {
        public string ContextProvider { get; set; }
        public string WhenExpression { get; set; }
        public string IfExpression { get; set; }
        public decimal TrueScore { get; set; }
        public decimal FalseScore { get; set; }

        public ValidationRule()
        {
            Type = RuleType.JsonRule;
        }
    }
}