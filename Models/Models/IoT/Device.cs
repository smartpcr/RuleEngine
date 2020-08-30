// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerDevice.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Device : DeviceData
    {
        public List<DeviceAssociation> DirectUpstreamDeviceList { get; set; }
        public List<DeviceAssociation> DirectDownstreamDeviceList { get; set; }
        public DeviceData PrimaryParentDevice { get; set; }
        public DeviceData SecondaryParentDevice { get; set; }
        public DeviceData MaintenanceParentDevice { get; set; }
        public DeviceData RedundantDevice { get; set; }
        public DeviceData RootDevice { get; set; }
        public List<DeviceData> Children { get; set; }
        public bool IsRedundantDevice { get; set; }
        
        [NotMapped] public List<DataPoint> DataPoints { get; set; }
        
    }
}