// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExportValidationResultToKusto.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Pipelines.Persistence
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExportValidationResultToKusto
    {
        Task<int> ExportToKusto(string runId, CancellationToken cancel);
    }
}