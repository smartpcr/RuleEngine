namespace Rules.Validations.Pipelines
{
    using System;

    public class PipelineSettings
    {
        public int MaxDequeueCount { get; set; } = 3;
        public int MaxParallelJobs { get; set; } = 1;
        public int MaxParallelism { get; set; } = 8;
        public int MaxBufferCapacity { get; set; } = 10000;
        public int PersistenceBatchSize { get; set; } = 100;
        public int MaxRetryCount { get; set; } = 100;
        public TimeSpan WaitSpan { get; set; } = TimeSpan.FromSeconds(10);
        public bool PropagateCompletion { get; set; } = true;
        public BroadcastBlockSettings JsonRuleBroadcast { get; set; }
        public BroadcastBlockSettings CodeRuleBroadcast { get; set; }
        public TimeSpan ProcessTimeout { get; set; }
    }

    public class BroadcastBlockSettings
    {
        public int TotalConsumers { get; set; } = 3;
    }
}