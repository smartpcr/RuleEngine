// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HostEnvironment.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Config
{
    using System;

    public enum HostType
    {
        Local,
        Docker,
        Kubernetes
    }

    public class HostEnvironment
    {
        public static HostType GetHostEnvironmentType()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST")))
                return HostType.Kubernetes;

            return Environment.UserInteractive ? HostType.Docker : HostType.Local;
        }
    }
}