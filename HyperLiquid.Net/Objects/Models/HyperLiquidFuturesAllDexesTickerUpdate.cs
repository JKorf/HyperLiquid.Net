using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Dex tickers update
    /// </summary>
    public record HyperLiquidFuturesAllDexesTickerUpdate
    {
        /// <summary>
        /// ["<c>ctxs</c>"] Dex list
        /// </summary>
        [JsonPropertyName("ctxs")]
        public HyperLiquidFuturesAllDexTickerUpdate[] Dexes { get; set; } = [];
    }

    /// <summary>
    /// Dex tickers
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidFuturesAllDexTickerUpdate>))]
    public record HyperLiquidFuturesAllDexTickerUpdate
    {
        /// <summary>
        /// Dex name
        /// </summary>
        [ArrayProperty(0)]
        public string Dex { get; set; } = string.Empty;

        /// <summary>
        /// Tickers
        /// </summary>
        [ArrayProperty(1)]
        [JsonConversion]
        public HyperLiquidFuturesTicker[] Tickers { get; set; } = [];

    }
}
