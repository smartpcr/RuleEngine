// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationJob.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;

    public class DeviceValidationJob : BaseEntity
    {
        public string ScheduleName { get; set; }
        public string DcName { get; set; }

        /// <summary>
        ///     when set, only selected devices is produced
        /// </summary>
        public List<string> DeviceNames { get; set; }

        /// <summary>
        ///     either RuleId or RuleSetId, but not both can be set
        /// </summary>
        public List<string> RuleSetIds { get; set; }

        /// <summary>
        ///     either RuleId or RuleSetId, but not both can be set
        /// </summary>
        public List<string> RuleIds { get; set; }

        public DateTime SubmissionTime { get; set; }
        public string SubmittedBy { get; set; }

        /// <summary>
        ///     correlation id between producer and consumer
        /// </summary>
        public string ActivityId { get; set; }
    }

    public class DeviceValidationJobWithRuns
    {
        public DeviceValidationJob Job { get; set; }
        public List<DeviceValidationRun> Runs { get; set; }
    }
}