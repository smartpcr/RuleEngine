// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenterValidationTrend.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Summaries
{
    using System;
    using System.Collections.Generic;

    public class DataCenterValidationTrend : BaseEntity
    {
        public string DcName { get; set; }
        public string RuleSetName { get; set; }
        public double LastScore { get; set; }
        public double Change { get; set; }
        public List<NameValuePair> ScoresByDates { get; set; }
    }

    public class NameValuePair
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public NameValuePair()
        {
        }

        public NameValuePair(string name, double value)
        {
            Name = name;
            Value = value;
        }
    }
}