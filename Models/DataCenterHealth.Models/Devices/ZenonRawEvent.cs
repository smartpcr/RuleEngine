//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ZenonRawEvent.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;

    public class ZenonRawEvent : BaseEntity
    {
        public string DataPoint { get; set; }
        public DateTime EventTime { get; set; }
        public double Value { get; set; }
    }
}