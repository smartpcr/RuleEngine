// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System;

    public abstract class TrackableEntity : BaseEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreationTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModificationTime { get; set; }
    }
}