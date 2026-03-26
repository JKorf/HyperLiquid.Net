using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Open order update
    /// </summary>
    public record HyperLiquidOpenOrderUpdate
    {
        /// <summary>
        /// DEX
        /// </summary>
        [JsonPropertyName("dex")]
        public string Dex { get; set; } = string.Empty;
        /// <summary>
        /// User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// Open orders
        /// </summary>
        [JsonPropertyName("orders")]
        public HyperLiquidOpenOrder[] Orders { get; set; } = [];
    }
}
