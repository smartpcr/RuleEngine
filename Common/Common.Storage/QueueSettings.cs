// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueSettings.cs" company="Microsoft">
// </copyright>
// <summary>
//
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Storage
{
    public class QueueSettings
    {
        public string Account { get; set; }
        public string QueueName { get; set; }
        public string ConnectionStringSecretName { get; set; }
        public string ConnectionStringEnvName { get; set; }
        public int MaxDequeueCount { get; set; }
        public string DeadLetterQueueName { get; set; }
        public string AccountServiceUrl => $"https://{Account}.queue.core.windows.net";
        public StorageAuthMode AuthMode { get; set; } = StorageAuthMode.Msi;
    }
}