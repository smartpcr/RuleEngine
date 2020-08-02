// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationContext.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Common.Telemetry;
    using DataCenterHealth.Models.DataTypes;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Traversals;
    using Microsoft.Extensions.Logging;

    public class EvaluationContext
    {
        public string DcName { get; }
        public string RunId { get; }
        public string JobId { get; }
        public ILogger Logger { get; }
        public IAppTelemetry AppTelemetry { get; }
        public List<string> EnabledDataCenters { get; set; }
        public Dictionary<string, PowerDevice> DeviceLookup { get; set; }
        public Dictionary<string, PowerDevice> RedundantDeviceLookup { get; set; }
        public Dictionary<string, List<DeviceRelation>> RelationLookup { get; set; }
        public Dictionary<string, List<ZenonEventStats>> ZenonStatsLookup { get; set; }
        public Dictionary<string, List<ZenonLastReading>> ZenonLastReadingLookup { get; set; }
        public Dictionary<string, List<ZenonDataPoint>> ZenonDataPointLookup { get; set; }
        public DeviceHierarchyDeviceTraversal DeviceTraversal { get; set; }

        public EvaluationContext(string dcName, string runId, string jobId, ILogger logger, IAppTelemetry appTelemetry)
        {
            DcName = dcName;
            RunId = runId;
            JobId = jobId;
            Logger = logger;
            AppTelemetry = appTelemetry;
            EnabledDataCenters = new List<string>();
            DeviceLookup = new Dictionary<string, PowerDevice>();
            RedundantDeviceLookup = new Dictionary<string, PowerDevice>();
            RelationLookup = new Dictionary<string, List<DeviceRelation>>();
            ZenonStatsLookup = new Dictionary<string, List<ZenonEventStats>>();
            ZenonLastReadingLookup = new Dictionary<string, List<ZenonLastReading>>();
            ZenonDataPointLookup = new Dictionary<string, List<ZenonDataPoint>>();
        }

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

        public List<(string deviceName, string ruleId, decimal score)> Scores { get; set; } =
            new List<(string deviceName, string ruleId, decimal score)>();

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