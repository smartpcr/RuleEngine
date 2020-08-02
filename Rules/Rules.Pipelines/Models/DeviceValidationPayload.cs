// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationPayload.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Models
{
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Rules;
    using Newtonsoft.Json;

    public class DeviceValidationPayload : IRoutable
    {
        public DeviceValidationPayload()
        {
        }

        public DeviceValidationPayload(PowerDevice device, ValidationRule rule, string jobId, string runId)
        {
            Device = device;
            Rule = rule;
            JobId = jobId;
            RunId = runId;
        }

        public PowerDevice Device { get; set; }
        public ValidationRule Rule { get; set; }
        public string JobId { get; set; }
        public string RunId { get; set; }

        #region routing

        [JsonIgnore] public int RouteKey { get; set; }

        public IRoutable Clone()
        {
            return new DeviceValidationPayload(Device, Rule, JobId, RunId) {RouteKey = RouteKey};
        }

        public IRoutable WithRouteKey(int key)
        {
            RouteKey = key;
            return this;
        }

        #endregion
    }
}