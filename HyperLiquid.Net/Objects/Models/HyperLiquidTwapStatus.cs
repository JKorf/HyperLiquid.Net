using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Twap status
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTwapStatus
    {
        /// <summary>
        /// ID of the Twap Order
        /// </summary>
        [JsonPropertyName("twapId")]
        public long TwapId { get; set; }
        /// <summary>
        /// Fill
        /// </summary>
        [JsonPropertyName("fill")]
        public HyperLiquidUserTrade Fill { get; set; } = default!;
    }
}
