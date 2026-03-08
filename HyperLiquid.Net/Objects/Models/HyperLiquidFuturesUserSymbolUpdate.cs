using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User symbol update
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFuturesUserSymbolUpdate
    {
        /// <summary>
        /// ["<c>user</c>"] User address
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>coin</c>"] Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>leverage</c>"] Leverage info
        /// </summary>
        [JsonPropertyName("leverage")]
        public HyperLiquidLeverage Leverage { get; set; } = default!;
        /// <summary>
        /// ["<c>maxTradeSzs</c>"] Max trade quantities
        /// </summary>
        [JsonPropertyName("maxTradeSzs")]
        public decimal[] MaxTradeQuantities { get; set; } = [];
        /// <summary>
        /// ["<c>availableToTrade</c>"] Available to trade
        /// </summary>
        [JsonPropertyName("availableToTrade")]
        public decimal[] AvailableToTrade { get; set; } = [];
        /// <summary>
        /// ["<c>markPx</c>"] Mark price
        /// </summary>
        [JsonPropertyName("markPx")]
        public decimal? MarkPrice { get; set; }
    }

    /// <summary>
    /// Leverage
    /// </summary>
    [SerializationModel]
    public record HyperLiquidLeverage
    {
        /// <summary>
        /// ["<c>rawUsd</c>"] Raw USD
        /// </summary>
        [JsonPropertyName("rawUsd")]
        public decimal RawUsd { get; set; }
        /// <summary>
        /// ["<c>type</c>"] Margin type
        /// </summary>
        [JsonPropertyName("type")]
        public MarginType MarginType { get; set; }
        /// <summary>
        /// ["<c>value</c>"] Value
        /// </summary>
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
