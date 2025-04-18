using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    [SerializationModel]
    internal record HyperLiquidOrderStatusResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("order")]
        public HyperLiquidOrderStatus? Order { get; set; }
    }

    /// <summary>
    /// Order status
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOrderStatus
    {
        /// <summary>
        /// Status timestamp
        /// </summary>
        [JsonPropertyName("statusTimestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Order status
        /// </summary>
        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Order info
        /// </summary>
        [JsonPropertyName("order")]
        public HyperLiquidOrder Order { get; set; } = default!;

    }
}
