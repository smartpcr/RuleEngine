// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCenter.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models
{
    using System;
    using FluentValidation;
    using Newtonsoft.Json;

    [TrackChange(true)]
    public class DataCenter : TrackableEntity
    {
        /// <summary>
        ///     changed from kusto field: DcShortName
        /// </summary>
        public string DcName { get; set; }

        /// <summary>
        ///     changed from kusto field: DcName
        /// </summary>
        public string DcLongName { get; set; }

        public string Region { get; set; }
        public long DcCode { get; set; }
        public string CampusName { get; set; }
        public string DcGeneration { get; set; }
        public string Owner { get; set; }

        /// <summary>
        ///     avoiding using reserved keyword in typescript
        /// </summary>
        [JsonProperty("class")]
        public string ClassName { get; set; }

        public string PhaseName { get; set; }
        public string CoolingType { get; set; }
        public string HVACType { get; set; }
        public string MSAssetId { get; set; }

        [JsonIgnore] public int TotalRuns { get; set; }

        [JsonIgnore] public decimal? LatestRunScore { get; set; }

        [JsonIgnore] public DateTime? LatestRunTime { get; set; }

        [JsonIgnore] public int? LatestRunTotalDevices { get; set; }

        [JsonIgnore] public int? LatestRunTotalRules { get; set; }
    }

    public class DataCenterValidator : AbstractValidator<DataCenter>
    {
        public DataCenterValidator()
        {
            RuleFor(x => x.DcName).NotNull().NotEmpty();
            RuleFor(x => x.DcName).NotNull().NotEmpty();
        }
    }

    public class ArgusEnabledDc : BaseEntity
    {
        public string DcName { get; set; }
    }
}