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
        /// Total volume
        /// </summary>
        [JsonPropertyName("cumVlm")]
        public decimal TotalVolume { get; set; }
        /// <summary>
        /// Request quota used
        /// </summary>
        [JsonPropertyName("nRequestsUsed")]
        public long RequestsUsed { get; set; }
        /// <summary>
        /// Request quota
        /// </summary>
        [JsonPropertyName("nRequestsCap")]
        public long RequestsCap { get; set; }
    }
}
