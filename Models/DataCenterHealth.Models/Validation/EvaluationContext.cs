// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationContext.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Telemetry;
    using DataCenterHealth.Models.DataTypes;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using DataCenterHealth.Models.Traversals;
    using Microsoft.Extensions.Logging;

    public class EvaluationContext
    {
        private readonly ILoggerFactory loggerFactory;
        public string DcName { get; }
        public List<string> DeviceNames { get; }
        public bool IsDataCenterEnabled { get; private set; }
        public DeviceValidationRun Run { get; }
        public string JobId { get; }
        public ValidationContextScope Scope { get; }
        public List<ValidationRule> Rules { get; set; }
        public ILogger Logger { get; }
        public IAppTelemetry AppTelemetry { get; }
        public Dictionary<string, PowerDevice> DeviceLookup { get; private set; }
        public Dictionary<string, PowerDevice> RedundantDeviceLookup { get; private set; }
        public Dictionary<string, List<DeviceRelation>> RelationLookup { get; private set; }
        public Dictionary<string, List<ZenonEventStats>> ZenonStatsLookup { get; private set; }
        public Dictionary<string, List<ZenonLastReading>> ZenonLastReadingLookup { get; private set; }
        public Dictionary<string, List<ZenonDataPoint>> ZenonDataPointLookup { get; private set; }
        public DeviceHierarchyDeviceTraversal DeviceTraversal { get; private set; }

        public EvaluationContext(
            string dcName,
            List<string> deviceNames,
            DeviceValidationRun run,
            string jobId,
            ValidationContextScope scope,
            List<ValidationRule> rules,
            ILoggerFactory loggerFactory,
            IAppTelemetry appTelemetry)
        {
            this.loggerFactory = loggerFactory;
            DcName = dcName;
            DeviceNames = deviceNames;
            if (string.IsNullOrEmpty(DcName) && DeviceNames?.Any() == true)
            {
                var deviceNameParts = DeviceNames[0].Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                DcName = deviceNameParts[0];
            }

            Run = run;
            JobId = jobId;
            Scope = scope;
            Rules = rules;
            Logger = loggerFactory.CreateLogger<EvaluationContext>();
            AppTelemetry = appTelemetry;
            IsDataCenterEnabled = true;
            DeviceLookup = new Dictionary<string, PowerDevice>();
            RedundantDeviceLookup = new Dictionary<string, PowerDevice>();
            RelationLookup = new Dictionary<string, List<DeviceRelation>>();
            ZenonStatsLookup = new Dictionary<string, List<ZenonEventStats>>();
            ZenonLastReadingLookup = new Dictionary<string, List<ZenonLastReading>>();
            ZenonDataPointLookup = new Dictionary<string, List<ZenonDataPoint>>();
        }
        
        #region lookup

        public void SetEnabledDataCenters(List<string> enabledDcNames)
        {
            IsDataCenterEnabled = enabledDcNames.Contains(DcName);
        }
        
        public void SetDevices(Dictionary<string, PowerDevice> deviceLookup)
        {
            DeviceLookup = deviceLookup;
            if (DeviceLookup?.Any() == true && RelationLookup?.Any() == true)
            {
                DeviceTraversal = new DeviceHierarchyDeviceTraversal(DeviceLookup, RelationLookup, loggerFactory);
            }
        }

        public void SetRedundantDevices(Dictionary<string, PowerDevice> redundantDeviceLookup)
        {
            RedundantDeviceLookup = redundantDeviceLookup;
        }

        public void SetDeviceRelations(Dictionary<string, List<DeviceRelation>> relationLookup)
        {
            RelationLookup = relationLookup;
            if (DeviceLookup?.Any() == true && RelationLookup?.Any() == true)
            {
                DeviceTraversal = new DeviceHierarchyDeviceTraversal(DeviceLookup, RelationLookup, loggerFactory);
            }
        }

        public void SetZenonReadingStats(Dictionary<string, List<ZenonEventStats>> zenonEventStatsLookup)
        {
            ZenonStatsLookup = zenonEventStatsLookup;
        }

        public void SetLastZenonReadings(Dictionary<string, List<ZenonLastReading>> zenonLastReadingLookup)
        {
            ZenonLastReadingLookup = zenonLastReadingLookup;
        }

        public void SetZenonDataPoints(Dictionary<string, List<ZenonDataPoint>> zenonDataPointLookup)
        {
            ZenonDataPointLookup = zenonDataPointLookup;
        }
        #endregion

        #region counts
        private int totalBroadcast;

        private int totalEvaluated;

        private int totalFailed;

        private int totalFiltered;

        private int totalReceived;

        private int totalSaved;

        private int totalSent;

        /// <summary>
        ///     payload created by producer
        /// </summary>
        public int TotalSent
        {
            get => totalSent;
            set => totalSent = value;
        }

        /// <summary>
        ///     received by transformer
        /// </summary>
        public int TotalReceived
        {
            get => totalReceived;
            set => totalReceived = value;
        }

        public int TotalBroadcast
        {
            get => totalBroadcast;
            set => totalBroadcast = value;
        }

        /// <summary>
        ///     validated by transformer
        /// </summary>
        public int TotalEvaluated
        {
            get => totalEvaluated;
            set => totalEvaluated = value;
        }

        public int TotalFiltered
        {
            get => totalFiltered;
            set => totalFiltered = value;
        }

        public int TotalFailed
        {
            get => totalFailed;
            set => totalFailed = value;
        }

        /// <summary>
        ///     sent to db
        /// </summary>
        public int TotalSaved
        {
            get => totalSaved;
            set => totalSaved = value;
        }

        public TimeSpan Span { get; set; }

        public List<(string deviceName, string ruleId, double score)> Scores { get; set; } =
            new List<(string deviceName, string ruleId, double score)>();

        public void AddTotalSent(int value)
        {
            Interlocked.Add(ref totalSent, value);
        }

        public void AddTotalReceived(int value)
        {
            Interlocked.Add(ref totalReceived, value);
        }

        public void AddTotalBroadcast(int value)
        {
            Interlocked.Add(ref totalBroadcast, value);
        }

        public void AddTotalEvaluated(int value)
        {
            Interlocked.Add(ref totalEvaluated, value);
        }

        public void AddTotalFiltered(int value)
        {
            Interlocked.Add(ref totalFiltered, value);
        }

        public void AddTotalFailed(int value)
        {
            Interlocked.Add(ref totalFailed, value);
        }

        public void AddTotalSaved(int value)
        {
            Interlocked.Add(ref totalSaved, value);
        }

        #endregion
    }
}