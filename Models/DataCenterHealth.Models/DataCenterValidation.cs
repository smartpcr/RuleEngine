// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenterValidation.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System;

    public class DataCenterValidation
    {
        public string DcName { get; set; }
        public string DcGeneration { get; set; }
        public string DeviceType { get; set; }
        public string Hierarchy { get; set; }
        public int TotalDevices { get; set; }
        public int TotalEvaluated { get; set; }
        public int TotalRuns { get; set; }
        public int TotalRules { get; set; }
        public DateTime LastRunTime { get; set; }
        public decimal AverageScore { get; set; }
        public decimal LastScore { get; set; }
    }
}