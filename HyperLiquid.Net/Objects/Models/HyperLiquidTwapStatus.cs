using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Twap status
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTwapStatus
    {
        /// <summary>
        /// ["<c>coin</c>"] Symbol as returned by the API
        /// </summary>
        [JsonPropertyName("coin")]
        public string ExchangeSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonIgnore]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Symbol type
        /// </summary>
        [JsonIgnore]
        public SymbolType SymbolType { get; set; }
        /// <summary>
        /// ["<c>user</c>"] User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>side</c>"] Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// ["<c>sz</c>"] Quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>executedSz</c>"] Executed quantity
        /// </summary>
        [JsonPropertyName("executedSz")]
        public decimal ExecutedQuantity { get; set; }
        /// <summary>
        /// ["<c>executedNtl</c>"] Executed value
        /// </summary>
        [JsonPropertyName("executedNtl")]
        public decimal ExecutedValue { get; set; }
        /// <summary>
        /// ["<c>minutes</c>"] Minutes
        /// </summary>
        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }
        /// <summary>
        /// ["<c>reduceOnly</c>"] Reduce only
        /// </summary>
        [JsonPropertyName("reduceOnly")]
        public bool ReduceOnly { get; set; }
        /// <summary>
        /// ["<c>randomize</c>"] Randomize
        /// </summary>
        [JsonPropertyName("randomize")]
        public bool Randomize { get; set; }
        /// <summary>
        /// ["<c>timestamp</c>"] Timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
