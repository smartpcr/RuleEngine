// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities
{
    using System;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensorType
    {
        [Description("Geist Sensor")]
        WD15,
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
        [Description("Mechanical Onboarding")] Mechanical = 3,
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeviceType : long
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Breaker")]
        Breaker = 1,
        [Description("Transformer")]
        Transformer = 2,
        [Description("Switch")]
        Switch = 4,
        [Description("Generator")]
        Generator = 8,
        [Description("STS")]
        STS = 16,
        [Description("UPS")]
        UPS = 32,
        [Description("AHU")]
        AHU = 64,
        [Description("ATS")]
        ATS = 128,
        [Description("Busbar")]
        Busbar = 256,
        [Description("DistributionPanel")]
        DistributionPanel = 512,
        [Description("Panel")]
        Panel = 1024,
        [Description("PowerMeter")]
        PowerMeter = 2048,
        [Description("RPP")]
        RPP = 4096,
        [Description("PDU")]
        PDU = 8192,
        [Description("AC/Heat")]
        Heat = 16384,
        [Description("Condenser")]
        Condenser = 32768,
        [Description("DOAS")]
        DOAS = 65536,
        [Description("Filter")]
        Filter = 131072,
        [Description("Heater")]
        Heater = 262144,
        [Description("Humidifier")]
        Humidifier = 524288,
        [Description("LoadBank")]
        LoadBank = 1048576,
        [Description("Pump")]
        Pump = 2097152,
        [Description("SurgeProtectiveDevice")]
        SurgeProtectiveDevice = 4194304,
        [Description("VFD")]
        VFD = 8388608,
        [Description("HRG")]
        HRG = 16777216,
        [Description("Feed")]
        Feed = 33554432,
        [Description("Zenon")]
        Zenon = 67108864,
        [Description("End")]
        End = 134217728,
        [Description("Busway")]
        Busway = 268435456,
        [Description("TieBreaker")]
        TieBreaker = 536870912,
        [Description("GenBreaker")]
        GenBreaker = 1073741824,
        [Description("BMS")]
        BMS = 2147483648,
        [Description("FuelPolisher")]
        FuelPolisher = 4294967296,
        [Description("FuelFill")]
        FuelFill = 8589934592,
        [Description("ParallelPanel")]
        ParallelPanel = 17179869184
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BreakerType
    {
        [Description("MainBreaker")]
        InputBreaker,
        [Description("FeedBreaker")]
        OutputBreaker
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(Missing)]
    public enum State
    {
        [Description("Missing")]
        Missing,
        [Description("NotApplicable")]
        NotApplicable,
        [Description("NormallyClosed")]
        NormallyClosed,
        [Description("NormallyOpen")]
        NormallyOpen,
        [Description("Source1")]
        Source1,
        [Description("Source2")]
        Source2,
        [Description("Spare")]
        Spare,
        [Description("StandBy")]
        StandBy
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(Primary)]
    public enum AssociationType
    {
        [Description("PowerSource")]
        PowerSource = 0,
        [Description("Primary")]
        Primary = 1,
        [Description("Backup")]
        Backup = 2,
        [Description("Maintenance")]
        Maintenance = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(None)]
    public enum CommunicationProtocol
    {
        None = 0,
        [Description("Modbus")]
        Modbus = 1,
        [Description("BACnet")]
        BACnet = 2
    }

    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum Tag
    {
        None = 0,
        [Description("Azure Flex")]
        Flex = 1,
        [Description("PUE")]
        PUE = 2,
        [Description("IDF")]
        IDF = 4
    }
}
