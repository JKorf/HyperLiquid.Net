using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    [SerializationModel]
    internal record HyperLiquidTwapOrderResultIntWrapper
    {
        [JsonPropertyName("status")]
        public HyperLiquidTwapOrderResultInt Status { get; set; } = default!;
    }

    [SerializationModel]
    internal record HyperLiquidTwapOrderResultInt
    {
        [JsonPropertyName("running")]
        public HyperLiquidTwapOrderResult? ResultRunning { get; set; }
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    /// <summary>
    /// Order result
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTwapOrderResult
    {
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("twapId")]
        public long TwapId { get; set; }
    }
}
