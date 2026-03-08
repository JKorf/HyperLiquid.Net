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
        /// ["<c>time</c>"] Transaction time (milliseconds)
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// ["<c>hash</c>"] Transaction hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// ["<c>delta</c>"] Delta information
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
        /// ["<c>delegate</c>"] Delegate
        /// </summary>
        [JsonPropertyName("delegate")]
        public HyperLiquidStakingDelegate? Delegate { get; set; }
        /// <summary>
        /// ["<c>cDeposit</c>"] Deposit info
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
        /// ["<c>amount</c>"] Quantity
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
        /// ["<c>validator</c>"] Validator
        /// </summary>
        [JsonPropertyName("validator")]
        public string Validator { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>amount</c>"] Quantity
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>isUndelegate</c>"] Is undelegate
        /// </summary>
        [JsonPropertyName("isUndelegate")]
        public bool IsUndelegate { get; set; }
    }
}
