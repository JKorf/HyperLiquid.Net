using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Liquidation update
    /// </summary>
    public record HyperLiquidLiquidationUpdate
    {
        /// <summary>
        /// ["<c>lid</c>"] Liquidation id
        /// </summary>
        [JsonPropertyName("lid")]
        public long Id { get; set; }
        /// <summary>
        /// ["<c>liquidator</c>"] Liquidator
        /// </summary>
        [JsonPropertyName("liquidator")]
        public string Liquidator { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>liquidated_user</c>"] Liquidated user
        /// </summary>
        [JsonPropertyName("liquidated_user")]
        public string LiquidatedUser { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>liquidated_ntl_pos</c>"] Liquidated notional position
        /// </summary>
        [JsonPropertyName("liquidated_ntl_pos")]
        public decimal LiquidatedNotionalPosition { get; set; }
        /// <summary>
        /// ["<c>liquidated_account_value</c>"] Liquidated account value
        /// </summary>
        [JsonPropertyName("liquidated_account_value")]
        public decimal LiquidatedValue { get; set; }
    }
}
