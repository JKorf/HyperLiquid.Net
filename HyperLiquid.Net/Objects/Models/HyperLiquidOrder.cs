using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Order info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOrder
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
        /// ["<c>coin</c>"] Symbol name as returned by the API
        /// </summary>
        [JsonPropertyName("coin")]
        public string ExchangeSymbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>isPositionTpsl</c>"] Is position take profit / stop loss
        /// </summary>
        [JsonPropertyName("isPositionTpsl")]
        public bool IsPositionTpSl { get; set; }
        /// <summary>
        /// ["<c>isTrigger</c>"] Is trigger order
        /// </summary>
        [JsonPropertyName("isTrigger")]
        public bool IsTrigger { get; set; }
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
        /// ["<c>orderType</c>"] Order type. Note that Limit is returned for Market orders
        /// </summary>
        [JsonPropertyName("orderType")]
        public OrderType OrderType { get; set; }
        /// <summary>
        /// ["<c>tif</c>"] Time in force
        /// </summary>
        [JsonPropertyName("tif")]
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// ["<c>origSz</c>"] Original order quantity
        /// </summary>
        [JsonPropertyName("origSz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>reduceOnly</c>"] Reduce only
        /// </summary>
        [JsonPropertyName("reduceOnly")]
        public bool ReduceOnly { get; set; }
        /// <summary>
        /// ["<c>sz</c>"] Remaining unexecuted order quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// ["<c>timestamp</c>"] Timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// ["<c>triggerCondition</c>"] Trigger condition
        /// </summary>
        [JsonPropertyName("triggerCondition")]
        public string TriggerCondition { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>triggerPx</c>"] Trigger price
        /// </summary>
        [JsonPropertyName("triggerPx")]
        public decimal? TriggerPrice { get; set; }
        /// <summary>
        /// Children orders of this order
        /// </summary>
        [JsonPropertyName("children")]
        public HyperLiquidOrder[]? Children { get; set; }
    }
}
