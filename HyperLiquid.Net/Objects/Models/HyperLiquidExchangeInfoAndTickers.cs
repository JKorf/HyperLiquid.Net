using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Exchange and ticker info
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidExchangeInfoAndTickers>))]
    [SerializationModel]
    public record HyperLiquidExchangeInfoAndTickers
    {
        /// <summary>
        /// Exchange info
        /// </summary>
        [ArrayProperty(0), JsonConversion]
        public HyperLiquidSpotExchangeInfo ExchangeInfo { get; set; } = default!;

        /// <summary>
        /// Tickers
        /// </summary>
        [ArrayProperty(1), JsonConversion]
        public HyperLiquidTicker[] Tickers { get; set; } = [];
    }

    /// <summary>
    /// Ticker info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidTicker
    {
        /// <summary>
        /// Previous day price
        /// </summary>
        [JsonPropertyName("prevDayPx")]
        public decimal PreviousDayPrice { get; set; }
        /// <summary>
        /// 24h notional volume
        /// </summary>
        [JsonPropertyName("dayNtlVlm")]
        public decimal QuoteVolume { get; set; }
        /// <summary>
        /// Mark price
        /// </summary>
        [JsonPropertyName("markPx")]
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// Mid price
        /// </summary>
        [JsonPropertyName("midPx")]
        public decimal? MidPrice { get; set; }
        /// <summary>
        /// Circulation supply
        /// </summary>
        [JsonPropertyName("circulatingSupply")]
        public decimal CirculatingSupply { get; set; }
        /// <summary>
        /// Total supply
        /// </summary>
        [JsonPropertyName("totalSupply")]
        public decimal TotalSupply { get; set; }
        /// <summary>
        /// 24h base volume
        /// </summary>
        [JsonPropertyName("dayBaseVlm")]
        public decimal BaseVolume { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string? Symbol { get; set; }
    }
}
