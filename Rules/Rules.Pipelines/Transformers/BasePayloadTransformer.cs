// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasePayloadTransformer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Transformers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;
    using Common.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines;

    public abstract class
        BasePayloadTransformer<TInput, TOutput, TConfig> : IPayloadTransformer<TInput, TOutput, TConfig>
        where TInput : class, new()
        where TOutput : class, new()
        where TConfig : class, new()
    {
        protected BasePayloadTransformer(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Settings = configuration.GetConfiguredSettings<PipelineSettings>();
        }

        protected PipelineSettings Settings { get; }

        public PipelineActivityType ActivityType => PipelineActivityType.Transform;
        public string Name => GetType().Name;

        public IPropagatorBlock<TInput, TOutput> CreateTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            var totalReceived = 0;
            var transformBlock = new TransformBlock<TInput, TOutput>(
                x =>
                {
                    Interlocked.Increment(ref totalReceived);
                    if (totalReceived % 100 == 0)
                        LogInformation($"total of {totalReceived} events are received by {GetType().Name}");
                    context.AddTotalReceived(1);

                    var result = Transform(x, context);
                    return result;
                },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = Settings.MaxBufferCapacity,
                    MaxDegreeOfParallelism = Settings.MaxParallelism,
                    CancellationToken = cancellationToken
                });

            return transformBlock;
        }

        public ISourceBlock<TOutput> CreateFirstTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ITargetBlock<TInput> CreateFinalTask(PipelineExecutionContext context,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected abstract TOutput Transform(TInput input, PipelineExecutionContext context);

        protected abstract void LogInformation(string message);
    }
}