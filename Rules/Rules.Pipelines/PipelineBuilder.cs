// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineBuilder.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines
{
    using Common.Config;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Validation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Rules.Pipelines.Batches;
    using Rules.Pipelines.Persistence;
    using Rules.Pipelines.Producers;
    using Rules.Pipelines.Producers.Enrichers;
    using Rules.Pipelines.Transformer;

    public static class PipelineBuilder
    {
        public static void AddPowerDevicePipeline(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            
            // producer
            services.AddSingleton<IContextEnricher<PowerDevice>, DeviceRelationEnricher>();
            services.AddSingleton<IContextEnricher<PowerDevice>, DataPointsEnricher>();
            services.AddSingleton<IContextEnricher<PowerDevice>, ZenonEventsEnricher>();
            services.AddSingleton<IContextEnricher<PowerDevice>, DevicePathEnricher>();
            services.AddSingleton<ContextEnricherFactory<PowerDevice>>();
            services.AddSingleton<IContextProvider<PowerDevice>, DeviceContextProvider>();
            services.AddSingleton<IPayloadProducer<PowerDevice>, DevicePayloadProducer>();
            
            // transformer 
            services.AddSingleton<IPayloadTransformer<PowerDevice>, DeviceEvaluator>();
            
            // batch 
            services.AddSingleton<IPayloadBatcher<EvaluationResult>, ValidationResultBatcher>();
            
            // persistence
            services.AddSingleton<IPayloadPersistence<EvaluationResult>, ValidationResultPersistence>();
        }
    }
}