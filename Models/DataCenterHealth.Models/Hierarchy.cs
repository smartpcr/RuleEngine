// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Hierarchy.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using Newtonsoft.Json;

    public class Hierarchy
    {
        [JsonProperty("DcName")] public string DataCenterCode { get; set; }

        public string Colocation { get; set; }
        public string Row { get; set; }
        public string Rack { get; set; }
        public string Node { get; set; }
        public string NodeAssetTag { get; set; }
    }
}