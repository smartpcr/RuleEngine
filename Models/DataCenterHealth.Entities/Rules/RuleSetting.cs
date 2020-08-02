// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleSetting.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Rules
{
    using DataCenterHealth.Models;
    using Newtonsoft.Json.Linq;

    [CosmosReader("power-reference-prod", "power-reference-db", "RuleSettings", "power-reference-prod-authkey", "Key")]
    [CosmosWriter("xd-dev", "metadata", "argus_rule", "xd-dev-authkey", "key", "key")]
    public class RuleSetting : BaseEntity
    {
        public string Key { get; set; }
        public JToken StringValue { get; set; }
        public int Version { get; set; }
    }
}