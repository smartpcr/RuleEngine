// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationResult.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Validation
{
    using System;
    using System.Collections.Generic;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;

    public class EvaluationResult : BaseEntity
    {
        public bool? Passed { get; set; }
        public double Score { get; set; }
        public string ContextType { get; set; }
        public string ContextId { get; set; }
        public string DeviceName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleSetName { get; set; }
        public RuleContext RuleContext { get; set; }
        public RuleState RuleState { get; set; }
        public RuleType RuleType { get; set; }
        public string RunId { get; set; }
        public string JobId { get; set; }
        public List<DeviceValidationEvidence> Evidences { get; set; }
        public DateTime EvaluationTime { get; set; }
        public string Error { get; set; }
    }
}