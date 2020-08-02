// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Device.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Devices
{
    using System;
    using Newtonsoft.Json;

    public class Device : IEquatable<Device>
    {
        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }
        public string DcName { get; set; }
        public long DcCode { get; set; }
        public OnboardingMode OnboardingMode { get; set; }

        public bool Equals(Device other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DeviceName.ToLower() == other.DeviceName.ToLower();
        }
    }
}
