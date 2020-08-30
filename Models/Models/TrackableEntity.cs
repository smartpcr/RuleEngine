// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackableEntity.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models
{
    using System;

    public abstract class TrackableEntity : BaseEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreationTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModificationTime { get; set; }

        protected TrackableEntity()
        {
            CreatedBy = Environment.UserName;
            ModifiedBy = Environment.UserName;
            ModificationTime = DateTime.UtcNow;
            CreationTime = DateTime.UtcNow;
        }
    }
}