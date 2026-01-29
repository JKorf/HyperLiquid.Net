using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;

namespace HyperLiquid.Net.Objects.Models;

[SerializationModel]
internal record HyperLiquidUserTwapFillResult
{
    [JsonPropertyName("fill")]
    public HyperLiquidUserTrade Fill { get; set; } = default!;
    [JsonPropertyName("twapId")]
    public long TwapId { get; set; }
}