using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Staking reward
    /// </summary>
    public record HyperLiquidStakingReward
    {
        /// <summary>
        /// ["<c>time</c>"] Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// ["<c>source</c>"] Source
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>totalAmount</c>"] Total quantity
        /// </summary>
        [JsonPropertyName("totalAmount")]
        public decimal TotalQuantity { get; set; }
    }
}
