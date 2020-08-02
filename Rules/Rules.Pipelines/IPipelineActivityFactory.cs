namespace Rules.Engines
{
    using System;
    using System.Threading.Tasks.Dataflow;

    public enum PipelineActivityType
    {
        Producer,
        Filter,
        Transform,
        Broadcast,
        Batch,
        Action
    }

    public interface IPipelineActivityFactory
    {
        Type InputType { get; }
        Type OutputType { get; }
        PipelineActivityType ActivityType { get; }
        IDataflowBlock CreateActivity();
    }
    
    public interface IProducerActivityFactory<TPayload> : IPipelineActivityFactory
    {
        ISourceBlock<TPayload> CreateActivity();
    }
    
    public interface IBatcherActivityFactory<TPayload> : IPipelineActivityFactory
    {
        IPropagatorBlock<TPayload, TPayload[]> CreateActivity();
    }
    
    public interface IBroadcastActivityFactory<TPayload> : IPipelineActivityFactory
    {
        IPropagatorBlock<TPayload, TPayload> CreateActivity();
    }
    
    public interface ITransformActivityFactory<TInput, TOutput> : IPipelineActivityFactory
    {
        IPropagatorBlock<TInput, TOutput> CreateActivity();
    }
    
    public interface IPerstenceActivityFactory<TPayload> : IPipelineActivityFactory
    {
        ITargetBlock<TPayload> CreateActivity();
    }
}