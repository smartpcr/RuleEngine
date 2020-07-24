// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultJsonSerializer.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.Config
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static class DefaultSettings
    {
        public static JsonSerializer JsonSerializer
        {
            get
            {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Converters.Add(new StringEnumConverter());
                jsonSerializer.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        OverrideSpecifiedNames = false
                    }
                };
                return jsonSerializer;
            }
        }

        public static JsonSerializerSettings SerializerSettings
        {
            get
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    MaxDepth = 3,
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };
                return serializerSettings;
            }
        }

        public static string ToCamelCase(this string name)
        {
            return string.IsNullOrEmpty(name) || name.Length <= 1
                ? name
                : name[0].ToString().ToLowerInvariant() + name.Substring(1);
        }
    }
}