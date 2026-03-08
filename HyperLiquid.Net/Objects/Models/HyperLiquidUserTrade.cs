using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User trade
    /// </summary>
    [SerializationModel]
    public record HyperLiquidUserTrade
    {
        /// <summary>
        /// ["<c>closedPnl</c>"] Closed pnl
        /// </summary>
        [JsonPropertyName("closedPnl")]
        public decimal? ClosedPnl { get; set; }
        /// <summary>
        /// ["<c>coin</c>"] Symbol as returned by the exchange
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
        /// ["<c>crossed</c>"] Crossed, true: Taker, false: Maker
        /// </summary>
        [JsonPropertyName("crossed")]
        public bool Crossed { get; set; }
        /// <summary>
        /// ["<c>dir</c>"] Direction
        /// </summary>
        [JsonPropertyName("dir")]
        public Direction Direction { get; set; }
        /// <summary>
        /// ["<c>hash</c>"] Hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>oid</c>"] Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
        /// <summary>
        /// ["<c>px</c>"] Price
        /// </summary>
        [JsonPropertyName("px")]
        public decimal Price { get; set; }
        /// <summary>
        /// ["<c>side</c>"] Side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide OrderSide { get; set; }
        /// <summary>
        /// ["<c>startPosition</c>"] Start position
        /// </summary>
        [JsonPropertyName("startPosition")]
        public decimal StartPosition { get; set; }
        /// <summary>
        /// ["<c>sz</c>"] Quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>time</c>"] Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// ["<c>fee</c>"] Fee
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
        /// <summary>
        /// ["<c>feeToken</c>"] Fee token
        /// </summary>
        [JsonPropertyName("feeToken")]
        public string FeeToken { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>builderFee</c>"] Builder fee
        /// </summary>
        [JsonPropertyName("builderFee")]
        public decimal? BuilderFee { get; set; }
        /// <summary>
        /// ["<c>tid</c>"] Trade id
        /// </summary>
        [JsonPropertyName("tid")]
        public long TradeId { get; set; }
        /// <summary>
        /// ["<c>liquidation</c>"] Liquidation info
        /// </summary>
        [JsonPropertyName("liquidation")]
        public HyperLiquidLiquidationInfo? Liquidation { get; set; }
        /// <summary>
        /// ["<c>twapId</c>"] TWAP id
        /// </summary>
        [JsonPropertyName("twapId")]
        public long? TwapId { get; set; }
    }

    /// <summary>
    /// Liquidation info
    /// </summary>
    public record HyperLiquidLiquidationInfo
    {
        /// <summary>
        /// ["<c>liquidatedUser</c>"] Liquidated user
        /// </summary>
        [JsonPropertyName("liquidatedUser")]
        public string LiquidatedUser { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>markPx</c>"] Mark price
        /// </summary>
        [JsonPropertyName("markPx")]
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// ["<c>method</c>"] Liquidation method
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
    }
}
