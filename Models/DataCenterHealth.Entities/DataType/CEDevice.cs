// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CEDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.DataType
{
    using System.Collections.Generic;
    using DataCenterHealth.Models;

    [CosmosReader("power-reference-prod", "power-reference-db", "CeDevice", "power-reference-prod-authkey", "name")]
    [CosmosWriter("xd-dev", "metadata", "ce_device", "xd-dev-authkey", "name", "name")]
    [TrackChange(true)]
    public class CEDevice : BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DataSchemaId { get; set; }
        public int SchemaOffset { get; set; }
        public string Room { get; set; }
        public string Position { get; set; }
        public int UnitId { get; set; }
        public string SphereName { get; set; }
        public int PollIntervalMs { get; set; }
        public int PollTimeoutMs { get; set; }
        public string Protocol { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public int BaudRate { get; set; }
        public DeviceSpec Specs { get; set; }
    }

    public class DeviceSpec
    {
        public List<string> PrimaryParentDeviceIds { get; set; }
        public List<string> SecondaryParentDeviceIds { get; set; }
        public List<string> MaintenanceParentDeviceIds { get; set; }
        public string NormalState { get; set; }
        public string Hierarchy { get; set; }
        public int AmpRating { get; set; }
        public int VoltageRating { get; set; }
        public int DeployedCpacity { get; set; }
        public int DeratedCapacity { get; set; }
        public int DeratingFactor { get; set; }
        public int PowerFactor { get; set; }
        public int RatedCapacity { get; set; }
        public int ReservedCapacity { get; set; }
    }
}