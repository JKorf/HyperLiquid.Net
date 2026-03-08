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
        /// ["<c>balances</c>"] Balances
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
        /// ["<c>coin</c>"] Asset
        /// </summary>
        [JsonPropertyName("coin")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>token</c>"] Token
        /// </summary>
        [JsonPropertyName("token")]
        public int Token { get; set; }
        /// <summary>
        /// ["<c>hold</c>"] In holding
        /// </summary>
        [JsonPropertyName("hold")]
        public decimal Hold { get; set; }
        /// <summary>
        /// ["<c>total</c>"] Total
        /// </summary>
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
        /// <summary>
        /// ["<c>entryNtl</c>"] Entry notional
        /// </summary>
        [JsonPropertyName("entryNtl")]
        public decimal EntryNotional { get; set; }
    }
}
