using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// TWAP order status
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTwapHistoryStatus
    {
        /// <summary>
        /// TWAP status
        /// </summary>
        [JsonPropertyName("state")]
        public HyperLiquidTwapHistoryState TwapInfo { get; set; } = default!;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonPropertyName("status")]
        public HyperLiquidTwapOrderStatusDesc Status { get; set; } = default!;
        /// <summary>
        /// TWAP ID
        /// </summary>
        [JsonPropertyName("twapId")]
        public long TwapId { get; set; }
    }

    /// <summary>
    /// State of the Twap Order
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTwapHistoryState
    {
        /// <summary>
        /// Symbol as returned by the API
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
        /// User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Executed quantity
        /// </summary>
        [JsonPropertyName("executedSz")]
        public decimal ExecutedQuantity { get; set; }
        /// <summary>
        /// Executed value
        /// </summary>
        [JsonPropertyName("executedNtl")]
        public decimal ExecutedValue { get; set; }
        /// <summary>
        /// Minutes
        /// </summary>
        [JsonPropertyName("minutes")]
        public int Minutes { get; set; }
        /// <summary>
        /// Reduce only
        /// </summary>
        [JsonPropertyName("reduceOnly")]
        public bool ReduceOnly { get; set; }
        /// <summary>
        /// Randomize
        /// </summary>
        [JsonPropertyName("randomize")]
        public bool Randomize { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// TWAP order status
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTwapOrderStatusDesc
    {
        /// <summary>
        /// Order status
        /// </summary>
        [JsonPropertyName("status")]
        public TwapStatus Status { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
