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
    internal class TokenToConverter : JsonConverter<Dictionary<long, decimal>>
    {
        public override Dictionary<long, decimal> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Dictionary<long, decimal>();
            reader.Read(); // Start array

            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read(); // Start array
                var token = reader.GetInt64();
                reader.Read();
                var value = reader.GetString();
                result.Add(token, ExchangeHelpers.ParseDecimal(value) ?? 0);
                reader.Read();
                reader.Read(); // End array
            }

            //reader.Read(); // End array
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<long, decimal> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var kvp in value)
            {
                writer.WriteStartArray();
                writer.WriteNumberValue(kvp.Key);
                writer.WriteStringValue(kvp.Value.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}
