// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceLocation.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Location
{
    using DataCenterHealth.Models;
    using Newtonsoft.Json;

    [TrackChange(true)]
    public class DeviceLocation
    {
        [JsonProperty("id")]
        public string DeviceName { get; set; }
        public string DcName { get; set; }
        public long DcCode { get; set; }
        public string DeviceId { get; set; }
        public string ColoName { get; set; }
        public string DeviceType { get; set; }
        public string[] Racks { get; set; }
    }
}
