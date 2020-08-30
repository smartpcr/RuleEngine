// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceAssociation.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT
{
    public class DeviceAssociation
    {
        public string DeviceName { get; set; }
        public AssociationType AssociationType { get; set; }
    }
}