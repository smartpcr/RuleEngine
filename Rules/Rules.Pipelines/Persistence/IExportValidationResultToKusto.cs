// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExportValidationToKusto.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Validations.Persistence
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IExportValidationResultToKusto
    {
        Task<int> ExportToKusto(string runId, CancellationToken cancel);
    }
}