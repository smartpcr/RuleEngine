namespace Rules.Expressions.Tests.TestModels.IoT
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeviceType
    {
        Unknown,
        Breaker,
        Transformer,
        Switch,
        Generator,
        STS,
        UPS,
        AHU
    }
}