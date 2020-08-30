// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests.TestModels.IoT
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Contexts;
    using Newtonsoft.Json;

    public class Device : DeviceData
    {
        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
        public DeviceData PrimaryParentDevice { get; set; }
        public DeviceData SecondaryParentDevice { get; set; }
        public DeviceData MaintenanceParentDevice { get; set; }
        public DeviceData RedundantDevice { get; set; }
        public DeviceData RootDevice { get; set; }
        public List<DeviceData> Children { get; set; }
        public bool IsRedundantDevice { get; set; }
        
        [NotMapped] public List<DataPoint> DataPoints { get; set; }
        
        #region evaluation
        [JsonIgnore, NotMapped] public EvaluationContext EvaluationContext { get; set; }

        [JsonIgnore, NotMapped] public ConcurrentDictionary<string, EvaluationEvidence> EvidencesByRule { get; private set; }

        public void AddEvaluationEvidence(EvaluationEvidence evidence)
        {
            if (EvaluationContext.CurrentRuleId.Value != null)
            {
                EvidencesByRule ??= new ConcurrentDictionary<string, EvaluationEvidence>();
                EvidencesByRule.AddOrUpdate(EvaluationContext.CurrentRuleId.Value, evidence, (key, value) => evidence);
            }
        }
        #endregion
    }
}