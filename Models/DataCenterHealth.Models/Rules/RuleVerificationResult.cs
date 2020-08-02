// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationResult.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Rules
{
    using System.Collections.Generic;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;

    public class RuleVerificationResult
    {
        public string DcName { get; set; }
        public Rule Rule { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
        // public List<PowerDevice> FilteredDevices { get; set; }
        // public List<RuleAssertionResult> AssertionResults { get; set; }
    }

    public class RuleAssertionResult
    {
        public string DeviceName { get; set; }
        public bool Passed { get; set; }
        public List<DeviceValidationEvidence> Evidences { get; set; }
    }
}