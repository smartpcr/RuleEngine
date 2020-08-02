// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITrackExecution.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System.Threading.Tasks;
    using DataCenterHealth.Models.Summaries;

    public interface ITrackExecution<T> where T : class
    {
        bool EnableExecutionTracking { get; }
        ExecutionType ExecutionType { get; }
        Task TrackExecution(ExecutionHistory executionHistory);
    }
}