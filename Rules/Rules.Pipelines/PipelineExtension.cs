// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations
{
    using Batches;
    using Broadcasts;
    using Common.Storage;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using Persistence;
    using Pipelines;
    using Producers;
    using Transformers;

    public static class PipelineExtension
    {
        public static IServiceCollection AddPipelines(this IServiceCollection services, IConfiguration configuration)
        {
            // trigger
            services.AddSingleton<IQueueClient<DeviceValidationJob>, QueueClient<DeviceValidationJob>>();

            // producer
            services.AddSingleton<IContextEnricher<PowerDevice>, DeviceRelationEnricher>();
            services.AddSingleton<IContextEnricher<PowerDevice>, DeviceDataPointEnricher>();
            services.AddSingleton<IContextEnricher<PowerDevice>, DeviceRawDataStatsEnricher>();
            services.AddSingleton<IContextEnricher<PowerDevice>, DevicePathEnricher>();
            services.AddSingleton<ContextEnricherFactory<PowerDevice>>();
            services.AddSingleton<IContextProvider<PowerDevice>, DeviceContextProvider>();
            services.AddSingleton<
                IPayloadProducer<DeviceValidationPayload, DeviceValidationJob>,
                DeviceValidationPayloadProducer>();
            services.AddSingleton<
                IPayloadProducer<PowerDevice, DeviceValidationJob>,
                PowerDeviceProducer>();
            services.AddSingleton<PayloadProducerFactory<PowerDevice, DeviceValidationJob>>();

            // broadcast
            services.AddSingleton<
                IPayloadBroadcast<DeviceValidationPayload>,
                ValidationPayloadBroadcast>();
            services.AddSingleton<
                IPayloadBroadcast<PowerDevice>,
                PowerDevicePayloadBroadcast>();

            // transformer
            services.AddSingleton<IPayloadTransformer<DeviceValidationPayload, DeviceValidationResult, DeviceValidationJob>, PowerDeviceEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, DeviceInCircularPathEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, KwTotReadingMatchChildrenEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, LeafDeviceParentHierarchyEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, PowersourceDeviceHierarchyEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, PowerDeviceSumOfCurrentEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, PowerDeviceVoltageComparer>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, StalenessEvaluator>();
            services.AddSingleton<IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>, CompletenessEvaluator>();
            services.AddSingleton<PayloadTransformerFactory<DeviceValidationPayload, DeviceValidationResult, DeviceValidationJob>>();
            services.AddSingleton<PayloadTransformerFactory<PowerDevice, DeviceValidationResult, DeviceValidationJob>>();

            // batch
            services.AddSingleton<
                IPayloadBatcher<DeviceValidationResult>,
                PowerDeviceValidationResultBatcher>();

            // save
            services.AddSingleton<
                IPayloadPersistence<DeviceValidationResult[], DeviceValidationJob>,
                SaveToCosmosDb>();
            services.AddSingleton<
                IPayloadPersistence<DeviceValidationResult[], DeviceValidationJob>,
                SaveToKusto>();
            services.AddSingleton<PayloadPersistenceFactory<DeviceValidationResult[], DeviceValidationJob>>();

            // pipeline
            services.AddSingleton<IPipelineFactory, PipelineFactory>();
            services.AddSingleton<
                IPipeline<DeviceValidationPayload, PipelineExecutionContext, DeviceValidationJob>,
                JsonRulePipeline>();
            services.AddSingleton<
                IPipeline<PowerDevice, PipelineExecutionContext, DeviceValidationJob>,
                CodeRulePipeline>();
            services.AddSingleton<
                IPipeline<DataCenterValidationPayload, PipelineExecutionContext, DataCenterValidationJob>,
                DataCenterPipeline>();

            // export
            services.AddSingleton<IExportValidationResultToKusto, ExportValidationResultToKusto>();

            // validator
            services.AddSingleton<IValidator, Validator>();

            return services;
        }
    }
}