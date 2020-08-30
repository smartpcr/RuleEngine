namespace Models.IoT
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