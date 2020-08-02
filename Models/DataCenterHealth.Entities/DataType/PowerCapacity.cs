// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerCapacity.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Devices
{
    using System;
    using Models.Sync;
    using Newtonsoft.Json;

    public class PowerCapacity : ICloneable
    {
        public double? RatedCapacity { get; set; }

        public double? DeratedCapacity { get; set; }

        public double? MaxItCapacity { get; set; }

        public double? ItCapacity { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType().GUID != this.GetType().GUID) return false;
            return Equals((PowerCapacity)obj);
        }

        protected bool Equals(PowerCapacity other)
        {
            return this.RatedCapacity.Equals(other.RatedCapacity)
                   && this.DeratedCapacity.Equals(other.DeratedCapacity)
                   && this.MaxItCapacity.Equals(other.MaxItCapacity)
                   && this.ItCapacity.Equals(other.ItCapacity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.RatedCapacity.GetHashCode();
                hashCode = hashCode * 397 + this.DeratedCapacity.GetHashCode();
                hashCode = hashCode * 397 + this.MaxItCapacity.GetHashCode();
                hashCode = hashCode * 397 + this.ItCapacity.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
