namespace DataCenterHealth.Models.Jobs
{
    using System;
    using System.Collections.Generic;

    public class OnDemandValidationSummary
    {
        public string Id { get; set; }
        public List<string> DcNames { get; set; }
        public List<string> DeviceNames { get; set; }
        public List<string> RuleIds { get; set; }
        public List<string> RuleSetIds { get; set; }
        public List<string> RuleNames { get; set; }
        public List<string> RuleSetNames { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string SubmittedBy { get; set; }
        public int? TotalJobs { get; set; }
        public int? TotalRuns { get; set; }
        public string TimeSpan { get; set; }
        public int? TotalEvaluated  { get; set; }
        public decimal? AverageScore { get; set; }
    }
}