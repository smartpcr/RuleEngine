// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CosmosReaderSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataCenterHealth.Models.Sync
{
    using Common.DocDb;

    public class CosmosReaderSettings
    {
        public DocDbSettings DocDb { get; set; }
        public string CountBy { get; set; }
    }
}