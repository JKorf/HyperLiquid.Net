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
    /// Portfolio data
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidUserPortfolioData>))]
    public record HyperLiquidUserPortfolioData
    {
        /// <summary>
        /// Period name
        /// </summary>
        [ArrayProperty(0)]
        public string Period { get; set; } = string.Empty;
        /// <summary>
        /// Period value
        /// </summary>
        [ArrayProperty(1)]
        [JsonConversion]
        public HyperLiquidUserPortfolioHistory Value { get; set; } = default!;
    }

    /// <summary>
    /// Portolio history data
    /// </summary>
    public record HyperLiquidUserPortfolioHistory
    {
        /// <summary>
        /// ["<c>vlm</c>"] Volume
        /// </summary>
        [JsonPropertyName("vlm")]
        public decimal Volume { get; set; }
        /// <summary>
        /// ["<c>accountValueHistory</c>"] Value history
        /// </summary>
        [JsonPropertyName("accountValueHistory")]
        public HyperLiquidHistoryItem[] ValueHistory { get; set; } = [];
        /// <summary>
        /// ["<c>pnlHistory</c>"] PNL history
        /// </summary>
        [JsonPropertyName("pnlHistory")]
        public HyperLiquidHistoryItem[] PnlHistory { get; set; } = [];

    }

    /// <summary>
    /// History item
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidHistoryItem>))]
    public record HyperLiquidHistoryItem
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        [ArrayProperty(0)]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        [ArrayProperty(1)]
        public decimal Value { get; set; }
    }
}
