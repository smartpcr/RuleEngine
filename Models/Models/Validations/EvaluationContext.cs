// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EvaluationContext.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.Validations
{
    using System.Collections.Generic;
    using System.Threading;
    using IoT;

    public class EvaluationContext
    {
        public static readonly AsyncLocal<string> CurrentRuleId = new AsyncLocal<string>();
        
        public Dictionary<string, Device> DeviceLookup { get; set; }
        public Dictionary<string, Dictionary<string, DeviceRelation>> RelationLookup { get; set; }

        public EvaluationContext()
        {
            DeviceLookup = new Dictionary<string, Device>();
            RelationLookup=new Dictionary<string, Dictionary<string, DeviceRelation>>();
        }
    }
}