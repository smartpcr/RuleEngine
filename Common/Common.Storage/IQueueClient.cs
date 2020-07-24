// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IQueueClient.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Queues.Models;

    public interface IQueueClient<T>
    {
        Task<SendReceipt> Enqueue(T message, CancellationToken cancellationToken);

        Task<List<MessageFromQueue<T>>> Dequeue(int maxMessages, TimeSpan visibilityTimeout,
            CancellationToken cancellationToken);

        Task<bool> Peek(CancellationToken cancellationToken);
        Task ResetVisibility(string messageId, string receipt, T message, CancellationToken cancellationToken);
        Task<int> GetQueueLength(CancellationToken cancellationToken);
        Task DeleteMessage(string messageId, string receipt, CancellationToken cancellationToken);
    }

    public class MessageFromQueue<T>
    {
        public T Value { get; set; }
        public string MessageId { get; set; }
        public string Receipt { get; set; }
        public int DequeueCount { get; set; }
    }
}