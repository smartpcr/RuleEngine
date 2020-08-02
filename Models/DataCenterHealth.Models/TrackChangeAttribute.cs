// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackChangeAttribute.cs" company="Microsoft Corporation">
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
    public class TrackChangeAttribute : Attribute
    {
        public bool Enabled { get; set; }
        public ChangeType Type { get; set; }
        public string TitlePropName { get; set; }

        public TrackChangeAttribute(bool isEnabled, ChangeType type = ChangeType.MetaData, string titlePropName = "Name")
        {
            Enabled = isEnabled;
            Type = type;
            TitlePropName = titlePropName;
        }
    }
}