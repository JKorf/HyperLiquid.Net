using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Futures account info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFuturesAccount
    {
        /// <summary>
        /// Position info
        /// </summary>
        [JsonPropertyName("assetPositions")]
        public HyperLiquidPosition[] Positions { get; set; } = [];

        /// <summary>
        /// Cross margin maintenance margin used
        /// </summary>
        [JsonPropertyName("crossMaintenanceMarginUsed")]
        public decimal CrossMaintenanceMarginUsed { get; set; }
        /// <summary>
        /// Withdrawable
        /// </summary>
        [JsonPropertyName("withdrawable")]
        public decimal Withdrawable { get; set; }
        /// <summary>
        /// Data timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Cross margin summary
        /// </summary>
        [JsonPropertyName("crossMarginSummary")]
        public HyperLiquidMarginSummary CrossMarginSummary { get; set; } = default!;
        /// <summary>
        /// Margin summary
        /// </summary>
        [JsonPropertyName("marginSummary")]
        public HyperLiquidMarginSummary MarginSummary { get; set; } = default!;
    }

    /// <summary>
    /// Margin info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidMarginSummary
    {
        /// <summary>
        /// Total account value
        /// </summary>
        [JsonPropertyName("accountValue")]
        public decimal AccountValue { get; set; }
        /// <summary>
        /// Total margin used
        /// </summary>
        [JsonPropertyName("totalMarginUsed")]
        public decimal TotalMarginUsed { get; set; }
        /// <summary>
        /// Total notional position
        /// </summary>
        [JsonPropertyName("totalNtlPos")]
        public decimal TotalNotionalPosition { get; set; }
        /// <summary>
        /// Total raw USD
        /// </summary>
        [JsonPropertyName("totalRawUsd")]
        public decimal TotalRawUsd { get; set; }
    }

    /// <summary>
    /// Position info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidPosition
    {
        /// <summary>
        /// Position type
        /// </summary>
        [JsonPropertyName("type")]
        public PositionType PositionType { get; set; }

        /// <summary>
        /// Position info
        /// </summary>
        [JsonPropertyName("position")]
        public HyperLiquidPositionInfo Position { get; set; } = default!;
    }

    /// <summary>
    /// Position info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidPositionInfo
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Funding info
        /// </summary>
        [JsonPropertyName("cumFunding")]
        public HyperLiquidPositionFunding? Funding { get; set; }
        /// <summary>
        /// Average entry price
        /// </summary>
        [JsonPropertyName("entryPx")]
        public decimal? AverageEntryPrice { get; set; }
        /// <summary>
        /// Leverage info
        /// </summary>
        [JsonPropertyName("leverage")]
        public HyperLiquidPositionLeverage? Leverage { get; set; }
        /// <summary>
        /// Liquidation price
        /// </summary>
        [JsonPropertyName("liquidationPx")]
        public decimal? LiquidationPrice { get; set; }
        /// <summary>
        /// Margin used
        /// </summary>
        [JsonPropertyName("marginUsed")]
        public decimal? MarginUsed { get; set; }
        /// <summary>
        /// Max leverage
        /// </summary>
        [JsonPropertyName("maxLeverage")]
        public int MaxLeverage { get; set; }
        /// <summary>
        /// Position value
        /// </summary>
        [JsonPropertyName("positionValue")]
        public decimal? PositionValue { get; set; }
        /// <summary>
        /// Return on equity
        /// </summary>
        [JsonPropertyName("returnOnEquity")]
        public decimal? ReturnOnEquity { get; set; }
        /// <summary>
        /// Position quantity
        /// </summary>
        [JsonPropertyName("szi")]
        public decimal? PositionQuantity { get; set; }

        /// <summary>
        /// Unrealized profit and loss
        /// </summary>
        [JsonPropertyName("unrealizedPnl")]
        public decimal? UnrealizedPnl { get; set; }
    }

    /// <summary>
    /// Position leverage
    /// </summary>
    [SerializationModel]
    public record HyperLiquidPositionLeverage
    {
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
        /// <summary>
        /// Raw USD
        /// </summary>
        [JsonPropertyName("rawUsd")]
        public decimal? RawUsd { get; set; }
    }

    /// <summary>
    /// Position funding
    /// </summary>
    [SerializationModel]
    public record HyperLiquidPositionFunding
    {
        /// <summary>
        /// All time funding
        /// </summary>
        [JsonPropertyName("allTime")]
        public decimal AllTime { get; set; }
        /// <summary>
        /// Since change
        /// </summary>
        [JsonPropertyName("sinceChange")]
        public decimal SinceChange { get; set; }
        /// <summary>
        /// Since open
        /// </summary>
        [JsonPropertyName("sinceOpen")]
        public decimal SinceOpen { get; set; }
    }
}
