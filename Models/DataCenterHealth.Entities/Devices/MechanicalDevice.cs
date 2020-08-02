// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MechanicalDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Devices
{
    using DataCenterHealth.Models;

    [CosmosReader("power-reference-prod", "power-reference-db", "MechanicalDevice", "power-reference-prod-authkey", "Name")]
    [CosmosWriter("xd-dev", "metadata", "mechanical_device", "xd-dev-authkey", "name", "name")]
    [TrackChange(true)]
    public class MechanicalDevice : BaseEntity
    {
        public string Name { get; set; }
        public string DcName { get; set; }
        public long DcCode { get; set; }
        public string Designation { get; set; }
        public string Description { get; set; }
        public string DeviceType { get; set; }
        public string ImpactArea { get; set; }
        public string ColoName { get; set; }
        public string GridLocation { get; set; }
    }
}