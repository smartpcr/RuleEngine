// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BackendData.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System;
    using Common.DocDb;
    using DataCenterHealth.Entities.DataType;
    using DataCenterHealth.Entities.Location;
    using DataCenterHealth.Models.PowerServices;
    using DataCenterHealth.Models.Summaries;
    using Entities.Devices;
    using Models;
    using Models.Devices;
    using Models.Jobs;
    using Models.Rules;
    using Models.Sync;
    using DataPoint = Models.Devices.DataPoint;
    using DeviceRelation = Models.Devices.DeviceRelation;
    using PowerDevice = Models.Devices.PowerDevice;

    public class BackendData
    {
        [MappedModel(typeof(DataCenter))] public EntityStoreSettings DataCenter { get; set; }

        [MappedModel(typeof(PowerDevice))] public EntityStoreSettings Device { get; set; }

        [MappedModel(typeof(DeviceRelation))] public EntityStoreSettings DeviceRelation { get; set; }

        [MappedModel(typeof(DataPoint))] public EntityStoreSettings DataPoint { get; set; }
        [MappedModel(typeof(CeDataPoint))] public EntityStoreSettings CEDataPoint { get; set; }
        [MappedModel(typeof(Allocation))] public EntityStoreSettings Allocation { get; set; }

        [MappedModel(typeof(Hierarchy))] public EntityStoreSettings Hierarchy { get; set; }

        [MappedModel(typeof(DeviceRack))] public EntityStoreSettings DeviceRack { get; set; }

        [MappedModel(typeof(RuleSet))] public EntityStoreSettings RuleSet { get; set; }

        [MappedModel(typeof(ValidationRule))] public EntityStoreSettings ValidationRule { get; set; }

        [MappedModel(typeof(EvaluationRule))] public EntityStoreSettings EvaluationRule { get; set; }

        [MappedModel(typeof(CodeRule))] public EntityStoreSettings CodeRule { get; set; }

        [MappedModel(typeof(PowerDeviceRaw))] public EntityStoreSettings PowerDeviceRawEvent { get; set; }

        [MappedModel(typeof(DeviceValidationJob))]
        public EntityStoreSettings DeviceValidationJob { get; set; }

        [MappedModel(typeof(DeviceValidationRun))]
        public EntityStoreSettings DeviceValidationRun { get; set; }

        [MappedModel(typeof(DeviceValidationResult))]
        public EntityStoreSettings DeviceValidationResult { get; set; }

        [MappedModel(typeof(DeviceValidationSchedule))]
        public EntityStoreSettings DeviceValidationSchedule { get; set; }

        [MappedModel(typeof(QueueJobRequest))]
        public EntityStoreSettings QueueJobRequest { get; set; }

        [MappedModel(typeof(SyncSetting))]
        public EntityStoreSettings SyncSettings { get; set; }

        [MappedModel(typeof(SyncJob))]
        public EntityStoreSettings SyncJobs { get; set; }

        [MappedModel(typeof(ZenonDataType))]
        public EntityStoreSettings ZenonDataType { get; set; }

        [MappedModel(typeof(ZenonDriverConfig))]
        public EntityStoreSettings ZenonDriverConfig { get; set; }

        [MappedModel(typeof(ZenonDataPointConfig))]
        public EntityStoreSettings ZenonDataPointConfig { get; set; }

        [MappedModel(typeof(ChangeHistory))]
        public EntityStoreSettings ChangeHistory { get; set; }

        [MappedModel(typeof(ExecutionHistory))]
        public EntityStoreSettings ExecutionHistory { get; set; }

        [MappedModel(typeof(Alert))]
        public EntityStoreSettings Alert { get; set; }

        #region powerservice
        [MappedModel(typeof(RuleSetting))] public EntityStoreSettings RuleSetting { get; set; }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class MappedModelAttribute : Attribute
    {
        public MappedModelAttribute(Type modelType)
        {
            ModelType = modelType;
        }

        public Type ModelType { get; set; }
    }
}