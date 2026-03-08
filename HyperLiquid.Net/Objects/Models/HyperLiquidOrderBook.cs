using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Order book
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOrderBook
    {
        /// <summary>
        /// ["<c>coin</c>"] Symbol 
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>time</c>"] Data timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// ["<c>levels</c>"] Levels
        /// </summary>
        [JsonPropertyName("levels")]
        public HyperLiquidOrderBookLevels Levels { get; set; } = null!;
    }

    /// <summary>
    /// Order book levels
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidOrderBookLevels>))]
    [SerializationModel]
    public record HyperLiquidOrderBookLevels
    {
        /// <summary>
        /// Bids
        /// </summary>
        [ArrayProperty(0), JsonConversion]
        public HyperLiquidOrderBookEntry[] Bids { get; set; } = [];
        /// <summary>
        /// Asks
        /// </summary>
        [ArrayProperty(1), JsonConversion]
        public HyperLiquidOrderBookEntry[] Asks { get; set; } = [];
    }

    /// <summary>
    /// Order book entry
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOrderBookEntry : ISymbolOrderBookEntry
    {
        /// <summary>
        /// ["<c>px</c>"] Price
        /// </summary>
        [JsonPropertyName("px")]
        public decimal Price { get; set; }
        /// <summary>
        /// ["<c>sz</c>"] Quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>n</c>"] Number of orders
        /// </summary>
        [JsonPropertyName("n")]
        public int Orders { get; set; }
    }
}
