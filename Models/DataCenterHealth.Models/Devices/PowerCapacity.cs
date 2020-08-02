// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerCapacity.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;
    using Newtonsoft.Json;

    public class PowerCapacity : ICloneable
    {
        public double? RatedCapacity { get; set; }

        public double? DeratedCapacity { get; set; }

        public double? MaxItCapacity { get; set; }

        public double? ItCapacity { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType().GUID != GetType().GUID) return false;
            return Equals((PowerCapacity) obj);
        }

        protected bool Equals(PowerCapacity other)
        {
            return RatedCapacity.Equals(other.RatedCapacity)
                   && DeratedCapacity.Equals(other.DeratedCapacity)
                   && MaxItCapacity.Equals(other.MaxItCapacity)
                   && ItCapacity.Equals(other.ItCapacity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RatedCapacity.GetHashCode();
                hashCode = hashCode * 397 + DeratedCapacity.GetHashCode();
                hashCode = hashCode * 397 + MaxItCapacity.GetHashCode();
                hashCode = hashCode * 397 + ItCapacity.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}