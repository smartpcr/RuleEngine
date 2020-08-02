// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DcRank.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Physical
{
    using DataCenterHealth.Models;
    using Newtonsoft.Json;

    [KustoReader("Mciocihprod", "MCIOCIHArgusProd", "DCHierarchy2", new []{"DataCenterCode", "Rack"})]
    [CosmosWriter("xd-dev", "metadata", "layout", "xd-dev-authkey", "rack", "rack")]
    [TrackChange(true)]
    public class DcRack
    {
        [JsonProperty("dcName")]
        public string DataCenterCode { get; set; }

        public string Colocation { get; set; }
        public string Row { get; set; }
        public string Rack { get; set; }
        public string Node { get; set; }
        public string NodeAssetTag { get; set; }
    }
}
