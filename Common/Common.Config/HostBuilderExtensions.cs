// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HostBuilderExtensions.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Common.Config
{
    using System;
    using System.Net;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    public static class HostBuilderExtensions
    {
        public static IHostBuilder AsJob(this IHostBuilder hostBuilder)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isProduction = string.IsNullOrEmpty(env) ||
                               string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);

            hostBuilder
                .UseEnvironment(isProduction ? "Production" : env)
                .UseConsoleLifetime()
                .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("appsettings.json", false, false);
                    if (!isProduction)
                    {
                        var overrides = env.Split('.', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var envOverride in overrides)
                            c.AddJsonFile($"appsettings.{envOverride}.json", true, false);
                    }

                    c.AddEnvironmentVariables();
                });
            SetResourceLimits();

            return hostBuilder;
        }

        private static void SetResourceLimits()
        {
            ThreadPool.SetMinThreads(100, 100);
            ConfigureServicePointManager();
        }

        private static void ConfigureServicePointManager()
        {
            ServicePointManager.DefaultConnectionLimit = 50;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }

    public interface IExecutor
    {
        Task ExecuteAsync(CancellationToken cancel);
    }
}