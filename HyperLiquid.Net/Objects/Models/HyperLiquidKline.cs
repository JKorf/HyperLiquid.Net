using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Kline/candlestick info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidKline
    {
        /// <summary>
        /// Open timestamp
        /// </summary>
        [JsonPropertyName("t")]
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Close timestamp
        /// </summary>
        [JsonPropertyName("T")]
        public DateTime CloseTime { get; set; }
        /// <summary>
        /// Kline interval
        /// </summary>
        [JsonPropertyName("i")]
        public KlineInterval Interval { get; set; }
        /// <summary>
        /// Close price
        /// </summary>
        [JsonPropertyName("c")]
        public decimal ClosePrice { get; set; }
        /// <summary>
        /// High price
        /// </summary>
        [JsonPropertyName("h")]
        public decimal HighPrice { get; set; }
        /// <summary>
        /// Low price
        /// </summary>
        [JsonPropertyName("l")]
        public decimal LowPrice { get; set; }
        /// <summary>
        /// Open price
        /// </summary>
        [JsonPropertyName("o")]
        public decimal OpenPrice { get; set; }
        /// <summary>
        /// Volume
        /// </summary>
        [JsonPropertyName("v")]
        public decimal Volume { get; set; }
        /// <summary>
        /// Trade count
        /// </summary>
        [JsonPropertyName("n")]
        public int TradeCount { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("s")]
        public string Symbol { get; set; } = string.Empty;
    }
}
