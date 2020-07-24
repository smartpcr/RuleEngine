// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AadAuthBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Auth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class AadAuthBuilder
    {
        public static AuthenticationBuilder AddAadAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .AddAuthentication(opts =>
                {
                    opts.DefaultScheme = "smart";
                    opts.DefaultChallengeScheme = "smart";
                })
                .AddPolicyScheme("smart", "Authorization Bearer or OIDC",
                    opts =>
                    {
                        opts.ForwardDefaultSelector = AuthSelector(CookieAuthenticationDefaults.AuthenticationScheme);
                    })
                .AddAadBearer(opts => { configuration.Bind("AadSettings", opts); })
                .AddAadOpenId(opts => { configuration.Bind("AadSettings", opts); })
                .AddCookie(opts =>
                {
                    opts.ExpireTimeSpan = TimeSpan.FromHours(1);
                    opts.SlidingExpiration = false;
                });
        }

        public static void RequireAuthenticationOn(this IApplicationBuilder app, string pathPrefix)
        {
            app.Use((context, next) =>
            {
                if (context.Request.Path.HasValue &&
                    context.Request.Path.Value.StartsWith(pathPrefix, StringComparison.InvariantCultureIgnoreCase) &&
                    !context.User.Identity.IsAuthenticated)
                    return context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);

                return next();
            });
        }

        public static IServiceCollection AddAadAuthorization(this IServiceCollection services,
            params string[] extraAuthSchemes)
        {
            var schemes = new List<string>(extraAuthSchemes)
            {
                JwtBearerDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            };
            return services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(schemes.ToArray())
                    .Build();
            });
        }

        public static void UseHttpsForAadRedirect(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Request.Scheme = "https";
                await next.Invoke();
            });
        }

        #region auth

        private static Func<HttpContext, string> AuthSelector(string fallbackScheme)
        {
            return context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                return authHeader?.StartsWith("Bearer ") == true
                    ? JwtBearerDefaults.AuthenticationScheme
                    : fallbackScheme;
            };
        }

        private static AuthenticationBuilder AddAadBearer(this AuthenticationBuilder builder,
            Action<AadSettings> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureBearerOptions>();
            builder.AddJwtBearer(opts => opts.SaveToken = true);

            return builder;
        }

        private static AuthenticationBuilder AddAadOpenId(this AuthenticationBuilder builder,
            Action<AadSettings> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureOpenIdOptions>();
            builder.AddOpenIdConnect(opts =>
            {
                opts.SaveTokens = true;
                opts.ForwardDefaultSelector = AuthSelector(OpenIdConnectDefaults.AuthenticationScheme);
            });

            return builder;
        }

        private class ConfigureBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AadSettings _aadSettings;

            public ConfigureBearerOptions(IOptions<AadSettings> aadSettings)
            {
                _aadSettings = aadSettings.Value;
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                options.Audience = _aadSettings.ClientId;
                options.Authority = _aadSettings.Authority;
            }
        }

        private class ConfigureOpenIdOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AadSettings _aadSettings;

            public ConfigureOpenIdOptions(IOptions<AadSettings> aadSettings)
            {
                _aadSettings = aadSettings.Value;
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                options.ClientId = _aadSettings.ClientId;
                options.Authority = _aadSettings.Authority;
                options.UseTokenLifetime = true;
                options.CallbackPath = _aadSettings.CallbackPath;
            }
        }

        #endregion
    }
}