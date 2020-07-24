// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpsSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.KeyVault
{
    public class HttpsSettings
    {
        public string SslCertSecretName { get; set; }
        public int PortNumber { get; set; }
    }
}