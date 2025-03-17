using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HyperLiquid.Net.Converters
{
    internal class TwapCancelResultConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var error = JsonSerializer.Deserialize<ErrorMessage>(ref reader, (JsonTypeInfo<ErrorMessage>)options.GetTypeInfo(typeof(ErrorMessage)));
                return error!.Error;
            }
            else
            {
                return reader.GetString()!;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        class ErrorMessage
        {
            [JsonPropertyName("error")]
            public string Error { get; set; } = string.Empty;
        }
    }
}
