// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Device.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public abstract class Device : TrackableEntity, IEquatable<Device>
    {
        public string DeviceName { get; set; }
        public string DcName { get; set; }
        public long DcCode { get; set; }
        public OnboardingMode OnboardingMode { get; set; }
        public string Hierarchy { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceType DeviceType { get; set; }

        public string Parent { get; set; }

        public bool Equals(Device other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DeviceName == other.DeviceName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Device) obj);
        }

        public override int GetHashCode()
        {
            return DeviceName != null ? DeviceName.GetHashCode() : 0;
        }
    }
}