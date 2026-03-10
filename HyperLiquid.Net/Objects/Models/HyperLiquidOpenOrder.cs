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
        /// ["<c>cloid</c>"] Client order id
        /// </summary>
        [JsonPropertyName("cloid")]
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// ["<c>coin</c>"] Symbol as returned by the API
        /// </summary>
        [JsonPropertyName("coin")]
        public string ExchangeSymbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>limitPx</c>"] Limit price
        /// </summary>
        [JsonPropertyName("limitPx")]
        public decimal Price { get; set; }
        /// <summary>
        /// ["<c>oid</c>"] Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
        /// <summary>
        /// ["<c>side</c>"] Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide OrderSide { get; set; }
        /// <summary>
        /// ["<c>sz</c>"] Remaining unexecuted order quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// ["<c>origSz</c>"] Original order quantity
        /// </summary>
        [JsonPropertyName("origSz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>timestamp</c>"] Order timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
