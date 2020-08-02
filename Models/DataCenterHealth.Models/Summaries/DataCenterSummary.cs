// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenterSummary.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Summaries
{
    using System.Collections.Generic;

    public class DataCenterSummary : BaseEntity
    {
        public string DcName { get; set; }
        public int TotalDevices { get; set; }
        public double ValidationCoverage { get; set; }
        public int ValidationRulesApplied { get; set; }
        public int ValidationRulesPassed { get; set; }
        public double LastScore { get; set; }
        public List<NameCountPair> DevicesByType { get; set; }
        public List<NameCountPair> DevicesByHierarchy { get; set; }
        public List<NameCountPair> FailuresByRuleSet { get; set; }
        public List<NameCountPair> IssuesByRule { get; set; }
        public List<NameCountPair> IssuesByDeviceType { get; set; }
        public List<NameCountPair> IssuesByHierarchy { get; set; }
        public List<NameValuePair> ValidationScores { get; set; }
    }

    public class NameCountPair
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public NameCountPair()
        {
        }

        public NameCountPair(string name, int count)
        {
            Name = name;
            Value = count;
        }
    }
}