// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleSetting.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.PowerServices
{
    public class RuleSetting : BaseEntity
    {
        public string Key { get; set; }
        public string StringValue { get; set; }
        public int Version { get; set; } = 0;
    }
}