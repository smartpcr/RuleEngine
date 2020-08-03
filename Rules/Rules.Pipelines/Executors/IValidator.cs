// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IValidator.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Executors
{
    using System.Threading;
    using System.Threading.Tasks;
    using DataCenterHealth.Models.Jobs;

    public interface IValidator
    {
        Task<DeviceValidationRun> Validate(DeviceValidationJob job, DeviceValidationRun run, CancellationToken cancel);
    }
}