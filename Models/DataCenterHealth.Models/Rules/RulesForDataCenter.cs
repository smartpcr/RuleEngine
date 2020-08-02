// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulesForDataCenter.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    using System.Collections.Generic;
    using Jobs;

    public class RulesForDataCenter
    {
        public RuleSet RuleSet { get; set; }
        public List<ValidationRule> ValidationRules { get; set; }
        public List<EvaluationRule> EvaluationRules { get; set; }
        public List<DeviceValidationRun> Runs { get; set; }
        public List<DeviceValidationJob> Jobs { get; set; }
        public decimal? AverageScore { get; set; }
        public decimal? LastScore { get; set; }
    }
}