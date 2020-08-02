namespace Rules.Validations.Producers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Rules.Validations.Pipelines;

    public interface IContextProvider<T> : IDisposable where T : class, new()
    {
        IEnumerable<IContextEnricher<T>> ContextEnrichers { get; }
        Task<IEnumerable<T>> Provide(
            PipelineExecutionContext context,
            ContextProviderScope scope,
            List<string> filterValues,
            CancellationToken cancellationToken);
    }

    public enum ContextProviderScope
    {
        DC,
        Device
    }
}