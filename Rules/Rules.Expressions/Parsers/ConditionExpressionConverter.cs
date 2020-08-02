﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConditionExpressionConverter.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Rules.Expressions.Parsers
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Rules.Expressions;

    internal class ConditionExpressionConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(Resources.WriteJsonNotImplementedException);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IConditionExpression).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var jsonObject = JObject.Load(reader);
            if (DoesValueExist(jsonObject, nameof(AllOfExpression.AllOf)))
                return GetExpression<AllOfExpression>(jsonObject, serializer);

            if (DoesValueExist(jsonObject, nameof(AnyOfExpression.AnyOf)))
                return GetExpression<AnyOfExpression>(jsonObject, serializer);

            if (DoesValueExist(jsonObject, nameof(NotExpression.Not)))
                return GetExpression<NotExpression>(jsonObject, serializer);

            if (DoesValueExist(jsonObject, nameof(LeafExpression.Left)) &&
                DoesValueExist(jsonObject, nameof(LeafExpression.Operator)))
                return GetExpression<LeafExpression>(jsonObject, serializer);

            throw new FormatException("Expression provided does NOT contain the required fields for any of the defined condition expressions.");
        }

        private static bool DoesValueExist(JObject jsonObject, string valueName)
        {
            return jsonObject.GetValue(valueName, StringComparison.OrdinalIgnoreCase) != null;
        }

        private T GetExpression<T>(JObject jsonObject, JsonSerializer serializer) where T : IConditionExpression, new()
        {
            var expression = new T();
            serializer.Populate(jsonObject.CreateReader(), expression);
            return expression;
        }
    }
}