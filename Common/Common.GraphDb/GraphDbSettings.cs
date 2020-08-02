// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphDbSettings.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.GraphDb
{
    using System;

    public class GraphDbSettings
    {
        public Api Api { get; set; } = Api.SQL;
        public string Account { get; set; }
        public string Db { get; set; }
        public string Collection { get; set; }
        public string AuthKeySecret { get; set; }
        public bool CollectMetrics { get; set; }
        public Uri AccountUri => new Uri($"https://{Account}.documents.azure.com:443/");
    }

    public enum Api
    {
        SQL,
        Gremlin
    }
}