// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueJobRequest.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;
    using DataCenterHealth.Models.Summaries;

    [TrackExecution(true, ExecutionType.OnDemandValidation)]
    public class QueueJobRequest : BaseEntity
    {
        public string Name { get; set; }
        public List<string> DcNames { get; set; }
        public List<string> DeviceNames { get; set; }
        public List<string> RuleSetIds { get; set; }
        public List<string> RuleIds { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmissionTime { get; set; }
        public List<string> JobIds { get; set; }
    }
}