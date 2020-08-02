namespace DataCenterHealth.Models.Rules
{
    using System.Collections.Generic;
    using DataCenterHealth.Models.Summaries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum RuleType
    {
        JsonRule,
        CodeRule,
        HealthCheck,
        Plugin
    }

    [TrackChange(true, ChangeType.ValidationRule)]
    public class RuleSet : TrackableEntity
    {
        public const string CodeRuleSetName = "code rules";
        public const string HealthCheckRuleSetName = "healthcheck";

        public string Name { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RuleType Type { get; set; }
        public string[] DataCenters { get; set; }
        public string[] Hierarchies { get; set; }
        public string[] DeviceTypes { get; set; }

        public List<string> Owners { get; set; }
        public List<string> Contributors { get; set; }
    }

    public class RuleSetWithRules : RuleSet
    {
        public RuleSetWithRules()
        {
            Rules = new List<ValidationRule>();
        }

        public RuleSetWithRules(RuleSet rs) : this()
        {
            Id = rs.Id;
            Name = rs.Name;
            Type = rs.Type;
            DataCenters = rs.DataCenters;
            Hierarchies = rs.Hierarchies;
            DeviceTypes = rs.DeviceTypes;
            CreatedBy = rs.CreatedBy;
            CreationTime = rs.CreationTime;
            ModificationTime = rs.ModificationTime;
            ModifiedBy = rs.ModifiedBy;
        }

        public List<ValidationRule> Rules { get; set; }
    }
}