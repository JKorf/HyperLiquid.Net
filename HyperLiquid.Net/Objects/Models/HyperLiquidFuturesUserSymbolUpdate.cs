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
        /// User address
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Leverage info
        /// </summary>
        [JsonPropertyName("leverage")]
        public HyperLiquidLeverage Leverage { get; set; } = default!;
        /// <summary>
        /// Max trade quantities
        /// </summary>
        [JsonPropertyName("maxTradeSzs")]
        public decimal[] MaxTradeQuantities { get; set; } = [];
        /// <summary>
        /// Available to trade
        /// </summary>
        [JsonPropertyName("availableToTrade")]
        public decimal[] AvailableToTrade { get; set; } = [];
    }

    /// <summary>
    /// Leverage
    /// </summary>
    [SerializationModel]
    public record HyperLiquidLeverage
    {
        /// <summary>
        /// Raw USD
        /// </summary>
        [JsonPropertyName("rawUsd")]
        public decimal RawUsd { get; set; }
        /// <summary>
        /// Margin type
        /// </summary>
        [JsonPropertyName("type")]
        public MarginType MarginType { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
