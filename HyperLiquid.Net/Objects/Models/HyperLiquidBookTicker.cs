using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Book ticker
    /// </summary>
    public record HyperLiquidBookTicker
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }

        [JsonInclude, JsonPropertyName("bbo")]
        internal HyperLiquidOrderBookEntry[] Entries { get; set; } = [];

        /// <summary>
        /// Best bid level
        /// </summary>
        public HyperLiquidOrderBookEntry BestBid => Entries[0];

        /// <summary>
        /// Best ask level
        /// </summary>
        public HyperLiquidOrderBookEntry BestAsk => Entries[1];
    }
}
