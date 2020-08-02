namespace Rules.Validations
{
    using System.Threading;
    using System.Threading.Tasks;
    using DataCenterHealth.Models.Jobs;

    public interface IValidator
    {
        Task<DeviceValidationRun> ValidateDevices(
            DeviceValidationJob job,
            DeviceValidationRun run,
            CancellationToken cancel);

        Task<DataCenterValidationRun> ValidateDataCenter(
            DataCenterValidationJob job,
            DataCenterValidationRun run,
            CancellationToken cancel);
    }
}