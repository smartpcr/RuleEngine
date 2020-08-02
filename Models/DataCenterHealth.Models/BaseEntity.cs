// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityBase.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            TS = DateTime.UtcNow; // this will always be overwritten by docdb
        }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty(PropertyName = "_ts")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TS { get; set; }
    }
}