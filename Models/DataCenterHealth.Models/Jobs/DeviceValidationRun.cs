// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationRun.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;

    public class DeviceValidationRun : BaseEntity
    {
        public string JobId { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public int? TotalDevices { get; set; }
        public int? TotalRules { get; set; }

        public DateTime? FinishTime { get; set; }
        public int? TotalPayloads { get; set; }
        public int? TotalEvaluated { get; set; }
        public int? TotalResults { get; set; }
        public string TimeSpan { get; set; }
        public decimal? AverageScore { get; set; }
        public string Error { get; set; }
        public bool Succeed { get; set; }
    }
}