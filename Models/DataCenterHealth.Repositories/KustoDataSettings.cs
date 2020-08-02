namespace DataCenterHealth.Repositories
{
    using Common.Kusto;
    using DataCenterHealth.Entities.Devices;
    using DataCenterHealth.Models;
    using DataCenterHealth.Models.Jobs;
    using DataCenterHealth.Models.PowerServices;
    using Models.Devices;

    public class KustoDataSettings
    {
        [MappedModel(typeof(ZenonEventStats))] public KustoSettings ZenonEventStats { get; set; }

        [MappedModel(typeof(ZenonRawEvent))] public KustoSettings ZenonRawEvent { get; set; }

        [MappedModel(typeof(ZenonDataPoint))] public KustoSettings ZenonDataPoints { get; set; }

        [MappedModel(typeof(ZenonLastReading))] public KustoSettings ZenonLastReadings { get; set; }
        [MappedModel(typeof(DeviceValidationResult))] public KustoSettings DeviceValidationResult { get; set; }
        [MappedModel(typeof(ArgusEnabledDc))] public KustoSettings ArgusEnabledDc { get; set; }

        #region power service
        [MappedModel(typeof(ArgusEnabledDcCode))] public KustoSettings ArgusEnabledDcCode { get; set; }
        #endregion
    }
}