// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITrackChange.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Repositories
{
    using System;
    using System.Threading.Tasks;
    using DataCenterHealth.Models.Summaries;

    public interface ITrackChange<T> where T : class
    {
        bool EnableChangeTracking { get; }
        ChangeType ChangeType { get; }
        string GetChangeHistoryTitle(T instance);
        Task TrackChange(ChangeHistory changeHistory);
    }
}