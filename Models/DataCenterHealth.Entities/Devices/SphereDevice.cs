// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SphereDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Devices
{
    using DataCenterHealth.Models;
    using OpenTelemetry.Trace;

    [CosmosReader("power-reference-prod", "power-reference-db", "SphereDevice", "power-reference-prod-authkey", "deviceId")]
    [CosmosWriter("xd-dev", "metadata", "sphere_device", "xd-dev-authkey", "deviceId", "deviceId")]
    [TrackChange(true)]
    public class SphereDevice : BaseEntity
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string SourceId { get; set; }
        public string Group { get; set; }
        public link Uplink { get; set; }
        public Link Downlink { get; set; }
    }

    public class link
    {
        public string Interface { get; set; }
        public string Data { get; set; }
    }
}