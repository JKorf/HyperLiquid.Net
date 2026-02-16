using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Staking delegation info
    /// </summary>
    public record HyperLiquidStakingDelegation
    {
        /// <summary>
        /// Validator address
        /// </summary>
        [JsonPropertyName("validator")]
        public string Validator { get; set; } = string.Empty;

        /// <summary>
        /// Amount delegated
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Locked until timestamp
        /// </summary>
        [JsonPropertyName("lockedUntilTimestamp")]
        public DateTime? LockedUntil { get; set; }
    }
}
