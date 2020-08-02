// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Cache
{
    public class RedisSettings
    {
        public string HostName { get; set; }
        public string AccessKeySecretName { get; set; }
        public string ProtectionCertSecretName { get; set; }

        public string Endpoint => $"{HostName}.redis.cache.windows.net:6380";

    }
}