// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyExtension.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;

    public static class PropertyExtension
    {
        public static PropertyInfo GetMappedProperty(this Type type, string fieldName)
        {
            var properties = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.GetProperty |
                BindingFlags.GetField);
            var match = properties.SingleOrDefault(info =>
            {
                var propertyName = info.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? info.Name;
                return string.Equals(propertyName, fieldName, StringComparison.OrdinalIgnoreCase);
            });
            return match;
        }
    }
}