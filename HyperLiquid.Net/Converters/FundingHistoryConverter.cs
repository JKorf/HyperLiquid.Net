using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Converters
{
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

    internal class FundingHistoryConverter : JsonConverter<HyperLiquidUserLedger<HyperLiquidUserFunding[]>>
    {
        private static readonly JsonSerializerOptions _deserializeOptions = SerializerOptions.WithConverters(new HyperLiquidSourceGenerationContext());

        /// <inheritdoc />
        public override HyperLiquidUserLedger<HyperLiquidUserFunding[]>? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                    _ = JsonSerializer.Deserialize<object[]>(ref reader, options);
                    return new HyperLiquidUserLedger<HyperLiquidUserFunding[]>() { Data = [] };
                case JsonTokenType.StartObject:
                    return JsonSerializer.Deserialize<HyperLiquidUserLedger<HyperLiquidUserFunding[]>>(ref reader, _deserializeOptions);
            }
            ;

            return default;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, HyperLiquidUserLedger<HyperLiquidUserFunding[]> value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, (object?)value, options);
    }
}
