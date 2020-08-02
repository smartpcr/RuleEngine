// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationSchedule.cs" company="Microsoft Corporation">
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

    [TrackChange(true, ChangeType.ValidationSchedule)]
    public class DeviceValidationSchedule : TrackableEntity
    {
        public string Name { get; set; }
        public string Frequency { get; set; }
        public List<string> RuleSetNames { get; set; }
        public List<string> DcNames { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastRunTime { get; set; }
    }
}