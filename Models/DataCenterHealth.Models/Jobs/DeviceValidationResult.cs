// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;

    public class DeviceValidationResult : BaseEntity
    {
        public string RunId { get; set; }
        public string JobId { get; set; }
        public string ValidationRuleId { get; set; }
        public string DeviceName { get; set; }
        public DateTime ExecutionTime { get; set; }
        public bool? Assert { get; set; }
        public string Error { get; set; }
        public decimal? Score { get; set; }
    }

    public class DeviceValidationDetail
    {
        public string DcName { get; set; }
        public string DeviceType { get; set; }
        public string Hierarchy { get; set; }
        public string DeviceState { get; set; }
        public string DeviceName { get; set; }
        public string RuleName { get; set; }
        public bool? Assert { get; set; }
        public decimal? Score { get; set; }
        public List<DeviceValidationEvidence> Evidences { get; set; }
        public DateTime ExecutionTime { get; set; }
    }

    public class DeviceValidationEvidence
    {
        public string PropertyPath { get; set; }
        public string Expected { get; set; }
        public string Operator { get; set; }
        public string Actual { get; set; }
        public double Score { get; set; }
        public string Error { get; set; }
    }
}