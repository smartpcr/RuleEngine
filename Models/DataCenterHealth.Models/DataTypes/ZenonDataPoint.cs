// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZenonDataPoint.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.DataTypes
{
    using DataCenterHealth.Models;

    [TrackChange(true)]
    public class ZenonDataPoint : BaseEntity
    {
        public string DeviceDataPoint { get; set; }
        public string DcName { get; set; }
        public string DeviceName { get; set; }
        public string DataPoint { get; set; }
        public string DataType { get; set; }
        public string ChannelType { get; set; }
        public string Channel { get; set; }
        public int Offset { get; set; }
        public int PollInterval { get; set; }
        public int Scaling { get; set; }
        public string Primitive { get; set; }
        public bool FilterdOutInPG { get; set; }
    }
}