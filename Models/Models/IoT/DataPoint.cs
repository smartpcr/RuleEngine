// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataPoint.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT
{
    using Newtonsoft.Json;

    public class DataPoint
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