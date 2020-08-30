// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnixEpochTimeConverter.cs" company="Microsoft Corporation">
//   Copyright (c) 2020 Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.DocDb
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Converts a <see cref="DateTime"/> to and from Unix epoch time
    /// </summary>
    public class UnixEpochTimeConverter : DateTimeConverterBase
    {
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long seconds;
            if (value is DateTime dateTime)
            {
                seconds = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
            }
            // else if (value is DateTimeOffset dateTimeOffset)
            // {
            //     seconds = (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
            // }
            else
            {
                throw new JsonSerializationException("Expected date object value.");
            }

            if (seconds < 0)
            {
                seconds = 0;
            }

            writer.WriteValue(seconds);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool nullable = Nullable.GetUnderlyingType(objectType) != null;
            if (reader.TokenType == JsonToken.Null)
            {
                if (!nullable)
                {
                    throw new JsonSerializationException($"Cannot convert null value to {objectType}.");
                }

                return null;
            }

            long seconds;
            if (reader.TokenType == JsonToken.Integer)
            {
                seconds = (long)reader.Value!;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                if (!long.TryParse((string)reader.Value!, out seconds))
                {
                    throw new JsonSerializationException($"Cannot convert invalid value {reader.Value} to {objectType}.");
                }
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token parsing date. Expected Integer or String, got {reader.TokenType}.");
            }

            if (seconds >= 0)
            {
                DateTime d = UnixEpoch.AddSeconds(seconds);
                // Type t = (nullable)
                //     ? Nullable.GetUnderlyingType(objectType)
                //     : objectType;
                return d;
            }

            return new DateTime(1970, 1, 1);
        }
    }
}