// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rule.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    using System.Collections.Generic;
    using DataCenterHealth.Models.Devices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public abstract class Rule : TrackableEntity
    {
        public string Name { get; set; }
        public string RuleSetId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleType Type { get; set; }
        public string ContextType { get; set; } = typeof(PowerDevice).FullName;
        public decimal Weight { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleContext Context { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleState State { get; set; }
        public string Category { get; set; }
        public string Reason { get; set; }
        public string ReasonExtended { get; set; }
        public List<string> Owners { get; set; }
        public List<string> Contributors { get; set; }
        public string Description { get; set; }
    }

    public enum RuleState
    {
        Warning,
        Normal,
        Unhealthy
    }

    public enum RuleContext
    {
        Device,
        Channel,
        Configuration,
        Global,
        AzureSignal
    }
}