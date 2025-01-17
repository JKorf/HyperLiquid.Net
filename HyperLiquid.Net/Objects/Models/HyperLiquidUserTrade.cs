﻿using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User trade
    /// </summary>
    public record HyperLiquidUserTrade
    {
        /// <summary>
        /// Closed pnl
        /// </summary>
        [JsonPropertyName("closedPnl")]
        public decimal? ClosedPnl { get; set; }
        /// <summary>
        /// Symbol as returned by the exchange
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
        /// Crossed
        /// </summary>
        [JsonPropertyName("crossed")]
#warning is this taker/maker?
        public bool Crossed { get; set; }
        /// <summary>
        /// Direction
        /// </summary>
        [JsonPropertyName("dir")]
        public Direction Direction { get; set; }
        /// <summary>
        /// Hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("px")]
        public decimal Price { get; set; }
        /// <summary>
        /// Side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide OrderSide { get; set; }
        /// <summary>
        /// Start position
        /// </summary>
        [JsonPropertyName("startPosition")]
#warning What's this?
        public decimal StartPosition { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Fee
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
        /// <summary>
        /// Fee token
        /// </summary>
        [JsonPropertyName("feeToken")]
        public string FeeToken { get; set; } = string.Empty;
        /// <summary>
        /// Builder fee
        /// </summary>
        [JsonPropertyName("builderFee")]
        public decimal? BuilderFee { get; set; }
        /// <summary>
        /// Trade id
        /// </summary>
        [JsonPropertyName("tid")]
        public long TradeId { get; set; }
    }


}
