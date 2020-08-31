// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceRelation.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.IoT
{
    public class DeviceRelation
    {
        public string FromDeviceName { get; set; }
        public string ToDeviceName { get; set; }
        public AssociationType Association { get; set; }
        public DirectionType Direction { get; set; }
    }
    
    public enum DirectionType
    {
        DirectUpstream,
        DirectDownstream
    }
}