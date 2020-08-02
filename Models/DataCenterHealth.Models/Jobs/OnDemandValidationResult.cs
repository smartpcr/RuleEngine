namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;

    public class OnDemandValidationResult
    {
        public List<string> RequestedDcNames { get; set; }
        public List<string> RequestedDeviceNames { get; set; }
        public string DeviceName { get; set; }
        public bool? Assert { get; set; }
        public string Error { get; set; }
        public decimal? Score { get; set; }
        public List<DeviceValidationEvidence> Evidences { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public decimal Weight { get; set; }
        public string RuleSetId { get; set; }
        public string RuleSetName { get; set; }
        public string RuleSetType { get; set; }
        public string TimeSpan { get; set; }
        public int? TotalEvaluated { get; set; }
        public int? TotalDevices { get; set; }
        public int? TotalPayloads { get; set; }
        public int? TotalRules { get; set; }
    }
}