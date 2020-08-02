// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestHelper.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Tests
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static class TestHelper
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None,
            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()},
            }
        };
        
        public static string FormatObject(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented, serializerSettings);
        }
    }
}