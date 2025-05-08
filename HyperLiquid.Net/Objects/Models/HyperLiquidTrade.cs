using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Trade info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTrade
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Trade price
        /// </summary>
        [JsonPropertyName("px")]
        public decimal Price { get; set; }
        /// <summary>
        /// Trade quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Trade id
        /// </summary>
        [JsonPropertyName("tid")]
        public long TradeId { get; set; }
        /// <summary>
        /// Users
        /// </summary>
        [JsonPropertyName("users")]
        public string[] Users { get; set; } = [];
    }
}
