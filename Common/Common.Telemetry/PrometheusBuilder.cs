// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrometheusBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Telemetry
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Prometheus.Client.AspNetCore;
    using Prometheus.Client.MetricServer;

    public static class PrometheusBuilder
    {
        /// <summary>
        ///     this is used in web host
        /// </summary>
        /// <param name="app"></param>
        /// <param name="settings"></param>
        public static void UsePrometheus(this IApplicationBuilder app,
            PrometheusMetricSettings settings)
        {
            app.UsePrometheusServer(options =>
            {
                options.UseDefaultCollectors = true;
                options.MapPath = settings.Route;
            });
        }

        /// <summary>
        ///     this is used in console (GenericHost) app
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        public static void UsePrometheus(this IServiceCollection services, PrometheusMetricSettings settings)
        {
            var metricServer = new MetricServer(null, new MetricServerOptions
            {
                Port = settings.Port,
                MapPath = settings.Route,
                Host = "localhost"
            });
            metricServer.Start();
        }

        /// <summary>
        ///     TODO: use prometheus server deployed in the cluster
        /// </summary>
        /// <param name="serices"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IServiceCollection AddPrometheusPushGateway(this IServiceCollection serices,
            IConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}