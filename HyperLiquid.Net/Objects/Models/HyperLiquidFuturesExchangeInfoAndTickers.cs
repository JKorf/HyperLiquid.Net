using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Exchange and ticker info
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidFuturesExchangeInfoAndTickers>))]
    [SerializationModel]
    public record HyperLiquidFuturesExchangeInfoAndTickers
    {
        /// <summary>
        /// Exchange info
        /// </summary>
        [ArrayProperty(0), JsonConversion]
        public HyperLiquidFuturesExchangeInfo ExchangeInfo { get; set; } = default!;

        /// <summary>
        /// Tickers
        /// </summary>
        [ArrayProperty(1), JsonConversion]
        public HyperLiquidFuturesTicker[] Tickers { get; set; } = [];
    }

    /// <summary>
    /// Ticker info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFuturesTicker
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        [JsonIgnore]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Impact prices
        /// </summary>
        [JsonPropertyName("impactPxs")]
        public decimal[] ImpactPrices { get; set; } = [];
        /// <summary>
        /// Funding rate
        /// </summary>
        [JsonPropertyName("funding")]
        public decimal? FundingRate { get; set; }
        /// <summary>
        /// Open interest
        /// </summary>
        [JsonPropertyName("openInterest")]
        public decimal? OpenInterest { get; set; }
        /// <summary>
        /// Oracle price
        /// </summary>
        [JsonPropertyName("oraclePx")]
        public decimal? OraclePrice { get; set; }
        /// <summary>
        /// Premium
        /// </summary>
        [JsonPropertyName("premium")]
        public decimal? Premium { get; set; }
        /// <summary>
        /// Previous day price
        /// </summary>
        [JsonPropertyName("prevDayPx")]
        public decimal PreviousDayPrice { get; set; }
        /// <summary>
        /// 24h notional volume
        /// </summary>
        [JsonPropertyName("dayNtlVlm")]
        public decimal NotionalVolume { get; set; }
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
    }
}
