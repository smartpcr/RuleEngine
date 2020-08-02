// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenterValidationRun.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;

    public class DataCenterValidationRun : BaseEntity
    {
        public string JobId { get; set; }
        public List<string> ValidationRuleIds { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public int? TotalDevices { get; set; }
        public DateTime? FinishTime { get; set; }
        public string TimeSpan { get; set; }
        public decimal? AverageScore { get; set; }
        public string Error { get; set; }
        public bool Succeed { get; set; }
    }
}