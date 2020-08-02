namespace DataCenterHealth.Entities.Devices
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using DataCenterHealth.Entities.Parsers;
    using DataCenterHealth.Models.Sync;
    using Models;
    using Newtonsoft.Json;

    [XmlRoot("Subject")]
    public class Subject
    {
        [XmlAttribute] public string ShortName { get; set; }
        [XmlAttribute] public int MainVersion { get; set; }
        [XmlElement("Apartment")] public Apartment[] Apartment { get; set; }
    }

    public class Apartment
    {
        [XmlAttribute] public string ShortName { get; set; }
        [XmlAttribute] public int Version { get; set; }

        [XmlElement("Variable")] public ZenonVariable[] Variable { get; set; }
        [XmlElement("Driver")] public ZenonDriver[] Driver { get; set; }
        [XmlElement("Type")] public ZenonType[] Type { get; set; }
    }

    public class ZenonType
    {
        [XmlAttribute] public int TypeID { get; set; }
        [XmlAttribute] public BoolString IsComplex { get; set; }
        [XmlElement] public string Name { get; set; }
        [XmlAttribute] public string Matrix { get; set; }

        public string Description { get; set; }
        public string ExternalReference { get; set; }
        public string SystemModelGroup { get; set; }
        public BoolString Invisible { get; set; }
        public BoolString Hidden { get; set; }
        public BoolString ComplexTyp { get; set; }
        public BoolString InternalType { get; set; }

        #region simple

        public byte Datatyp { get; set; }
        public string Tagname { get; set; }
        public string Unit { get; set; }
        public decimal AlternateValue { get; set; }
        public string AlternateValueString { get; set; }
        public string Recourceslabel { get; set; }
        public BoolString IsRemaActiv { get; set; }
        public string AlarmQuitPV { get; set; }
        public string AlarmViewQuitPV { get; set; }
        public byte AlarmQuitPVValue { get; set; }
        public BoolString VarInASM { get; set; }
        public BoolString AlarmViaEquipmentModel { get; set; }
        public string AreaName { get; set; }
        public string AreaName2 { get; set; }
        public string AreaName3 { get; set; }
        public string AreaName4 { get; set; }
        public byte Digits { get; set; }
        public decimal HystNeg { get; set; }
        public decimal HystPos { get; set; }
        public decimal ArchHystNeg { get; set; }
        public decimal ArchHystPos { get; set; }
        public decimal SignalMin { get; set; }
        public decimal SignalMax { get; set; }
        public decimal RangeMin { get; set; }
        public decimal RangeMax { get; set; }
        public BoolString UseMacro { get; set; }
        public string AdjustHardware { get; set; }
        public string AdjustZenon { get; set; }
        public BoolString DDEActive { get; set; }
        public byte ArraySizeOld { get; set; }
        public byte CounterGroup { get; set; }
        public byte MaxGradient { get; set; }
        public BoolString NormalStateActive { get; set; }
        public BoolString NormalState { get; set; }
        public string AlarmPV0 { get; set; }
        public string AlarmPV1 { get; set; }
        public string AlarmPV2 { get; set; }
        public BoolString HDActive { get; set; }
        public decimal HDUpdate { get; set; }
        public int HDSize { get; set; }
        public BoolString IsKDAActiv { get; set; }
        public BoolString InOut { get; set; }
        public BoolString SBO { get; set; }
        public BoolString CancelOperate { get; set; }
        public decimal ValueMin { get; set; }
        public decimal ValueMax { get; set; }
        public string LockingName { get; set; }
        public byte SetValueProtocol { get; set; }
        public BoolString SV_Act { get; set; }
        public BoolString SV_VBA { get; set; }
        public byte MaxStringLen { get; set; }
        public byte UpdatePriority { get; set; }
        public BoolString Standby { get; set; }
        public BoolString Used_in_ProcRec { get; set; }
        public string InternalTyp { get; set; }

        #endregion

        #region complex

        public ZenonTypeItem Items_0 { get; set; }
        public ZenonTypeItem Items_1 { get; set; }
        public ZenonTypeItem Items_2 { get; set; }
        public ZenonTypeItem Items_3 { get; set; }
        public ZenonTypeItem Items_4 { get; set; }
        public ZenonTypeItem Items_5 { get; set; }
        public ZenonTypeItem Items_6 { get; set; }
        public ZenonTypeItem Items_7 { get; set; }
        public ZenonTypeItem Items_8 { get; set; }
        public ZenonTypeItem Items_9 { get; set; }
        public ZenonTypeItem Items_10 { get; set; }
        public ZenonTypeItem Items_11 { get; set; }
        public ZenonTypeItem Items_12 { get; set; }
        public ZenonTypeItem Items_13 { get; set; }
        public ZenonTypeItem Items_14 { get; set; }
        public ZenonTypeItem Items_15 { get; set; }
        public ZenonTypeItem Items_16 { get; set; }
        public ZenonTypeItem Items_17 { get; set; }
        public ZenonTypeItem Items_18 { get; set; }
        public ZenonTypeItem Items_19 { get; set; }
        public ZenonTypeItem Items_20 { get; set; }
        public ZenonTypeItem Items_21 { get; set; }
        public ZenonTypeItem Items_22 { get; set; }
        public ZenonTypeItem Items_23 { get; set; }
        public ZenonTypeItem Items_24 { get; set; }
        public ZenonTypeItem Items_25 { get; set; }
        public ZenonTypeItem Items_26 { get; set; }
        public ZenonTypeItem Items_27 { get; set; }
        public ZenonTypeItem Items_28 { get; set; }
        public ZenonTypeItem Items_29 { get; set; }
        public ZenonTypeItem Items_30 { get; set; }
        public ZenonTypeItem Items_31 { get; set; }
        public ZenonTypeItem Items_32 { get; set; }
        public ZenonTypeItem Items_33 { get; set; }
        public ZenonTypeItem Items_34 { get; set; }
        public ZenonTypeItem Items_35 { get; set; }
        public ZenonTypeItem Items_36 { get; set; }
        public ZenonTypeItem Items_37 { get; set; }
        public ZenonTypeItem Items_38 { get; set; }
        public ZenonTypeItem Items_39 { get; set; }
        public ZenonTypeItem Items_40 { get; set; }
        public ZenonTypeItem Items_41 { get; set; }
        public ZenonTypeItem Items_42 { get; set; }
        public ZenonTypeItem Items_43 { get; set; }
        public ZenonTypeItem Items_44 { get; set; }
        public ZenonTypeItem Items_45 { get; set; }
        public ZenonTypeItem Items_46 { get; set; }
        public ZenonTypeItem Items_47 { get; set; }
        public ZenonTypeItem Items_48 { get; set; }
        public ZenonTypeItem Items_49 { get; set; }
        #endregion

        #region simple

        public TypeLimit Limits_0 { get; set; }

        public TypeLimit Limits_1 { get; set; }

        #endregion

        public List<ZenonTypeItem> GetItems()
        {
            var items = new List<ZenonTypeItem>();
            if (Items_1 != null) items.Add(Items_1);
            if (Items_2 != null) items.Add(Items_2);
            if (Items_3 != null) items.Add(Items_3);
            if (Items_4 != null) items.Add(Items_4);
            if (Items_5 != null) items.Add(Items_5);
            if (Items_6 != null) items.Add(Items_6);
            if (Items_7 != null) items.Add(Items_7);
            if (Items_8 != null) items.Add(Items_8);
            if (Items_9 != null) items.Add(Items_9);
            if (Items_10 != null) items.Add(Items_10);
            if (Items_11 != null) items.Add(Items_11);
            if (Items_12 != null) items.Add(Items_12);
            if (Items_13 != null) items.Add(Items_13);
            if (Items_14 != null) items.Add(Items_14);
            if (Items_15 != null) items.Add(Items_15);
            if (Items_16 != null) items.Add(Items_16);
            if (Items_17 != null) items.Add(Items_17);
            if (Items_18 != null) items.Add(Items_18);
            if (Items_19 != null) items.Add(Items_19);
            if (Items_20 != null) items.Add(Items_20);
            if (Items_21 != null) items.Add(Items_21);
            if (Items_22 != null) items.Add(Items_22);
            if (Items_23 != null) items.Add(Items_23);
            if (Items_24 != null) items.Add(Items_24);
            if (Items_25 != null) items.Add(Items_25);
            if (Items_26 != null) items.Add(Items_26);
            if (Items_27 != null) items.Add(Items_27);
            if (Items_28 != null) items.Add(Items_28);
            if (Items_29 != null) items.Add(Items_29);
            if (Items_30 != null) items.Add(Items_30);
            if (Items_31 != null) items.Add(Items_31);
            if (Items_32 != null) items.Add(Items_32);
            if (Items_33 != null) items.Add(Items_33);
            if (Items_34 != null) items.Add(Items_34);
            if (Items_35 != null) items.Add(Items_35);
            if (Items_36 != null) items.Add(Items_36);
            if (Items_37 != null) items.Add(Items_37);
            if (Items_38 != null) items.Add(Items_38);
            if (Items_39 != null) items.Add(Items_39);
            if (Items_40 != null) items.Add(Items_40);
            if (Items_41 != null) items.Add(Items_41);
            if (Items_42 != null) items.Add(Items_42);
            if (Items_43 != null) items.Add(Items_43);
            if (Items_44 != null) items.Add(Items_44);
            if (Items_45 != null) items.Add(Items_45);
            if (Items_46 != null) items.Add(Items_46);
            if (Items_47 != null) items.Add(Items_47);
            if (Items_48 != null) items.Add(Items_48);
            if (Items_49 != null) items.Add(Items_49);

            return items;
        }
    }

    public class ZenonTypeItem
    {
        [XmlAttribute("NODE")] public string NODE { get; set; }
        [XmlAttribute("TYPE")] public string TYPE { get; set; }
        [XmlElement] public string Name { get; set; }

        public int TypeID { get; set; }
        public int Offset { get; set; }
        public int BitOffset { get; set; }
        public int ID_DataTyp { get; set; }
        public byte LBound { get; set; }
        public int Dim1 { get; set; }
        public int Dim2 { get; set; }
        public int Dim3 { get; set; }
        public byte ItemIndex { get; set; }
        public BoolString HasRef { get; set; }
        public string Description { get; set; }
        public string ExternalReference { get; set; }
    }

    public class TypeLimit
    {
        [XmlAttribute] public string NODE { get; set; }

        public BoolString Active { get; set; }
        public string Text { get; set; }
        public decimal LimitValue { get; set; }
        public BoolString IsMax { get; set; }
        public decimal ThresholdValue { get; set; }
        public byte Delay { get; set; }
        public BoolString IsVariable { get; set; }
        public string Variable { get; set; }
        public string Function { get; set; }
        public BoolString AML_Call { get; set; }
        public string Color { get; set; }
        public BoolString Invisible { get; set; }
        public BoolString Blinking { get; set; }
        public string UserProperty1 { get; set; }
        public string UserProperty2 { get; set; }
        public BoolString Alarm { get; set; }
        public BoolString Cel { get; set; }
        public BoolString AlarmAcknowledge { get; set; }
        public BoolString AlarmComment { get; set; }
        public BoolString AlarmCause { get; set; }
        public BoolString AlarmDelete { get; set; }
        public BoolString Print { get; set; }
        public string GroupName { get; set; }
        public string ClassName { get; set; }
        public string HelpFile { get; set; }
        public string HelpCapture { get; set; }
    }

    public class ZenonVariable
    {
        [XmlAttribute] public string ShortName { get; set; }
        [XmlAttribute] public string DriverID { get; set; }
        [XmlAttribute] public int TypeID { get; set; }
        [XmlAttribute] public byte HWObjectType { get; set; }
        [XmlAttribute] public string HWObjectName { get; set; }
        [XmlAttribute] public BoolString IsComplex { get; set; }
        [XmlAttribute] public string Matrix { get; set; }
        public int ID_Complex { get; set; }
        public int ID_ComplexVariable { get; set; }
        public string Name { get; set; }
        public string Tagname { get; set; }
        public string ExternalReference { get; set; }
        public string SystemModelGroup { get; set; }
        public string NetAddr { get; set; }
        public byte DataBlock { get; set; }
        public int Offset { get; set; }
        public byte BitAddr { get; set; }
        public byte Alignment { get; set; }
        public byte StringLength { get; set; }
        public string SymbAddr { get; set; }
        public BoolString IsOffsetManuell { get; set; }
        public BoolString OfsAccordingType { get; set; }
        public BoolString IsStartAtNewOffset { get; set; }
        public int ID_DriverTyp { get; set; }
        public byte LBound { get; set; }
        public int Dim1 { get; set; }
        public int Dim2 { get; set; }
        public int Dim3 { get; set; }
        public BoolString ExternVisible { get; set; }
        public string ExternVisibleFor { get; set; }
        public BoolString ReadWrite { get; set; }
        public string InitialValue { get; set; }
        public string Profilename { get; set; }
        public string Adressparam { get; set; }
        public string Vargroup { get; set; }
        public string LockingName { get; set; }
        public string VisualName { get; set; }
        public string Meaning { get; set; }
        public string WaterfallParam { get; set; }
        public BoolString IsLocalDigits { get; set; }
        public BoolString IsLocalSignalMin { get; set; }
        public BoolString IsLocalSignalMax { get; set; }
        public BoolString IsLocalRangeMin { get; set; }
        public BoolString IsLocalRangeMax { get; set; }
        public BoolString IsLocalAlternateValue { get; set; }
        public BoolString IsLocalValueMin { get; set; }
        public BoolString IsLocalValueMax { get; set; }
        public BoolString IsLocalUseMacro { get; set; }
        public BoolString IsLocalInOut { get; set; }
        public BoolString IsLocalHystNeg { get; set; }
        public BoolString IsLocalHystPos { get; set; }
        public BoolString IsLocalArchHystNeg { get; set; }
        public BoolString IsLocalArchHystPos { get; set; }
        public BoolString IsLocalUpdatePriority { get; set; }
        public BoolString IsLocalDDEaktiv { get; set; }
        public BoolString IsLocalStandby { get; set; }
        public BoolString IsLocalRemaActiv { get; set; }
        public BoolString IsLocalRema { get; set; }
        public BoolString IsLocalHDActive { get; set; }
        public BoolString IsLocalHDUpdate { get; set; }
        public BoolString IsLocalHDSize { get; set; }
        public BoolString IsLocalKDAActiv { get; set; }
        public BoolString IsLocalStingLength { get; set; }
        public BoolString IsLocalQuitPV { get; set; }
        public BoolString IsLocalViewQuitPV { get; set; }
        public BoolString IsLocalQuitValue { get; set; }
        public BoolString IsLocalTagname { get; set; }
        public BoolString IsLocalUnit { get; set; }
        public BoolString IsLocalAltValString { get; set; }
        public BoolString IsLocalRecLabel { get; set; }
        public BoolString IsLocalAdjustHW { get; set; }
        public BoolString IsLocalAdjustZenon { get; set; }
        public BoolString IsLocalArraySize { get; set; }
        public BoolString IsLocalCounterGroup { get; set; }
        public BoolString IsLocalMaxGradient { get; set; }
        public BoolString IsLocalNormalStateActive { get; set; }
        public BoolString IsLocalNormalState { get; set; }
        public BoolString IsLocalAlarmPV0 { get; set; }
        public BoolString IsLocalAlarmPV1 { get; set; }
        public BoolString IsLocalAlarmPV2 { get; set; }
        public BoolString IsLocalVarInASM { get; set; }
        public BoolString IsLocalAlarmDomain { get; set; }
        public BoolString IsLocalAlarmDomain2 { get; set; }
        public BoolString IsLocalAlarmDomain3 { get; set; }
        public BoolString IsLocalAlarmDomain4 { get; set; }
        public BoolString IsLocalLocking { get; set; }
        public BoolString IsLocalSBO { get; set; }
        public BoolString IsLocalCancelOperate { get; set; }
        public BoolString IsLocalAlarmViaEquipmentModel { get; set; }
        public BoolString IsLocalUsedInProcRec { get; set; }
        public byte IsSWProtokol { get; set; }
        public BoolString IsSW_Akt { get; set; }
        public BoolString IsSW_VBA { get; set; }

        public VariableLimit Limits_0 { get; set; }

        public VariableLimit Limits_1 { get; set; }
    }

    public class VariableLimit
    {
        [XmlAttribute] public string NODE { get; set; }

        public BoolString FlagActiv { get; set; }
        public BoolString FlagAlarm { get; set; }
        public BoolString FlagDelay { get; set; }
        public BoolString FlagAColor { get; set; }
        public BoolString FlagGroup { get; set; }
        public BoolString FlagClassID { get; set; }
        public BoolString FlagMessage { get; set; }
        public BoolString FlagIsAlarm { get; set; }
        public BoolString FlagTreshold { get; set; }
        public BoolString FlagToDelete { get; set; }
        public BoolString FlagToCEL { get; set; }
        public BoolString FlagInvisible { get; set; }
        public BoolString FlagLimitIsMax { get; set; }
        public BoolString FlagLimitVar { get; set; }
        public BoolString FlagAQuit { get; set; }
        public BoolString FlagBlinking { get; set; }
        public BoolString FlagAlarmPrt { get; set; }
        public BoolString FlagFunction { get; set; }
        public BoolString FlagQuest { get; set; }
        public BoolString FlagLimitVariable { get; set; }
        public BoolString FlagHelpFile { get; set; }
        public BoolString FlagHelpCapture { get; set; }
        public BoolString FlagUser1 { get; set; }
        public BoolString FlagUser2 { get; set; }
        public BoolString FlagAComment { get; set; }
        public BoolString FlagACause { get; set; }
    }

    public class ZenonDriver
    {
        [XmlAttribute]
        public int DriverID { get; set; }

        public string Name { get; set; }
        public string Modul { get; set; }
    }

    public enum BoolString
    {
        [XmlEnum("FALSE")] FALSE,
        [XmlEnum("TRUE")] TRUE
    }

    [BlobReader(typeof(DataTypeBlobParser), "mciocihstoragecusstg", "datatype", "cih-storage-cus-stg")]
    [CosmosWriter("xd-dev", "metadata", "zenon_datatype", "xd-dev-authkey", "dataTypeFileName", "fileDataPoint")]
    [TrackChange(true)]
    public class ZenonDataType: BaseEntity
    {
        public string FileDataPoint { get; set; }
        public string DataTypeFileName { get; set; }
        public string ChannelTypeName { get; set; }
        public int ChannelTypeId { get; set; }
        public string ChannelName { get; set; }
        public int ChannelId { get; set; }
        public int Offset { get; set; }
        public string DataPoint { get; set; }
        public string Primitive { get; set; }
        public int BitOffset { get; set; }
        public byte ItemIndex { get; set; }
        public byte DataType { get; set; }
        public decimal RangeMin { get; set; }
        public decimal RangeMax { get; set; }
        public byte Digits { get; set; }
        public byte UpdatePriority { get; set; }
    }

}