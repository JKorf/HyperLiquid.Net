using CryptoExchange.Net;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HyperLiquid.Net.Converters
{
    internal class ArrayDictConverter<T> : JsonConverter<Dictionary<string, T>>
    {
        public override Dictionary<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, T>();
            reader.Read(); // Start array

            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read(); // Start array
                var token = reader.GetString();
                reader.Read();
                var value = JsonSerializer.Deserialize<T>(ref reader, (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T)));
                result.Add(token!, value!);
                reader.Read(); // End array
            }

            reader.Read(); // End array
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var kvp in value)
            {
                writer.WriteStartArray();
                writer.WriteStringValue(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T)));
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}
