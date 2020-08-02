// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CeDataPoint.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.DataTypes
{
    using System;
    using Newtonsoft.Json;

    [TrackChange(true)]
    public class CeDataPoint : BaseEntity
    {
        public string DcName { get; set; }
        private string dataPoint;
        [JsonProperty("dataPoint")]
        public string DataPoint
        {
            get => dataPoint;
            set
            {
                dataPoint = value;
                var parts = dataPoint.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    DeviceName = parts[0];
                }
            }
        }
        public string DataPointType { get; set; }
        public string PGFileName { get; set; }
        public string DeviceName { get; set; }
    }
}