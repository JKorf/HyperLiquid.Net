using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using HyperLiquid.Net.Converters;
using System;
using System.Collections.Generic;
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
        /// Symbol 
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Data timestamp
        /// </summary>
        [JsonPropertyName("time")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Levels
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
        /// Price
        /// </summary>
        [JsonPropertyName("px")]
        public decimal Price { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("sz")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Number of orders
        /// </summary>
        [JsonPropertyName("n")]
        public int Orders { get; set; }
    }
}
