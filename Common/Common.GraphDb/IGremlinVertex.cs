// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVertex.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.GraphDb
{
    using System.Collections.Generic;
    using System.Reflection;

    public interface IGremlinVertex
    {
        string Id { get; set; }
        string PartitionKey { get; set; }
        string Label { get; set; }
        List<PropertyInfo> GetFlattenedProperties();
        Dictionary<string, string> GetPropertyValues(List<PropertyInfo> props);
    }
}