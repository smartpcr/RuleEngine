// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePayloadBroadcast.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Broadcasts
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using DataCenterHealth.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public abstract class BasePayloadBroadcast<TPayload> : IPayloadBroadcast<TPayload>
        where TPayload : class, IRoutable, new()
    {
        protected BasePayloadBroadcast(IServiceProvider serviceProvider, PipelineType pipelineType)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Settings = configuration.GetConfiguredSettings<PipelineSettings>();
            switch (pipelineType)
            {
                case PipelineType.CodeRulePipeline:
                    TotalConsumers = Settings.CodeRuleBroadcast.TotalConsumers;
                    break;
                default:
                    TotalConsumers = Settings.JsonRuleBroadcast.TotalConsumers;
                    break;
            }
        }

        protected PipelineSettings Settings { get; }
        public int TotalConsumers { get; set; }
        public PipelineActivityType ActivityType => PipelineActivityType.Broadcast;

        public IPropagatorBlock<TPayload, TPayload> CreateTask(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            var totalBroadcast = 0;

            return new TransformManyBlock<TPayload, TPayload>(payload =>
            {
                Interlocked.Increment(ref totalBroadcast);
                if (totalBroadcast % 100 == 0)
                    LogInformation($"total of {totalBroadcast} events are received by {GetType().Name}");

                context.AddTotalBroadcast(1);
                if (context.TotalBroadcast % Settings.MaxBufferCapacity == 0)
                    LogInformation($"total of {context.TotalBroadcast} events broadcasted");
                return Enumerable.Range(1, TotalConsumers)
                    .Select(id => payload.Clone().WithRouteKey(id) as TPayload).ToList();
            });
        }

        public ISourceBlock<TPayload> CreateFirstTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ITargetBlock<TPayload> CreateFinalTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected abstract void LogInformation(string message);
    }
}