// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDeviceRaw.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;

    public class PowerDeviceRaw
    {
        public string DataCenterName { get; set; }
        public string DataPoint { get; set; }
        public double Value { get; set; }
        public DateTime Time { get; set; }
        public long Status { get; set; }
    }
}