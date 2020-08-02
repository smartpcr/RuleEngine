// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SchedulerSetting.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Jobs
{
    using System;

    public class SchedulerSettings
    {
        public string CheckFrequency { get; set; }
        public bool CompensateMissedSchedules { get; set; }
        public TimeSpan SleepInterval { get; set; } = TimeSpan.FromMinutes(5);
    }
}