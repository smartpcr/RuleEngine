// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppAuthFilterAttribute.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Auth
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using Common.Config;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// allows user impersonation and trusted app
    /// </summary>
    public sealed class AppAuthFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger<AppAuthFilterAttribute> logger;
        private readonly AllowedAppSettings settings;

        public AppAuthFilterAttribute(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<AppAuthFilterAttribute>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<AllowedAppSettings>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var authHeaders = context.HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "Authorization");
            var authHeader = authHeaders.Value.FirstOrDefault();
            var (upn, appId) = ParseJwtToken(authHeader);
            logger.LogInformation($"AppAuthorizationFilter: upn: {upn}, appid: {appId}");
            if (!string.IsNullOrEmpty(appId) && settings.WhiteListedAppIds.All(a => !a.Equals(appId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnauthorizedAccessException("The caller is not authorized app specified in white list");
            }

            if (!string.IsNullOrEmpty(upn) && upn.IndexOf("@microsoft.com", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new UnauthorizedAccessException("The caller is not from microsoft tenant");
            }

            base.OnActionExecuting(context);
        }

        private (string upn, string appid) ParseJwtToken(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return (null, null);
            }
            if (!authHeader.StartsWith("bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return (null, null);
            }

            var accessToken = authHeader.Substring("bearer ".Length);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadToken(accessToken) as JwtSecurityToken;
            var appIdClaim = jwt?.Claims.FirstOrDefault(c => c.Type.Equals("appid", StringComparison.OrdinalIgnoreCase));
            var upnClaim = jwt?.Claims.FirstOrDefault(c => c.Type.Equals("upn", StringComparison.OrdinalIgnoreCase));
            return (upnClaim?.Value, appIdClaim?.Value);
        }
    }

    public class AllowedAppSettings
    {
        public List<string> WhiteListedAppIds { get; set; }
    }
}