// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackExecutionAttribute.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System;
    using DataCenterHealth.Models.Summaries;

    [AttributeUsage(AttributeTargets.Class)]
    public class TrackExecutionAttribute : Attribute
    {
        public bool Enabled { get; set; }
        public ExecutionType Type { get; set; }
        public string TitlePropName { get; set; }

        public TrackExecutionAttribute(bool isEnabled, ExecutionType type, string titlePropName = "Name")
        {
            Enabled = isEnabled;
            Type = type;
            TitlePropName = titlePropName;
        }
    }
}