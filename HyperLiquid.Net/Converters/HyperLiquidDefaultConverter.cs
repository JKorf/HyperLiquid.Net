using HyperLiquid.Net.Objects.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HyperLiquid.Net.Converters
{
    internal record HyperLiquidDefaultInt
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    internal class HyperLiquidDefaultConverter : JsonConverter<HyperLiquidDefault>
    {
        public override HyperLiquidDefault Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Can't deserialize to HyperLiquidDefault type or we will cause a stack overflow
                var result = JsonSerializer.Deserialize<HyperLiquidDefaultInt>(ref reader, (JsonTypeInfo<HyperLiquidDefaultInt>)options.GetTypeInfo(typeof(HyperLiquidDefaultInt)));
                return new HyperLiquidDefault()
                {
                    Type = result!.Type
                };
            }
            else
            {
                var error = reader.GetString()!;
                return new HyperLiquidDefault
                {
                    Type = error
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, HyperLiquidDefault value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue(value.Type);
            writer.WriteEndObject();
        }
    }
}
