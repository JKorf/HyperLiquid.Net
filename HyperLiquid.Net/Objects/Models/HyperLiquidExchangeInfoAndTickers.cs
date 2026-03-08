using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
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
        /// ["<c>prevDayPx</c>"] Previous day price
        /// </summary>
        [JsonPropertyName("prevDayPx")]
        public decimal PreviousDayPrice { get; set; }
        /// <summary>
        /// ["<c>dayNtlVlm</c>"] 24h notional volume
        /// </summary>
        [JsonPropertyName("dayNtlVlm")]
        public decimal QuoteVolume { get; set; }
        /// <summary>
        /// ["<c>markPx</c>"] Mark price
        /// </summary>
        [JsonPropertyName("markPx")]
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// ["<c>midPx</c>"] Mid price
        /// </summary>
        [JsonPropertyName("midPx")]
        public decimal? MidPrice { get; set; }
        /// <summary>
        /// ["<c>circulatingSupply</c>"] Circulation supply
        /// </summary>
        [JsonPropertyName("circulatingSupply")]
        public decimal CirculatingSupply { get; set; }
        /// <summary>
        /// ["<c>totalSupply</c>"] Total supply
        /// </summary>
        [JsonPropertyName("totalSupply")]
        public decimal TotalSupply { get; set; }
        /// <summary>
        /// ["<c>dayBaseVlm</c>"] 24h base volume
        /// </summary>
        [JsonPropertyName("dayBaseVlm")]
        public decimal BaseVolume { get; set; }
        /// <summary>
        /// ["<c>coin</c>"] Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string? Symbol { get; set; }
    }
}
