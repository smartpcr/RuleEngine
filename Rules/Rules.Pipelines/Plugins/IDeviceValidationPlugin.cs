// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeviceValidationPlugin.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Plugins
{
    using System;
    using System.Collections.Generic;
    using DataCenterHealth.Models.Jobs;

    public interface IDeviceValidationPlugin
    {
        DeviceValidationResult Validate<T>(T payload, Dictionary<string, T> lookup) where T : class, new();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ValidationPluginAttribute : Attribute
    {

    }
}