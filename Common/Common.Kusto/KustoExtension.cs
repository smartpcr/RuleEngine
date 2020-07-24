namespace Common.Kusto
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using global::Kusto.Cloud.Platform.Utils;
    using global::Kusto.Data.Common;
    using Newtonsoft.Json;

    public static class KustoExtension
    {
        public static string ToKustoColumnType(this Type type)
        {
            if (type == typeof(string)) return "string";
            if (type.IsEnum) return "string";
            if (type == typeof(bool) || type == typeof(bool?)) return "bool";
            if (type == typeof(DateTime) || type == typeof(DateTime?)) return "datetime";
            if (type == typeof(Guid) || type == typeof(Guid?)) return "guid";
            if (type == typeof(int) || type == typeof(byte) || type == typeof(int?)) return "int";
            if (type == typeof(long) || type == typeof(long?)) return "long";
            if (type == typeof(decimal) || type == typeof(decimal?) || type == typeof(float) ||
                type == typeof(float?) || type == typeof(double) || type == typeof(double?)) return "real";
            if (type == typeof(TimeSpan)) return "timespan";
            if (!type.IsScalar()) return "dynamic";
            return "string";
        }

        public static List<(JsonColumnMapping mapping, Type fieldType)> GetKustoColumnMappings(this Type type)
        {
            var columnMappings = new List<(JsonColumnMapping mapping, Type fieldType)>();
            foreach (var prop in type.GetProperties())
            {
                var jsonPropAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var propName = jsonPropAttr == null ? prop.Name : jsonPropAttr.PropertyName;
                var kustoFieldType = prop.PropertyType.ToKustoColumnType();
                var propType = prop.PropertyType.GetTypeWithNullableSupport();
                if (propType == typeof(decimal)) propType = typeof(double);

                if (prop.PropertyType == typeof(string[]) || prop.PropertyType == typeof(List<string>))
                    columnMappings.Add((new JsonColumnMapping
                    {
                        ColumnName = propName,
                        ColumnType = kustoFieldType,
                        JsonPath = "$." + propName,
                        TransformationMethod = TransformationMethod.PropertyBagArrayToDictionary
                    }, typeof(string)));
                else
                    columnMappings.Add((new JsonColumnMapping
                    {
                        ColumnName = propName,
                        ColumnType = kustoFieldType,
                        JsonPath = "$." + propName
                    }, propType));
            }

            return columnMappings;
        }

        private static Type GetTypeWithNullableSupport(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) return nullableType;
            return type;
        }
    }
}