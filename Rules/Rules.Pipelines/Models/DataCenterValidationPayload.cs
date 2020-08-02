// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenterValidationPayload.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Models
{
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.Rules;

    public class DataCenterValidationPayload : IRoutable
    {
        public string DcName { get; set; }
        public ValidationRule Rule { get; set; }
        public string JobId { get; set; }
        public string RunId { get; set; }

        public DataCenterValidationPayload()
        {
        }

        public DataCenterValidationPayload(string dcName, ValidationRule rule, string jobId, string runId)
        {
            DcName = dcName;
            Rule = rule;
            JobId = jobId;
            RunId = runId;
        }

        #region routable
        public int RouteKey { get; set; }
        public IRoutable Clone()
        {
            return new DataCenterValidationPayload(DcName, Rule, JobId, RunId) {RouteKey = RouteKey};
        }

        public IRoutable WithRouteKey(int key)
        {
            RouteKey = key;
            return this;
        }
        #endregion
    }
}