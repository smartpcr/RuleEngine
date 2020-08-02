// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenterValidationJob.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;

    public class DataCenterValidationJob : BaseEntity
    {
        public string ScheduleName { get; set; }
        public string DcName { get; set; }
        public List<string> RuleIds { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string SubmittedBy { get; set; }
    }
}