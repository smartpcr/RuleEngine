using System;
using System.Collections.Generic;

namespace Common.Batch
{
    public class BatchSetting<TField> where TField : IComparable
    {
        public string Name { get; set; }
        public string SortField { get; set; }
        public long Total { get; set; }
        public int BatchSize { get; set; }
        public TField Start { get; set; }
        public TField End { get; set; }
        public int Concurrency { get; set; } = 1;
        public TimeSpan LeaseTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }

}