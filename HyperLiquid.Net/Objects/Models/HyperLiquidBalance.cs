using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Spot balances
    /// </summary>
    [SerializationModel]
    public record HyperLiquidBalances
    {
        /// <summary>
        /// Balances
        /// </summary>
        [JsonPropertyName("balances")]
        public HyperLiquidBalance[] Balances { get; set; } = [];
    }

    /// <summary>
    /// Balance info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidBalance
    {
        /// <summary>
        /// Asset
        /// </summary>
        [JsonPropertyName("coin")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Token
        /// </summary>
        [JsonPropertyName("token")]
        public int Token { get; set; }
        /// <summary>
        /// In holding
        /// </summary>
        [JsonPropertyName("hold")]
        public decimal Hold { get; set; }
        /// <summary>
        /// Total
        /// </summary>
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
        /// <summary>
        /// Entry notional
        /// </summary>
        [JsonPropertyName("entryNtl")]
        public decimal EntryNotional { get; set; }
    }
}
