using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Staking history
    /// </summary>
    public record HyperLiquidStakingHistory
    {
        /// <summary>
        /// Transaction time (milliseconds)
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Delta information
        /// </summary>
        [JsonPropertyName("delta")]
        public HyperLiquidStakingDelta Delta { get; set; } = null!;
    }

    /// <summary>
    /// Delta
    /// </summary>
    public record HyperLiquidStakingDelta
    {
        /// <summary>
        /// Delegate
        /// </summary>
        [JsonPropertyName("delegate")]
        public HyperLiquidStakingDelegate? Delegate { get; set; }
        /// <summary>
        /// Deposit info
        /// </summary>
        [JsonPropertyName("cDeposit")]
        public HyperLiquidStackingDeposit? Deposit { get; set; }
    }

    /// <summary>
    /// Deposit info
    /// </summary>
    public record HyperLiquidStackingDeposit
    {
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Quantity { get; set; }
    }

    /// <summary>
    /// Delegate
    /// </summary>
    public record HyperLiquidStakingDelegate
    {
        /// <summary>
        /// Validator
        /// </summary>
        [JsonPropertyName("validator")]
        public string Validator { get; set; } = string.Empty;
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Is undelegate
        /// </summary>
        [JsonPropertyName("isUndelegate")]
        public bool IsUndelegate { get; set; }
    }
}
