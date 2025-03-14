using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    [SerializationModel]
    internal record HyperLiquidOrderResultIntWrapper
    {
        [JsonPropertyName("statuses")]
        public HyperLiquidOrderResultInt[] Statuses { get; set; } = [];
    }

    [SerializationModel]
    internal record HyperLiquidOrderResultInt
    {
        [JsonPropertyName("resting")]
        public HyperLiquidOrderResult? ResultResting { get; set; }
        [JsonPropertyName("filled")]
        public HyperLiquidOrderResult? ResultFilled { get; set; }
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    /// <summary>
    /// Order result
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOrderResult
    {
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
        /// <summary>
        /// Order status
        /// </summary>
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("totalSz")]
        public decimal? FilledQuantity { get; set; }
        /// <summary>
        /// Average fill price
        /// </summary>
        [JsonPropertyName("avgPx")]
        public decimal? AveragePrice { get; set; }
    }
}
