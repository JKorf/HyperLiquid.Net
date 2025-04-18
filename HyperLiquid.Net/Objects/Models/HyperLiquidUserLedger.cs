using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Funding history item
    /// </summary>
    [SerializationModel]
    public record HyperLiquidUserLedger<T>
    {
        /// <summary>
        /// Data
        /// </summary>
        [JsonPropertyName("delta")]
        public T Data { get; set; } = default!;
        /// <summary>
        /// Hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
    }

}
