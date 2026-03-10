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
        /// ["<c>time</c>"] Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime? Timestamp { get; set; }
        /// <summary>
        /// ["<c>coin</c>"] Symbol name
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>usdc</c>"] USDC
        /// </summary>
        [JsonPropertyName("usdc")]
        public decimal Usdc { get; set; }
        /// <summary>
        /// ["<c>szi</c>"] Quantity
        /// </summary>
        [JsonPropertyName("szi")]
        public decimal Szi { get; set; }
        /// <summary>
        /// ["<c>fundingRate</c>"] Funding rate
        /// </summary>
        [JsonPropertyName("fundingRate")]
        public decimal FundingRate { get; set; }
    }
}
