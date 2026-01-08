using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Perp DEX market status
    /// </summary>
    public record HyperLiquidPerpDexStatus
    {
        /// <summary>
        /// Total net deposit
        /// </summary>
        [JsonPropertyName("totalNetDeposit")]
        public decimal TotalNetDeposit { get; set; }
    }
}
