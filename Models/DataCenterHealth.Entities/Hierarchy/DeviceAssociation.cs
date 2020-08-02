// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceAssociation.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Entities.Hierarchy
{
    using System;
    using System.Collections.Generic;
    using DataCenterHealth.Models;

    [TrackChange(true)]
    public class DeviceAssociation
    {
        public string DeviceName { get; set; }
        public AssociationType AssociationType { get; set; }
        public override string ToString()
        {
            return $"[DeviceName: \"{this.DeviceName}\", AssociationType: \"{this.AssociationType}\"]";
        }
    }

    public class DeviceRelation
    {
        public string Name { get; set; }
        public string DcName { get; set; }

        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
    }

    public class DeviceAssociationComparer : IEqualityComparer<DeviceAssociation>
    {
        public bool Equals(DeviceAssociation x, DeviceAssociation y)
        {
            // Check whether the objects are the same object.
            if (Object.ReferenceEquals(x, y)) return true;

            return x != null && y != null && x.DeviceName.Equals(y.DeviceName) && x.AssociationType.Equals(y.AssociationType);

        }

        public int GetHashCode(DeviceAssociation obj)
        {
            // Get hash code for the Name field if it is not null.
            int hashDeviceName = obj.DeviceName?.GetHashCode() ?? 0;

            // Get hash code for the Code field.
            int hashDeviceAssociationType = obj.AssociationType.GetHashCode();

            return hashDeviceName ^ hashDeviceAssociationType;
        }
    }
}
