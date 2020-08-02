// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopaConfig.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System;

    public class CopaConfig : IEquatable<CopaConfig>
    {
        public string ProjectName { get; set; }
        public string DriverName { get; set; }
        public string ConfiguredDriverType { get; set; }
        public string ConfiguredObjectType { get; set; }
        public string ConfiguredDriverExeName { get; set; }
        public string ConnectionName { get; set; }
        public string NetAddress { get; set; }
        public string PrimaryIpAddress { get; set; }
        public int PortNumber { get; set; }
        public int UnitId { get; set; }
        public bool Offset { get; set; }
        public uint StartOffset { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSerial { get; set; }
        public bool IsMultiMaster { get; set; }
        public bool SkipRegister { get; set; }

        public bool Equals(CopaConfig other)
        {
            return string.Equals(ProjectName, other.ProjectName) && string.Equals(DriverName, other.DriverName) &&
                   string.Equals(ConfiguredDriverType, other.ConfiguredDriverType) &&
                   string.Equals(ConfiguredObjectType, other.ConfiguredObjectType) &&
                   string.Equals(ConfiguredDriverExeName, other.ConfiguredDriverExeName) &&
                   string.Equals(ConnectionName, other.ConnectionName) &&
                   string.Equals(NetAddress, other.NetAddress) &&
                   string.Equals(PrimaryIpAddress, other.PrimaryIpAddress) &&
                   PortNumber == other.PortNumber && UnitId == other.UnitId &&
                   Offset == other.Offset && StartOffset == other.StartOffset &&
                   IsEnabled == other.IsEnabled && IsSerial == other.IsSerial && IsMultiMaster == other.IsMultiMaster &&
                   SkipRegister == other.SkipRegister;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType().GUID != GetType().GUID) return false;
            return Equals((CopaConfig) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ProjectName?.GetHashCode() ?? 0;
                hashCode = hashCode * 397 + (DriverName?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + (ConfiguredDriverType?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + (ConfiguredObjectType?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + (ConfiguredDriverExeName?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + (ConnectionName?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + (NetAddress?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + (PrimaryIpAddress?.GetHashCode() ?? 0);
                hashCode = hashCode * 397 + PortNumber;
                hashCode = hashCode * 397 + UnitId;
                hashCode = hashCode * 397 + Offset.GetHashCode();
                hashCode = hashCode * 397 + (int) StartOffset;
                hashCode = hashCode * 397 + IsEnabled.GetHashCode();
                hashCode = hashCode * 397 + IsSerial.GetHashCode();
                hashCode = hashCode * 397 + IsMultiMaster.GetHashCode();
                hashCode = hashCode * 397 + SkipRegister.GetHashCode();

                return hashCode;
            }
        }
    }
}