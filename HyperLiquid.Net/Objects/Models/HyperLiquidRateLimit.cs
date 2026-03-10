using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Rate limit info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidRateLimit
    {
        /// <summary>
        /// ["<c>cumVlm</c>"] Total volume
        /// </summary>
        [JsonPropertyName("cumVlm")]
        public decimal TotalVolume { get; set; }
        /// <summary>
        /// ["<c>nRequestsUsed</c>"] Request quota used
        /// </summary>
        [JsonPropertyName("nRequestsUsed")]
        public long RequestsUsed { get; set; }
        /// <summary>
        /// ["<c>nRequestsCap</c>"] Request quota
        /// </summary>
        [JsonPropertyName("nRequestsCap")]
        public long RequestsCap { get; set; }
    }
}
