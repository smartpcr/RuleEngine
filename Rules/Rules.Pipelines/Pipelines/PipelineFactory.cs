// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineFactory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Pipelines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using Batches;
    using Broadcasts;
    using Common.Config;
    using DataCenterHealth.Models.Devices;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.Rules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Persistence;
    using Producers;
    using Transformers;

    public enum PipelineType
    {
        JsonRulePipeline,
        CodeRulePipeline
    }

    public class PipelineFactory : IPipelineFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILoggerFactory loggerFactory;
        private readonly IPayloadBatcher<DeviceValidationResult> batcher;
        private readonly IPayloadBroadcast<PowerDevice> broadcast;

        private readonly Dictionary<ContextErrorCode, IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>> codeRuleTransformers =
            new Dictionary<ContextErrorCode, IPayloadTransformer<PowerDevice, DeviceValidationResult, DeviceValidationJob>>();

        private readonly ILogger<PipelineFactory> logger;
        private readonly IPayloadPersistence<DeviceValidationResult[], DeviceValidationJob> persistence;
        private readonly IPayloadProducer<DeviceValidationPayload, DeviceValidationJob> producer;
        private readonly IPayloadProducer<PowerDevice, DeviceValidationJob> producer2;
        private readonly PipelineSettings settings;
        private readonly IPayloadTransformer<DeviceValidationPayload, DeviceValidationResult, DeviceValidationJob> transformer;

        public PipelineFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<PipelineFactory>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            settings = configuration.GetConfiguredSettings<PipelineSettings>();

            producer = serviceProvider
                .GetRequiredService<IPayloadProducer<DeviceValidationPayload, DeviceValidationJob>>();
            var transformerFactory = serviceProvider
                .GetRequiredService<PayloadTransformerFactory<
                    DeviceValidationPayload,
                    DeviceValidationResult,
                    DeviceValidationJob>>();
            transformer = transformerFactory.GetTransformer(serviceProvider, nameof(PowerDeviceEvaluator));
            batcher = serviceProvider.GetRequiredService<IPayloadBatcher<DeviceValidationResult>>();
            var persistenceFactory = serviceProvider
                .GetRequiredService<PayloadPersistenceFactory<DeviceValidationResult[], DeviceValidationJob>>();
            persistence = persistenceFactory.GetPersistenceActivity(serviceProvider, nameof(SaveToCosmosDb));

            producer2 = serviceProvider
                .GetRequiredService<IPayloadProducer<PowerDevice, DeviceValidationJob>>();
            broadcast = serviceProvider
                .GetRequiredService<IPayloadBroadcast<PowerDevice>>();
            var transformerFactory2 = serviceProvider
                .GetRequiredService<PayloadTransformerFactory<
                    PowerDevice,
                    DeviceValidationResult,
                    DeviceValidationJob>>();

            var codeRuleEvalators = GetCodeRuleEvaluators();
            foreach (var codeRuleEval in codeRuleEvalators)
            {
                var codeRuleTransformer = transformerFactory2.GetTransformer(serviceProvider, codeRuleEval.GetType().Name);
                codeRuleTransformers.Add(codeRuleEval.CodeRuleErrorCode, codeRuleTransformer);
            }
        }

        public IList<(PipelineActivityType activityType, IDataflowBlock block)> CreateJsonRulePipeline(
            PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            var producerBlock = producer.CreateFirstTask(context, cancellationToken);
            var transformBlock = transformer.CreateTask(context, cancellationToken);
            var batcherBlock = batcher.CreateTask(context, cancellationToken);
            var persistenceBlock = persistence.CreateFinalTask(context, cancellationToken);

            var linkOptions = new DataflowLinkOptions {PropagateCompletion = settings.PropagateCompletion};
            producerBlock.LinkTo(transformBlock, linkOptions);
            transformBlock.LinkTo(batcherBlock, linkOptions); // batchblock doesn't allow predicate
            batcherBlock.LinkTo(persistenceBlock, linkOptions);

            var blocks = new List<(PipelineActivityType activityType, IDataflowBlock block)>
            {
                (PipelineActivityType.Producer, producerBlock),
                (PipelineActivityType.Transform, transformBlock),
                (PipelineActivityType.Batch, batcherBlock),
                (PipelineActivityType.Action, persistenceBlock)
            };

            logger.LogInformation($"total of {blocks.Count} blocks are produced for the pipeline");

            return blocks;
        }

        public IList<(PipelineActivityType activityType, IDataflowBlock block)> CreateCodeRulePipeline(
            PipelineExecutionContext context,
            List<CodeRule> codeRules,
            CancellationToken cancellationToken)
        {
            var linkOptions = new DataflowLinkOptions {PropagateCompletion = settings.PropagateCompletion};
            var blocks = new List<(PipelineActivityType activityType, IDataflowBlock block)>();

            var producerBlock = producer2.CreateFirstTask(context, cancellationToken);
            blocks.Add((PipelineActivityType.Producer, producerBlock));
            var allErrorCodes = codeRules.Select(r => r.ErrorCode).ToList();
            var allTransformers = codeRuleTransformers.Where(p => allErrorCodes.Contains(p.Key)).Select(p => p.Value).ToList();
            broadcast.TotalConsumers = allTransformers.Count;
            var broadcastBlock = broadcast.CreateTask(context, cancellationToken);
            producerBlock.LinkTo(broadcastBlock, linkOptions);
            blocks.Add((PipelineActivityType.Broadcast, broadcastBlock));

            for (var i = 0; i < allTransformers.Count; i++)
            {
                var transformerProducer = allTransformers[i];
                var transformBlock = transformerProducer.CreateTask(context, cancellationToken);
                var routeKey = i + 1;
                broadcastBlock.LinkTo(transformBlock, linkOptions, payload => payload.RouteKey == routeKey);
                blocks.Add((PipelineActivityType.Transform, transformBlock));

                var batchBlock = batcher.CreateTask(context, cancellationToken);
                transformBlock.LinkTo(batchBlock, linkOptions);
                blocks.Add((PipelineActivityType.Batch, batchBlock));

                var persistenceBlock = persistence.CreateFinalTask(context, cancellationToken);
                batchBlock.LinkTo(persistenceBlock, linkOptions);
                blocks.Add((PipelineActivityType.Action, persistenceBlock));
            }
            logger.LogInformation($"total of {blocks.Count} blocks are produced for the pipeline");

            return blocks;
        }

        public IList<(PipelineActivityType activityType, IDataflowBlock block)> CreateDataCenterPipeline(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private List<CodeRuleEvaluator> GetCodeRuleEvaluators()
        {
            var codeRuleEvaluators = new List<CodeRuleEvaluator>();
            var codeRuleEvaluatorTypes = typeof(CodeRuleEvaluator).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CodeRuleEvaluator)) && !t.IsAbstract)
                .ToList();
            foreach (var codeEvalType in codeRuleEvaluatorTypes)
            {
                if (Activator.CreateInstance(codeEvalType, serviceProvider, loggerFactory) is CodeRuleEvaluator codeEval)
                {
                    codeRuleEvaluators.Add(codeEval);
                }
            }

            return codeRuleEvaluators;
        }
    }
}