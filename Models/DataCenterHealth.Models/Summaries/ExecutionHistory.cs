// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionHistory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Summaries
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ExecutionHistory : BaseEntity
    {
        [JsonConverter(typeof(StringEnumConverter), true)]
        public ExecutionType Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter), true)]
        public ExecutionStatus Status { get; set; }

        public string ExecutedByUser { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public static ExecutionHistory Create<T>(ExecutionType type, string title, string user)
        {
            return new ExecutionHistory()
            {
                Type = type,
                Status = ExecutionStatus.Success,
                ExecutedByUser = user,
                ExecutionTime = DateTime.UtcNow,
                Title = title,
                Description = string.Empty
            };
        }
    }

    public enum ExecutionType
    {
        Deployment,
        MetaDataSync,
        OnDemandValidation
    }

    public enum ExecutionStatus
    {
        Success,
        Error,
        Warning,
        Cancelled
    }
}