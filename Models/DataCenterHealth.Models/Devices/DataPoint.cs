// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataPoint.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using Newtonsoft.Json;

    [TrackChange(true)]
    public class DataPoint : TrackableEntity
    {
        [JsonProperty("dataPoint")] public string Name { get; set; }

        public string DataType { get; set; }
        public string ChannelType { get; set; }
        public string Channel { get; set; }
        public int Offset { get; set; }
        public int PollInterval { get; set; }
        public int Scaling { get; set; }
        public string Priminitive { get; set; }
        public bool FilterdOutInPG { get; set; }
    }
}