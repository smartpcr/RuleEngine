namespace Common.EventHub
{
    public class EventHubSettings
    {
        public string Namespace { get; set; }
        public string HubName { get; set; }
        public string Topic { get; set; }
        public string ConsumerGroup { get; set; }
        public string ConnectionStringSecretName { get; set; }
        public string ConnectionStringEnvName { get; set; }
    }
}