// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Batch.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Common.Batch
{
    public interface IRangeHelper<TField> where TField : IComparable
    {
        IList<(TField start, TField end)> Split(TField start, TField end, int size);
    }

    public class TimeRangeHelper : IRangeHelper<DateTime>
    {
        public IList<(DateTime start, DateTime end)> Split(DateTime start, DateTime end, int size)
        {
            var output = new List<(DateTime start, DateTime end)>();
            var span = (end - start) / size;
            var current = start;
            while (current <= end)
            {
                output.Add((current, current + span));
                current += span;
            }

            return output;
        }
    }
}