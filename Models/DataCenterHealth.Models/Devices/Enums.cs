// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.Json.Serialization;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensorType
    {
        [Description("Geist Sensor")] WD15,

        [Description("X-Archive Geist Sensor")]
        WD1200
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensorVersion
    {
        GT3HD,
        GTHD
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensorLocation
    {
        InDoor,
        OutDoor
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensorPosition
    {
        WatchDog,
        Left,
        Right,
        Front,
        Rear
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(IC)]
    public enum SiteType
    {
        IC,
        NonIC
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(Hierarchy)]
    public enum OnboardingMode
    {
        [Description("Hierarchical Onboarding")]
        Hierarchy = 0,
        [Description("Zenon Onboarding")] Zenon = 1,
        [Description("Thermal Onboarding")] Thermal = 2,
        [Description("Mechanical Onboarding")] Mechanical = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeviceType
    {
        Unknown,
        Breaker,
        Transformer,
        Switch,
        Generator,
        STS,
        UPS,
        AHU,
        ATS,
        Busbar,
        DistributionPanel,
        Panel,
        PowerMeter,
        RPP,
        PDU,
        Heat,
        Condenser,
        DOAS,
        Filter,
        Heater,
        Humidifier,
        LoadBank,
        Pump,
        SurgeProtectiveDevice,
        VFD,
        HRG,
        Feed,
        Zenon,
        End,
        Busway,
        TieBreaker,
        GenBreaker,
        BMS,
        BMS_JAR,
        BMS_STRING,
        FuelPolisher,
        FuelFill,
        ParallelPanel
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BreakerType
    {
        [Description("MainBreaker")] InputBreaker,
        [Description("FeedBreaker")] OutputBreaker
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(Missing)]
    public enum State
    {
        [Description("Missing")] Missing,
        [Description("NotApplicable")] NotApplicable,
        [Description("NormallyClosed")] NormallyClosed,
        [Description("NormallyOpen")] NormallyOpen,
        [Description("Source1")] Source1,
        [Description("Source2")] Source2,
        [Description("Spare")] Spare,
        [Description("StandBy")] StandBy
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(Primary)]
    public enum AssociationType
    {
        [Description("PowerSource")] PowerSource = 0,
        [Description("Primary")] Primary = 1,
        [Description("Backup")] Backup = 2,
        [Description("Maintenance")] Maintenance = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(None)]
    public enum CommunicationProtocol
    {
        None = 0,
        [Description("Modbus")] Modbus = 1,
        [Description("BACnet")] BACnet = 2
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum Tag
    {
        None = 0,
        [Description("Azure Flex")] Flex = 1,
        [Description("PUE")] PUE = 2,
        [Description("IDF")] IDF = 4
    }

    public static class DeviceHierarchies
    {
        public const string ATS = "ATS";
        public const string BUSBAR = "BUSBAR";
        public const string GEN = "GEN";
        public const string LVS_Colo = "LVS-Colo";
        public const string LVS_SubStation = "LVS-SubStation";
        public const string MSB = "MSB";
        public const string MVS_SubStation = "MVS-SubStation";
        public const string MechanicalDistribution = "MechanicalDistribution";
        public const string Misc_Others = "Misc-Others";
        public const string PDU = "PDU";
        public const string PDUInput = "PDUInput";
        public const string PDUOutput = "PDUOutput";
        public const string PDUR = "PDUR";
        public const string PDURInput = "PDURInput";
        public const string PDUROutput = "PDUROutput";
        public const string RPP = "RPP";
        public const string STS = "STS";
        public const string UDS = "UDS";
        public const string UPS = "UPS";
        public const string UTS_Campus = "UTS-Campus";
        public const string UTS_Facility = "UTS-Facility";
        public const string Unknown = "Unknown";

        public static List<string> AllHierarchies = new List<string>
        {
            ATS,
            BUSBAR,
            GEN,
            LVS_Colo,
            LVS_SubStation,
            MSB,
            MVS_SubStation,
            MechanicalDistribution,
            Misc_Others,
            PDU,
            PDUInput,
            PDUOutput,
            PDUR,
            PDURInput,
            PDUROutput,
            RPP,
            STS,
            UDS,
            UTS_Campus,
            UTS_Facility,
            Unknown
        };
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DevicePath
    {
        None = 0,
        Self = 1,
        Redundant = 2,
        Elecr = 3,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeviceFamily
    {
        None = 0,
        Self = 1,
        PowerSource = 2,
        Children = 3,
        Maintenance = 6
    }
}