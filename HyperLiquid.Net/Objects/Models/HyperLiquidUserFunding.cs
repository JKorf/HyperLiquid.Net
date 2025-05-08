using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User funding
    /// </summary>
    [SerializationModel]
    public record HyperLiquidUserFunding
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Symbol name
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// USDC
        /// </summary>
        [JsonPropertyName("usdc")]
        public decimal Usdc { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("szi")]
        public decimal Szi { get; set; }
        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonPropertyName("fundingRate")]
        public decimal FundingRate { get; set; }
    }
}
