// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssociationType.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT
{
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(Primary)]
    public enum AssociationType
    {
        [Description("PowerSource")] PowerSource = 0,
        [Description("Primary")] Primary = 1,
        [Description("Backup")] Backup = 2,
        [Description("Maintenance")] Maintenance = 3
    }
}