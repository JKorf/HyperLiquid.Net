using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Spot balance update
    /// </summary>
    public record HyperLiquidBalanceUpdate
    {
        /// <summary>
        /// User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// Open orders
        /// </summary>
        [JsonPropertyName("spotState")]
        public HyperLiquidBalances Orders { get; set; } = default!;
    }
}
