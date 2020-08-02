using DataCenterHealth.Models;

namespace DataCenterHealth.Entities.DataType
{
    using System.Xml.Serialization;
    using DataCenterHealth.Entities.Devices;
    using DataCenterHealth.Entities.Parsers;

    [XmlRoot("root")]
    public class ZenonDriverConfigRoot
    {
        [XmlElement("driverType")] public ZenonDriverType DriverType { get; set; }
    }

    public class ZenonDriverType
    {
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute] public string ConfiguredObjectType { get; set; }
        [XmlElement("general")] public ZenonDriverGeneral General { get; set; }
        [XmlElement("settings")] public ZenonDriverSettings Settings { get; set; }
    }

    public class ZenonDriverGeneral
    {
        public byte GenDriverMode { get; set; }
        public BoolString KeepUpdateListInMemory { get; set; }
        public BoolString VariableImageReminent { get; set; }
        public BoolString StoppedOnStandbyServer { get; set; }
        public BoolString GenUseGlobalUpdateTime { get; set; }
        public int GenGlobalUpdateTime { get; set; }
        public int PriorityTimesNormal { get; set; }
        public int PriorityTimesHigh { get; set; }
        public int PriorityTimesHigher { get; set; }
        public int PriorityTimesHighest { get; set; }
    }

    public class ZenonDriverSettings
    {
        public int MaximumBlockSize { get; set; }
        public BoolString Offset1 { get; set; }
        public BoolString JoinConnections { get; set; }
        public byte STRINGByteOrder { get; set; }
        public byte FLOATByteOrder { get; set; }
        public byte DWORDByteOrder { get; set; }
        public int CommunicationTimeout { get; set; }
        public byte CommunicationRetries { get; set; }
        public int ReConnectTimeout { get; set; }
        public string FileTransferDirectory { get; set; }
    }

    [BlobReader(typeof(DriverBlobParser), "mciocihstoragecusstg", "driver", "cih-storage-cus-stg")]
    [CosmosWriter("xd-dev", "metadata", "zenon_driver_config", "xd-dev-authkey", "configFileName", "configFileName")]
    [TrackChange(true)]
    public class ZenonDriverConfig : BaseEntity
    {
        public string ConfigFileName { get; set; }
        public int PriorityTimesNormal { get; set; }
        public int PriorityTimesHigh { get; set; }
        public int PriorityTimesHigher { get; set; }
        public int PriorityTimesHighest { get; set; }
        public int CommunicationTimeout { get; set; }
        public byte CommunicationRetries { get; set; }
        public int ReConnectTimeout { get; set; }
    }
}