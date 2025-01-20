﻿using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Order info
    /// </summary>
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
        /// Symbol name as returned by the API
        /// </summary>
        [JsonPropertyName("coin")]
        public string ExchangeSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Is position take profit / stop loss
        /// </summary>
        [JsonPropertyName("isPositionTpsl")]
        public bool IsPositionTpSl { get; set; }
        /// <summary>
        /// Is trigger order
        /// </summary>
        [JsonPropertyName("isTrigger")]
        public bool IsTrigger { get; set; }
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
        /// Order type
        /// </summary>
        [JsonPropertyName("type")]
        public OrderType OrderType { get; set; }
        /// <summary>
        /// Original order quantity
        /// </summary>
        [JsonPropertyName("origSz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Reduce only
        /// </summary>
        [JsonPropertyName("reduceOnly")]
        public bool ReduceOnly { get; set; }
        /// <summary>
        /// Remaining unexecuted order quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal QuantityRemaining { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Trigger condition
        /// </summary>
        [JsonPropertyName("triggerCondition")]
        public string TriggerCondition { get; set; } = string.Empty;
        /// <summary>
        /// Trigger price
        /// </summary>
        [JsonPropertyName("triggerPx")]
        public decimal? TriggerPrice { get; set; }
    }
}
