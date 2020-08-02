// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZenonDataPointConfig.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using DataCenterHealth.Models;

namespace DataCenterHealth.Entities.DataType
{
    using System.Xml.Serialization;
    using Parsers;

    [XmlRoot("Configuration")]
    public class ReactionMatrix
    {
        public string StopPgScriptContent { get; set; }
        public ApprovedDataPoints ApprovedDataPoints { get; set; }
        public SystemVariables SystemVariables { get; set; }
        public EventHubConnections EventHubConnections { get; set; }
        public ProjectBasedConfigurations ProjectBasedConfigurations { get; set; }
    }

    public class ApprovedDataPoints
    {
        [XmlElement("DataPoints")] public DataPoints[] DataPoints { get; set; }
    }

    public class DataPoints
    {
        [XmlAttribute] public string Type { get; set; }
        [XmlAttribute] public string Attributes { get; set; }
        [XmlAttribute] public int SplitLimit { get; set; }
        [XmlAttribute] public string Version { get; set; }
    }

    public class SystemVariables
    {
        [XmlElement("Variable")] public SystemVariable[] Variable { get; set; }
    }

    public class SystemVariable
    {
        [XmlAttribute] public string Name { get; set; }
    }

    public class EventHubConnections
    {
        [XmlElement("Connection")] public Connection[] Connection { get; set; }
    }

    public class Connection
    {
        [XmlAttribute] public string Name { get; set; }
        public string Conn { get; set; }
        public string Queue { get; set; }
        public byte Mode { get; set; }
        public byte Format { get; set; }
    }

    public class ProjectBasedConfigurations
    {
        [XmlElement("Project")] public Project[] Project { get; set; }
    }

    public class Project
    {
        [XmlAttribute] public string Name { get; set; }
        public ProjectConnections Connections { get; set; }
    }

    public class ProjectConnections
    {
        [XmlElement("Connection")] public ProjectConnection[] Connection { get; set; }
    }

    public class ProjectConnection
    {
        [XmlAttribute] public string Name { get; set; }
    }

    [BlobReader(typeof(DataPointConfigBlobParser), "mciocihstoragecusstg", "reactionmatrix", "cih-storage-cus-stg")]
    [CosmosWriter("xd-dev", "metadata", "zenon_datapoint_config", "xd-dev-authkey", "", "dataPoint")]
    [TrackChange(true)]
    public class ZenonDataPointConfig : BaseEntity
    {
        public string DataPoint { get; set; }
        public string Type { get; set; }
        public string ChannelType { get; set; }
        public string Channel { get; set; }
        public string Version { get; set; }
    }
}