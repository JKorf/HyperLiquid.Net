using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Margin table
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidMarginTable>))]
    public record HyperLiquidMarginTable
    {
        /// <summary>
        /// Id
        /// </summary>
        [ArrayProperty(0)]
        public int Id { get; set; }
        /// <summary>
        /// Table info
        /// </summary>
        [ArrayProperty(1)]
        [JsonConversion]
        public HyperLiquidMarginTableEntry Table { get; set; } = default!;
    }

    /// <summary>
    /// Table entry
    /// </summary>
    public record HyperLiquidMarginTableEntry
    {
        /// <summary>
        /// Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Margin tiers
        /// </summary>
        [JsonPropertyName("marginTiers")]
        public HyperLiquidMarginTableTier[] MarginTiers { get; set; } = [];
    }

    /// <summary>
    /// Margin tier
    /// </summary>
    public record HyperLiquidMarginTableTier
    {
        /// <summary>
        /// Lower bound
        /// </summary>
        [JsonPropertyName("lowerBound")]
        public decimal LowerBound { get; set; }
        /// <summary>
        /// Max leverage
        /// </summary>
        [JsonPropertyName("maxLeverage")]
        public int MaxLeverage{ get; set; }
    }
}
