// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlobStorageSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Storage
{
    public class BlobStorageSettings
    {
        public string Account { get; set; }
        public string Container { get; set; }
        public string ConnectionStringEnvName { get; set; }
        public string ConnectionStringSecretName { get; set; }
        public string ContainerEndpoint => $"https://{Account}.blob.core.windows.net/{Container}";
        public StorageAuthMode AuthMode { get; set; } = StorageAuthMode.Msi;
    }

    public enum StorageAuthMode
    {
        Msi,
        Spn,
        SecretFromVault,
        ConnStr
    }
}