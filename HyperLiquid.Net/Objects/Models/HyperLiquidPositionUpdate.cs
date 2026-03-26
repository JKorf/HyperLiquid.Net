using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Futures position update
    /// </summary>
    public record HyperLiquidAllDexPositionUpdate
    {
        /// <summary>
        /// ["<c>user</c>"] User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>clearinghouseState</c>"] Clearinghouse state
        /// </summary>
        [JsonPropertyName("clearinghouseStates")]
        public HyperLiquidDexPositionUpdate[] Data { get; set; } = [];
    }

    /// <summary>
    /// Update data
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidDexPositionUpdate>))]
    public record HyperLiquidDexPositionUpdate
    {
        /// <summary>
        /// Dex name
        /// </summary>
        [ArrayProperty(0)]
        public string Dex { get; set; } = string.Empty;
        /// <summary>
        /// Data
        /// </summary>
        [ArrayProperty(1)]
        [JsonConversion]
        public HyperLiquidPositionUpdateData Data { get; set; } = default!;
    }

    /// <summary>
    /// Futures position update
    /// </summary>
    public record HyperLiquidPositionUpdate
    {
        /// <summary>
        /// ["<c>dex</c>"] Dex
        /// </summary>
        [JsonPropertyName("dex")]
        public string Dex { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>user</c>"] User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>clearinghouseState</c>"] Clearinghouse state
        /// </summary>
        [JsonPropertyName("clearinghouseState")]
        public HyperLiquidPositionUpdateData Data { get; set; } = null!;
    }

    /// <summary>
    /// Position update data
    /// </summary>
    public record HyperLiquidPositionUpdateData
    {
        /// <summary>
        /// ["<c>marginSummary</c>"] Margin summary
        /// </summary>
        [JsonPropertyName("marginSummary")]
        public HyperLiquidPositionUpdateAccountValue MarginSummary { get; set; } = null!;
        /// <summary>
        /// ["<c>crossMarginSummary</c>"] Cross margin summary
        /// </summary>
        [JsonPropertyName("crossMarginSummary")]
        public HyperLiquidPositionUpdateAccountValue CrossMarginSummary { get; set; } = null!;
        /// <summary>
        /// ["<c>crossMaintenanceMarginUsed</c>"] Cross maintenance margin used
        /// </summary>
        [JsonPropertyName("crossMaintenanceMarginUsed")]
        public decimal CrossMaintenanceMarginUsed { get; set; }
        /// <summary>
        /// ["<c>withdrawable</c>"] Withdrawable
        /// </summary>
        [JsonPropertyName("withdrawable")]
        public decimal Withdrawable { get; set; }
        /// <summary>
        /// ["<c>assetPositions</c>"] Asset positions
        /// </summary>
        [JsonPropertyName("assetPositions")]
        public HyperLiquidPosition[] AssetPositions { get; set; } = [];
        /// <summary>
        /// ["<c>time</c>"] Update timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Account value
    /// </summary>
    public record HyperLiquidPositionUpdateAccountValue
    {
        /// <summary>
        /// ["<c>accountValue</c>"] Account value
        /// </summary>
        [JsonPropertyName("accountValue")]
        public decimal AccountValue { get; set; }
        /// <summary>
        /// ["<c>totalNtlPos</c>"] Total notional position
        /// </summary>
        [JsonPropertyName("totalNtlPos")]
        public decimal TotalNtlPos { get; set; }
        /// <summary>
        /// ["<c>totalRawUsd</c>"] Total raw usd
        /// </summary>
        [JsonPropertyName("totalRawUsd")]
        public decimal TotalRawUsd { get; set; }
        /// <summary>
        /// ["<c>totalMarginUsed</c>"] Total margin used
        /// </summary>
        [JsonPropertyName("totalMarginUsed")]
        public decimal TotalMarginUsed { get; set; }
    }
}
