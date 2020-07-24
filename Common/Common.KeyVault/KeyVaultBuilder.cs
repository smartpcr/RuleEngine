// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyVaultBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Common.KeyVault
{
    using System;
    using System.Threading.Tasks;
    using Auth;
    using Config;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class KeyVaultBuilder
    {
        public static IServiceCollection AddKeyVault(this IServiceCollection services, IConfiguration configuration)
        {
            var aadSettings = configuration.GetConfiguredSettings<AadSettings>();

            IKeyVaultClient kvClient;
            if (string.IsNullOrEmpty(aadSettings.ClientCertFile) && string.IsNullOrEmpty(aadSettings.ClientSecretFile))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                kvClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        azureServiceTokenProvider.KeyVaultTokenCallback));
            }
            else
            {
                var authBuilder = new AadTokenProvider(aadSettings);

                Task<string> AuthCallback(string authority, string resource, string scope)
                {
                    return authBuilder.GetAccessTokenAsync(resource);
                }

                kvClient = new KeyVaultClient(AuthCallback);
            }

            services.AddSingleton(kvClient);

            return services;
        }

        public static void UseSslAuth(this KestrelServerOptions options)
        {
            var serviceProvider = options.ApplicationServices;
            var kvClient = serviceProvider.GetRequiredService<IKeyVaultClient>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var httpsSettings = configuration.GetConfiguredSettings<HttpsSettings>();
            if (httpsSettings != null)
            {
                Console.WriteLine(
                    $"serving web request... port: {httpsSettings.PortNumber}, cert: {httpsSettings.SslCertSecretName}");
                var vaultSettings = configuration.GetConfiguredSettings<VaultSettings>();
                var x509 = kvClient.GetX509CertificateAsync(vaultSettings.VaultUrl, httpsSettings.SslCertSecretName)
                    .GetAwaiter().GetResult();
                options.ListenAnyIP(httpsSettings.PortNumber, listenOptions => { listenOptions.UseHttps(x509); });
            }
        }
    }
}