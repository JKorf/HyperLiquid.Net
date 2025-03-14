using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Open order info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOpenOrder
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        [JsonIgnore]
        public string? Symbol { get; set; }
        /// <summary>
        /// Symbol type
        /// </summary>
        [JsonIgnore]
        public SymbolType SymbolType { get; set; }
        /// <summary>
        /// Symbol as returned by the API
        /// </summary>
        [JsonPropertyName("coin")]
        public string ExchangeSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Limit price
        /// </summary>
        [JsonPropertyName("limitPx")]
        public decimal Price { get; set; }
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
        /// <summary>
        /// Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide OrderSide { get; set; }
        /// <summary>
        /// Order quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Order timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
