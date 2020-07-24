// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KustoBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Kusto
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class KustoBuilder
    {
        public static IServiceCollection AddKusto(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IKustoClient, KustoClient>();
            return services;
        }
    }
}