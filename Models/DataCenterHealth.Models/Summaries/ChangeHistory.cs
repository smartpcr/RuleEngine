// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeHistory.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Summaries
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ChangeHistory : BaseEntity
    {
        private DateTime changeTime;

        [JsonConverter(typeof(StringEnumConverter), true)]
        public ChangeType ChangeType { get; set; }
        [JsonConverter(typeof(StringEnumConverter), true)]
        public ChangeStatus Status { get; set; }
        [JsonConverter(typeof(StringEnumConverter), true)]
        public ChangeOperation Operation { get; set; }
        public DateTime ChangeTime
        {
            get => changeTime;
            set
            {
                changeTime = value;
                if (changeTime != default)
                {
                    Ago = changeTime.ToFriendlyName();
                }
            }
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ChangedByUser { get; set; }
        [JsonIgnore]
        public string Ago { get; set; }
        public string EntityId { get; set; }

        public static ChangeHistory Create<T>(ChangeOperation operation, ChangeType type, string entityId, string title, string user)
        {
            return new ChangeHistory()
            {
                ChangeTime = DateTime.UtcNow,
                Status = ChangeStatus.Success,
                ChangeType = type,
                Operation = operation,
                Title = title,
                Description = $"{title} by {user}",
                ChangedByUser = user,
                EntityId = entityId
            };
        }
    }

    public enum ChangeType
    {
        MetaData,
        ValidationRule,
        ValidationSchedule,
        SyncSettings
    }

    public enum ChangeStatus
    {
        Success,
        Error,
        Warning,
        Info,
        Message,
        cancelled
    }

    public enum ChangeOperation
    {
        Create,
        Update,
        Delete
    }

    public class GroupedChangeHistory
    {
        public string Ago { get; set; }
        public long Timestamp { get; set; }
        public List<ChangeHistory> Histories { get; set; }
    }
}