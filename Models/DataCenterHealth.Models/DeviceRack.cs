// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Allocation.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using Newtonsoft.Json;

    public class DeviceRack
    {
        [JsonProperty("id")] public string DeviceName { get; set; }

        public string DcName { get; set; }
        public long DcCode { get; set; }
        public string DeviceId { get; set; }
        public string ColoName { get; set; }
        public string DeviceType { get; set; }
        public string[] Racks { get; set; }
    }
}