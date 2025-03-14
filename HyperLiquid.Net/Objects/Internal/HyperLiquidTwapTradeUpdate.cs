using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Objects.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    [SerializationModel]
    internal record HyperLiquidTwapTradeUpdate
    {
        [JsonPropertyName("twapSliceFills")]
        public HyperLiquidTwapStatus[] Trades { get; set; } = [];
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        [JsonPropertyName("isSnapshot")]
        public bool IsSnapshot { get; set; }
    }
}
