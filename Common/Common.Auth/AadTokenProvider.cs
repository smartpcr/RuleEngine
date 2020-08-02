// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AadAuthBuilder.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Auth
{
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class AadTokenProvider
    {
        private readonly AadSettings settings;

        public AadTokenProvider(AadSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// based on aadSettings, use either client_cert or client_pwd to authenticate aad
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync(string resource)
        {
            var authContext = new AuthenticationContext(settings.Authority);
            if (!string.IsNullOrEmpty(settings.ClientSecretFile))
            {
                var clientSecretFile = GetSecretOrCertFile(settings.ClientSecretFile);
                var clientSecret = File.ReadAllText(clientSecretFile);
                var clientCredential = new ClientCredential(settings.ClientId, clientSecret);
                var result = await authContext.AcquireTokenAsync(resource, clientCredential);
                return result?.AccessToken;
            }
            else
            {
                var clientCertFile = GetSecretOrCertFile(settings.ClientCertFile);
                var certificate = new X509Certificate2(clientCertFile);
                var clientAssertion = new ClientAssertionCertificate(settings.ClientId, certificate);
                var result = await authContext.AcquireTokenAsync(resource, clientAssertion);
                return result?.AccessToken;
            }
        }

        public (string secret, X509Certificate2 cert) GetClientSecretOrCert(
            Func<string, string> getSecretFromVault,
            Func<string,X509Certificate2> getCertFromVault)
        {
            if (!string.IsNullOrEmpty(settings.ClientSecretFile))
            {
                var clientSecretFile = GetSecretOrCertFile(settings.ClientSecretFile);
                var clientSecret = File.ReadAllText(clientSecretFile);
                return (clientSecret, null);
            }

            if (!string.IsNullOrEmpty(settings.ClientCertFile))
            {
                var clientCertFile = GetSecretOrCertFile(settings.ClientCertFile);
                var certificate = new X509Certificate2(clientCertFile);
                return (null, certificate);
            }

            if (!string.IsNullOrEmpty(settings.ClientPwdSecretName) && getSecretFromVault != null)
            {
                var clientSecret = getSecretFromVault(settings.ClientPwdSecretName);
                return (clientSecret, null);
            }

            if (!string.IsNullOrEmpty(settings.ClientCertSecretName) && getCertFromVault != null)
            {
                var clientCert = getCertFromVault(settings.ClientCertSecretName);
                return (null, clientCert);
            }

            return (null, null);
        }

        public (ClientSecretCredential secretCredential, ClientCertificateCredential certCredential) GetClientCredential(
            Func<string, string> getSecretFromVault,
            Func<string,X509Certificate2> getCertFromVault)
        {
            var secretOrCert = GetClientSecretOrCert(getSecretFromVault, getCertFromVault);
            if (secretOrCert.secret != null)
            {
                var credential = new ClientSecretCredential(
                    settings.TenantId,
                    settings.ClientId,
                    secretOrCert.secret,
                    new TokenCredentialOptions
                    {
                        AuthorityHost = new Uri(AadSettings.MicrosoftAadLoginUrl)
                    });
                return (credential, null);
            }
            else
            {
                var credential = new ClientCertificateCredential(
                    settings.TenantId,
                    settings.ClientId,
                    secretOrCert.cert,
                    new TokenCredentialOptions
                    {
                        AuthorityHost = new Uri(AadSettings.MicrosoftAadLoginUrl)
                    });
                return (null, credential);
            }
        }

        /// <summary>
        ///     fallback: secretFile --> ~/.secrets/secretFile --> /tmp/.secrets/secretFile
        /// </summary>
        /// <param name="secretOrCertFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static string GetSecretOrCertFile(string secretOrCertFile)
        {
            var secretOrCertFilePath = secretOrCertFile;
            if (!File.Exists(secretOrCertFilePath))
            {
                var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                secretOrCertFilePath = Path.Combine(homeFolder, ".secrets", secretOrCertFile);

                if (!File.Exists(secretOrCertFilePath))
                    secretOrCertFilePath = Path.Combine("/tmp/.secrets", secretOrCertFile);
            }

            if (!File.Exists(secretOrCertFilePath))
                throw new Exception($"unable to find client secret/cert file: {secretOrCertFilePath}");

            return secretOrCertFilePath;
        }
    }
}