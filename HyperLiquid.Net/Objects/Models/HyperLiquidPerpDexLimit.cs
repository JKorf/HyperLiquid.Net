using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Perp DEX market limits
    /// </summary>
    public record HyperLiquidPerpDexLimit
    {
        /// <summary>
        /// Total open interest cap
        /// </summary>
        [JsonPropertyName("totalOiCap")]
        public decimal TotalOpenInterestCap { get; set; }
        /// <summary>
        /// Open interest size cap per perp
        /// </summary>
        [JsonPropertyName("oiSzCapPerPerp")]
        public decimal OpenInterestSizeCapPerPerp { get; set; }
        /// <summary>
        /// Max transfer notional value
        /// </summary>
        [JsonPropertyName("maxTransferNtl")]
        public decimal MaxTransferNotional { get; set; }
        /// <summary>
        /// Open interest cap per symbol
        /// </summary>
        [JsonPropertyName("coinToOiCap")]
        public HyperLiquidAssetValue[] OpenInterestCaps { get; set; } = [];
    }
}
