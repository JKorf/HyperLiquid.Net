﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HyperLiquid.Net.Converters
{
    internal class CancelResultConverter : JsonConverter<string[]>
    {
        public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var resultList = new List<string>();
            reader.Read();
            while (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.String)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    resultList.Add(JsonSerializer.Deserialize(ref reader, (JsonTypeInfo<string>)options.GetTypeInfo(typeof(string)))!);
                    reader.Read();
                    continue;
                }

                var result = JsonSerializer.Deserialize(ref reader, (JsonTypeInfo<ErrorMessage>)options.GetTypeInfo(typeof(ErrorMessage)));
                resultList.Add(result!.Error);
                reader.Read();
            }

            return resultList.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
                writer.WriteStringValue(item);

            writer.WriteEndArray();
        }

        internal class ErrorMessage
        {
            [JsonPropertyName("error")]
            public string Error { get; set; } = string.Empty;
        }
    }
}
