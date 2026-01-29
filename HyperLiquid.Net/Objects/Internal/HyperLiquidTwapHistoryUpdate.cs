using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Objects.Models;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    [SerializationModel]
    internal record HyperLiquidTwapHistoryUpdate
    {
        [JsonPropertyName("history")]
        public HyperLiquidTwapHistoryStatus[] History { get; set; } = [];
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        [JsonPropertyName("isSnapshot")]
        public bool IsSnapshot { get; set; }
    }
}
