// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VaultSettings.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.KeyVault
{
    public class VaultSettings
    {
        public string VaultName { get; set; }
        public string VaultUrl => $"https://{VaultName}.vault.azure.net";
        public VaultAuthMode AuthMode { get; set; } = VaultAuthMode.Msi;
    }

    public enum VaultAuthMode
    {
        Msi,
        Spn
    }
}