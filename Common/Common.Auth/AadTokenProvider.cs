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

        public (string secret, X509Certificate2 cert) GetClientSecretOrCert()
        {
            if (!string.IsNullOrEmpty(settings.ClientSecretFile))
            {
                var clientSecretFile = GetSecretOrCertFile(settings.ClientSecretFile);
                var clientSecret = File.ReadAllText(clientSecretFile);
                return (clientSecret, null);
            }

            var clientCertFile = GetSecretOrCertFile(settings.ClientCertFile);
            var certificate = new X509Certificate2(clientCertFile);
            return (null, certificate);
        }

        public (ClientSecretCredential secretCredential, ClientCertificateCredential certCredential)
            GetClientCredential()
        {
            var secretOrCert = GetClientSecretOrCert();
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