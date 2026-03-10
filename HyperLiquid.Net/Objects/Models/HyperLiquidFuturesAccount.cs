using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
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
        /// ["<c>assetPositions</c>"] Position info
        /// </summary>
        [JsonPropertyName("assetPositions")]
        public HyperLiquidPosition[] Positions { get; set; } = [];

        /// <summary>
        /// ["<c>crossMaintenanceMarginUsed</c>"] Cross margin maintenance margin used
        /// </summary>
        [JsonPropertyName("crossMaintenanceMarginUsed")]
        public decimal CrossMaintenanceMarginUsed { get; set; }
        /// <summary>
        /// ["<c>withdrawable</c>"] Withdrawable
        /// </summary>
        [JsonPropertyName("withdrawable")]
        public decimal Withdrawable { get; set; }
        /// <summary>
        /// ["<c>time</c>"] Data timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// ["<c>crossMarginSummary</c>"] Cross margin summary
        /// </summary>
        [JsonPropertyName("crossMarginSummary")]
        public HyperLiquidMarginSummary CrossMarginSummary { get; set; } = default!;
        /// <summary>
        /// ["<c>marginSummary</c>"] Margin summary
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
        /// ["<c>accountValue</c>"] Total account value
        /// </summary>
        [JsonPropertyName("accountValue")]
        public decimal AccountValue { get; set; }
        /// <summary>
        /// ["<c>totalMarginUsed</c>"] Total margin used
        /// </summary>
        [JsonPropertyName("totalMarginUsed")]
        public decimal TotalMarginUsed { get; set; }
        /// <summary>
        /// ["<c>totalNtlPos</c>"] Total notional position
        /// </summary>
        [JsonPropertyName("totalNtlPos")]
        public decimal TotalNotionalPosition { get; set; }
        /// <summary>
        /// ["<c>totalRawUsd</c>"] Total raw USD
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
        /// ["<c>type</c>"] Position type
        /// </summary>
        [JsonPropertyName("type")]
        public PositionType PositionType { get; set; }

        /// <summary>
        /// ["<c>position</c>"] Position info
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
        /// ["<c>coin</c>"] Symbol name
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>cumFunding</c>"] Funding info
        /// </summary>
        [JsonPropertyName("cumFunding")]
        public HyperLiquidPositionFunding? Funding { get; set; }
        /// <summary>
        /// ["<c>entryPx</c>"] Average entry price
        /// </summary>
        [JsonPropertyName("entryPx")]
        public decimal? AverageEntryPrice { get; set; }
        /// <summary>
        /// ["<c>leverage</c>"] Leverage info
        /// </summary>
        [JsonPropertyName("leverage")]
        public HyperLiquidPositionLeverage? Leverage { get; set; }
        /// <summary>
        /// ["<c>liquidationPx</c>"] Liquidation price
        /// </summary>
        [JsonPropertyName("liquidationPx")]
        public decimal? LiquidationPrice { get; set; }
        /// <summary>
        /// ["<c>marginUsed</c>"] Margin used
        /// </summary>
        [JsonPropertyName("marginUsed")]
        public decimal? MarginUsed { get; set; }
        /// <summary>
        /// ["<c>maxLeverage</c>"] Max leverage
        /// </summary>
        [JsonPropertyName("maxLeverage")]
        public int MaxLeverage { get; set; }
        /// <summary>
        /// ["<c>positionValue</c>"] Position value
        /// </summary>
        [JsonPropertyName("positionValue")]
        public decimal? PositionValue { get; set; }
        /// <summary>
        /// ["<c>returnOnEquity</c>"] Return on equity
        /// </summary>
        [JsonPropertyName("returnOnEquity")]
        public decimal? ReturnOnEquity { get; set; }
        /// <summary>
        /// ["<c>szi</c>"] Position quantity
        /// </summary>
        [JsonPropertyName("szi")]
        public decimal? PositionQuantity { get; set; }

        /// <summary>
        /// ["<c>unrealizedPnl</c>"] Unrealized profit and loss
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
        /// ["<c>type</c>"] Margin type
        /// </summary>
        [JsonPropertyName("type")]
        public MarginType MarginType { get; set; }
        /// <summary>
        /// ["<c>value</c>"] Value
        /// </summary>
        [JsonPropertyName("value")]
        public int Value { get; set; }
        /// <summary>
        /// ["<c>rawUsd</c>"] Raw USD
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
        /// ["<c>allTime</c>"] All time funding
        /// </summary>
        [JsonPropertyName("allTime")]
        public decimal AllTime { get; set; }
        /// <summary>
        /// ["<c>sinceChange</c>"] Since change
        /// </summary>
        [JsonPropertyName("sinceChange")]
        public decimal SinceChange { get; set; }
        /// <summary>
        /// ["<c>sinceOpen</c>"] Since open
        /// </summary>
        [JsonPropertyName("sinceOpen")]
        public decimal SinceOpen { get; set; }
    }
}
