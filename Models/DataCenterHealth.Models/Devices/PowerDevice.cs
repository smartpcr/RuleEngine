// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDevice.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using DataCenterHealth.Models.Validation;
    using Jobs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Rules;

    [TrackChange(true)]
    public class PowerDevice : Device, IRoutable
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public State DeviceState { get; set; }

        public string ColoName { get; set; }
        public long ColoId { get; set; }

        public decimal? Amperage { get; set; }
        public decimal? Voltage { get; set; }
        public decimal? RatedCapacity { get; set; }
        public decimal? DeratedCapacity { get; set; }
        public decimal? MaxItCapacity { get; set; }
        public decimal? ItCapacity { get; set; }
        public decimal? AmpRating { get; set; }
        public decimal? VoltageRating { get; set; }
        public decimal? KwRating { get; set; }
        public decimal? KvaRating { get; set; }
        public decimal? PowerFactor { get; set; }
        public decimal? DeRatingFactor { get; set; }
        public string PanelName { get; set; }

        [EnumDataType(typeof(CommunicationProtocol))]
        [JsonConverter(typeof(StringEnumConverter))]
        public CommunicationProtocol CopaConfigType { get; set; }

        public bool IsMonitorable { get; set; }

        #region relations
        public string PrimaryParent { get; set; }
        public string SecondaryParent { get; set; }
        public string MaintenanceParent { get; set; }
        public string RedundantDeviceNames { get; set; }
        #endregion

        #region datatype
        public string DataType { get; set; }
        public string ProjectName { get; set; }
        public string ConfiguredObjectType { get; set; }
        public string DriverName { get; set; }
        public string ConnectionName { get; set; }
        #endregion

        #region validation result (props only used in controller response)
        public decimal? AverageScore { get; set; }
        public List<DeviceValidationResult> ValidationResults { get; set; }
        public List<CodeRuleEvidence> ContextErrors { get; set; }
        #endregion

        #region enriched properties
        private void CloneEnrichment(PowerDevice soruce, PowerDevice target)
        {
            target.DirectDownstreamDeviceList = soruce.DirectDownstreamDeviceList;
            target.DirectUpstreamDeviceList = soruce.DirectUpstreamDeviceList;
            target.PrimaryParentDevice = soruce.PrimaryParentDevice;
            target.SecondaryParentDevice = soruce.SecondaryParentDevice;
            target.MaintenanceParentDevice = soruce.MaintenanceParentDevice;
            target.RedundantDevice = soruce.RedundantDevice;
            target.AllParents = soruce.AllParents;
            target.RootDevice = soruce.RootDevice;
            target.Children = soruce.Children;
            target.SiblingDevices = soruce.SiblingDevices;

            target.DataPoints = soruce.DataPoints;

            target.ReadingStats = soruce.ReadingStats;
            target.LastReadings = soruce.LastReadings;
            target.ConfiguredRanges = soruce.ConfiguredRanges;

            target.HierarchyId = soruce.HierarchyId;
            target.DevicePath = soruce.DevicePath;
            target.DeviceFamily = soruce.DeviceFamily;
            target.Validate = soruce.Validate;
        }

        #region relations
        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
        public PowerDevice PrimaryParentDevice { get; set; }
        public PowerDevice SecondaryParentDevice { get; set; }
        public PowerDevice MaintenanceParentDevice { get; set; }
        public PowerDevice RedundantDevice { get; set; }
        public List<PowerDevice> AllParents { get; set; }
        public PowerDevice RootDevice { get; set; }
        public List<PowerDevice> Children { get; set; }
        public List<PowerDevice> SiblingDevices { get; set; }
        public bool IsRedundantDevice { get; set; }
        #endregion

        #region data point
        public List<DataPoint> DataPoints { get; set; }
        #endregion

        #region readings
        public List<ZenonEventStats> ReadingStats { get; set; }
        public List<ZenonLastReading> LastReadings { get; set; }
        public List<AllowedRange> ConfiguredRanges { get; set; }
        #endregion

        #region hierarchy
        public double? HierarchyId { get; set; }
        public DevicePath? DevicePath { get; set; }
        public DeviceFamily? DeviceFamily { get; set; }
        public int? Validate { get; set; }
        #endregion
        #endregion

        #region evaluation
        [JsonIgnore]
        public EvaluationContext EvaluationContext { get; set; }
        [JsonIgnore]
        public EvaluationResult EvaluationResult { get; set; }

        public void AddEvaluationEvidence(DeviceValidationEvidence evidence)
        {
            EvaluationResult = EvaluationResult ?? new EvaluationResult();
            EvaluationResult.Evidences = EvaluationResult.Evidences ?? new List<DeviceValidationEvidence>();
            EvaluationResult.Evidences.Add(evidence);
        }
        #endregion

        #region IRoutable multicast in producer
        [JsonIgnore] public int RouteKey { get; set; }

        /// <summary>
        ///     avoid using serialization and reflection
        /// </summary>
        /// <returns></returns>
        public IRoutable Clone()
        {
            var clone = new PowerDevice
            {
                Id = Id,
                DeviceName = DeviceName,
                CreatedBy = CreatedBy,
                CreationTime = CreationTime,
                ModificationTime = ModificationTime,
                ModifiedBy = ModifiedBy,
                CopaConfigType = CopaConfigType,
                ContextErrors = ContextErrors,
                ConnectionName = ConnectionName,
                ConfiguredObjectType = ConfiguredObjectType,
                AllParents = AllParents,
                AmpRating = AmpRating,
                Amperage = Amperage,
                AverageScore = AverageScore,
                Children = Children,
                ColoId = ColoId,
                ColoName = ColoName,
                DeviceState = DeviceState,
                DataPoints = DataPoints,
                DataType = DataType,
                DeRatingFactor = DeRatingFactor,
                DeratedCapacity = DeratedCapacity,
                DriverName = DriverName,
                DcCode = DcCode,
                DcName = DcName,
                DeviceType = DeviceType,
                Hierarchy = Hierarchy,
                IsMonitorable = IsMonitorable,
                ItCapacity = ItCapacity,
                KvaRating = KvaRating,
                KwRating = KwRating,
                MaxItCapacity = MaxItCapacity,
                MaintenanceParent = MaintenanceParent,
                OnboardingMode = OnboardingMode,
                PowerFactor = PowerFactor,
                PanelName = PanelName,
                PrimaryParent = PrimaryParent,
                ProjectName = ProjectName,
                Parent = Parent,
                RouteKey = RouteKey,
                RatedCapacity = RatedCapacity,
                ReadingStats = ReadingStats,
                RedundantDeviceNames = RedundantDeviceNames,
                SecondaryParent = SecondaryParent,
                ValidationResults = ValidationResults,
                Voltage = Voltage,
                VoltageRating = VoltageRating
            };
            CloneEnrichment(this, clone);
            return clone;
        }

        public IRoutable WithRouteKey(int key)
        {
            RouteKey = key;
            return this;
        }
        #endregion
    }
}