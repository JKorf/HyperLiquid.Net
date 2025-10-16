using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Liquidation update
    /// </summary>
    public record HyperLiquidLiquidationUpdate
    {
        /// <summary>
        /// Liquidation id
        /// </summary>
        [JsonPropertyName("lid")]
        public long Id { get; set; }
        /// <summary>
        /// Liquidator
        /// </summary>
        [JsonPropertyName("liquidator")]
        public string Liquidator { get; set; } = string.Empty;
        /// <summary>
        /// Liquidated user
        /// </summary>
        [JsonPropertyName("liquidated_user")]
        public string LiquidatedUser { get; set; } = string.Empty;
        /// <summary>
        /// Liquidated notional position
        /// </summary>
        [JsonPropertyName("liquidated_ntl_pos")]
        public decimal LiquidatedNotionalPosition { get; set; }
        /// <summary>
        /// Liquidated account value
        /// </summary>
        [JsonPropertyName("liquidated_account_value")]
        public decimal LiquidatedValue { get; set; }
    }
}
