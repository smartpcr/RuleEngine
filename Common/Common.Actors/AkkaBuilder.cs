// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AkkaBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Actors
{
    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Extensions.DependencyInjection;
    using Common.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class AkkaBuilder
    {
        /// <summary>
        /// all dependency should already been added to <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services"></param>
        public static void AddAkkaActorSystem(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var akkaSettings = configuration.GetConfiguredSettings<AkkaSettings>();
            var akkaConfig = configuration.GetSection("Akka").Get<AkkaConfig>();
            var config = ConfigurationFactory.FromObject(new {akka = akkaConfig});
            var actorSystem = ActorSystem.Create(akkaSettings.Name, config);
            services.AddSingleton(actorSystem);
            serviceProvider = services.BuildServiceProvider();
            actorSystem.UseServiceProvider(serviceProvider);
        }
    }
}