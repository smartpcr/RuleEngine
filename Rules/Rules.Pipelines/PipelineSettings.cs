// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PipelineSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines
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
        public TimeSpan ProcessTimeout { get; set; }
    }
}