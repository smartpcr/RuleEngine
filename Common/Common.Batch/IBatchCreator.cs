// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBatcher.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Batch
{
    public interface IBatchCreator<TField> where TField : IComparable
    {
        Task GenerateBatchesIfNotExist();
        Task<int> GetTotalProcessed();
        Task<int> GetTotalInProgress();
        Task<int> GetTotalInQueue();
        Task<IEnumerable<Batch<TField>>> Pickup(string consumer, int count = 1);
        Task Fail(Batch<TField> batch);
        Task Succeed(Batch<TField> batch);
    }
}