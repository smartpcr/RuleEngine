// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models
{
    using System;
    using Common.DocDb;
    using Newtonsoft.Json;

    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid().ToString("D");
            TS = DateTime.UtcNow; // this will always be overwritten by docdb
        }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty(PropertyName = "_ts")]
        [JsonConverter(typeof(UnixEpochTimeConverter))]
        public DateTime TS { get; set; }
    }
}