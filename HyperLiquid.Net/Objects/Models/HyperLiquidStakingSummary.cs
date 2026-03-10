using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Staking summary
    /// </summary>
    public record HyperLiquidStakingSummary
    {
        /// <summary>
        /// ["<c>delegated</c>"] Total amount delegated to validators
        /// </summary>
        [JsonPropertyName("delegated")]
        public decimal Delegated { get; set; }

        /// <summary>
        /// ["<c>undelegated</c>"] Amount in staking balance but not delegated
        /// </summary>
        [JsonPropertyName("undelegated")]
        public decimal Undelegated { get; set; }

        /// <summary>
        /// ["<c>totalPendingWithdrawal</c>"] Total amount pending withdrawal (in 7-day queue)
        /// </summary>
        [JsonPropertyName("totalPendingWithdrawal")]
        public decimal TotalPendingWithdrawal { get; set; }

        /// <summary>
        /// ["<c>nPendingWithdrawals</c>"] Number of pending withdrawals
        /// </summary>
        [JsonPropertyName("nPendingWithdrawals")]
        public int PendingWithdrawals { get; set; }
    }
}
