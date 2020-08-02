// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceAssociation.cs" company="Microsoft">
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Devices
{
    using System.Collections.Generic;

    public class DeviceAssociation
    {
        public string DeviceName { get; set; }
        public AssociationType AssociationType { get; set; }
    }

    [TrackChange(true)]
    public class DeviceRelation : BaseEntity
    {
        public string Name { get; set; }
        public string DcName { get; set; }

        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
    }
}