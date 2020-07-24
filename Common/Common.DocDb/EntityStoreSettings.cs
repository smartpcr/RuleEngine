// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CosmosData.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.DocDb
{
    public class EntityStoreSettings
    {
        public string Db { get; set; }
        public string Collection { get; set; }
        public string PartitionKey { get; set; }
        public string[] UniqueKey { get; set; }
        public string TypeField { get; set; }
        public string TypeValue { get; set; }

        public string BuildQuery(string filterClause = null)
        {
            if (!string.IsNullOrEmpty(filterClause))
                return !string.IsNullOrEmpty(TypeField) && !string.IsNullOrEmpty(TypeValue)
                    ? $"select * from c where c.{TypeField} == \"{TypeValue}\" and {filterClause}"
                    : $"select * from c where {filterClause}";
            return !string.IsNullOrEmpty(TypeField) && !string.IsNullOrEmpty(TypeValue)
                ? $"select * from c where c.{TypeField} == \"{TypeValue}\""
                : "select * from c";
        }

        public string BuildQuery(string filterClause, string sortField, bool isDenscending, int skip, int take)
        {
            var query = !string.IsNullOrEmpty(TypeField) && !string.IsNullOrEmpty(TypeValue)
                ? $"select * from c where c.{TypeField} == \"{TypeValue}\" and {filterClause}"
                : $"select * from c where {filterClause}";

            var sortDirection = isDenscending ? "desc" : "asc";
            return $"{query} order by c.{sortField} {sortDirection} offset {skip} limit {take}";
        }
    }
}