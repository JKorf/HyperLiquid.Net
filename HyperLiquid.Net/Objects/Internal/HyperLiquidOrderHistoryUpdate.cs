using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Objects.Internal;

[SerializationModel]
internal record HyperLiquidOrderHistoryUpdate
{
    [JsonPropertyName("orderHistory")]
    public HyperLiquidOrderStatus[] Orders { get; set; } = [];
    [JsonPropertyName("user")]
    public string User { get; set; } = string.Empty;
}