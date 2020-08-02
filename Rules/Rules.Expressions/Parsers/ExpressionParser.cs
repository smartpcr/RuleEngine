// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionParser.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Parsers
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Rules.Expressions;

    public class ExpressionParser
    {
        private static readonly JsonSerializerSettings MediaTypeFormatterSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,

            Converters = new List<JsonConverter>
            {
                new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()},
                new ConditionExpressionConverter()
            }
        };

        private static readonly JsonSerializer JsonMediaTypeSerializer =
            JsonSerializer.Create(MediaTypeFormatterSettings);

        public IConditionExpression Parse(JToken rawFilter)
        {
            return rawFilter?.ToObject<IConditionExpression>(JsonMediaTypeSerializer);
        }
    }
}