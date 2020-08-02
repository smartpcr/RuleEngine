// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeRule.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    using DataCenterHealth.Models.Summaries;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [TrackChange(true, ChangeType.ValidationRule)]
    public class CodeRule : Rule
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ContextErrorCode ErrorCode { get; set; }
        public string CoceRuleEvaluatorTypeName { get; set; }

        public CodeRule()
        {
            Type = RuleType.CodeRule;
        }
    }

    public enum ContextErrorCode
    {
        DeviceInCircularPath,
        PowersourceDeviceInWrongHierarchy,
        LeafDeviceParentInWrongHierarchy,
        KwTotReadingNotMatchChildren,
        DeviceAmpsReading,
        Stale,
        Complete
    }

    public class CodeRuleEvidence
    {
        public ContextErrorCode ErrorCode { get; set; }
        public bool Passed { get; set; }
        public double Score { get; set; }
        public string PropertyPath { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public string Remarks { get; set; }
    }
}