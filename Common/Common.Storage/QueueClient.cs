// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueClient.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Storage.Queues;
    using Azure.Storage.Queues.Models;
    using Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class QueueClient<T> : IQueueClient<T> where T : class, new()
    {
        private readonly QueueClient client;
        private readonly QueueClient deadLetterQueueClient;
        private readonly ILogger<QueueClient<T>> logger;
        private readonly QueueSettings queueSettings;

        public QueueClient(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<QueueClient<T>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            queueSettings = configuration.GetConfiguredSettings<QueueSettings>();
            var clientFactory = new QueueClientFactory(serviceProvider, loggerFactory);
            client = clientFactory.QueueClient;
            if (client == null) throw new Exception("Failed to access storage queue");

            deadLetterQueueClient = clientFactory.DeadLetterQueueClient;
            if (deadLetterQueueClient == null) throw new Exception("Dead letter queue client is not provisioned");
        }

        public async Task<SendReceipt> Enqueue(T message, CancellationToken cancellationToken)
        {
            var queueMessage = JsonConvert.SerializeObject(message);
            var receipt = await client.SendMessageAsync(queueMessage, cancellationToken);
            return receipt;
        }

        public async Task<List<MessageFromQueue<T>>> Dequeue(int maxMessages, TimeSpan visibilityTimeout,
            CancellationToken cancellationToken)
        {
            var response = await client.ReceiveMessagesAsync(maxMessages, visibilityTimeout, cancellationToken);
            var messages = response.Value;
            var messageList = messages.Select(m => new MessageFromQueue<T>
                {
                    Value = JsonConvert.DeserializeObject<T>(m.MessageText),
                    Receipt = m.PopReceipt,
                    MessageId = m.MessageId,
                    DequeueCount = (int) m.DequeueCount
                })
                .ToList();
            logger.LogInformation(
                $"total of {messageList.Count} messages found from queue '{queueSettings.QueueName}'");

            var messagesToRemove = messageList.Where(m => m.DequeueCount >= queueSettings.MaxDequeueCount).ToList();
            if (messagesToRemove.Count > 0)
                foreach (var deadMsg in messagesToRemove)
                {
                    var jsonMsg = JsonConvert.SerializeObject(deadMsg.Value);
                    await deadLetterQueueClient.SendMessageAsync(jsonMsg, cancellationToken);
                    messageList.Remove(deadMsg);
                    logger.LogInformation(
                        $"message exceed retry count {deadMsg.DequeueCount} is over {queueSettings.MaxDequeueCount}, moved to dead letter queue: {queueSettings.DeadLetterQueueName}");
                }

            return messageList;
        }

        public async Task<bool> Peek(CancellationToken cancellationToken)
        {
            var peekResponse = await client.PeekMessagesAsync(cancellationToken: cancellationToken);
            return peekResponse.Value.Any();
        }

        public async Task ResetVisibility(string messageId, string receipt, T message,
            CancellationToken cancellationToken)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            await client.UpdateMessageAsync(
                messageId,
                receipt,
                messageJson,
                TimeSpan.FromSeconds(10),
                cancellationToken);
        }

        public async Task<int> GetQueueLength(CancellationToken cancellationToken)
        {
            var props = await client.GetPropertiesAsync(cancellationToken);
            return props.Value.ApproximateMessagesCount;
        }

        public async Task DeleteMessage(string messageId, string receipt, CancellationToken cancellationToken)
        {
            await client.DeleteMessageAsync(messageId, receipt, cancellationToken);
        }
    }
}