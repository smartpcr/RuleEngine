// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Alert.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System;

    public class Alert : TrackableEntity
    {
        public AlertType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ResolvedAt { get; set; }
        public string AssignedTo { get; set; }
        public string ResolvedBy { get; set; }
    }

    public enum AlertType
    {
        Success,
        Info,
        Warning,
        Error
    }
}