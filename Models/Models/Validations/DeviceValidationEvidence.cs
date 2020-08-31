// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceValidationEvidence.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Models.Validations
{
    public class DeviceValidationEvidence
    {
        public string PropertyPath { get; set; }
        public string Actual { get; set; }
        public string Expected { get; set; }
    }
}