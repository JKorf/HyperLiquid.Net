using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Sub account
    /// </summary>
    public record HyperLiquidSubAccount
    {
        /// <summary>
        /// Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Public address of the sub account
        /// </summary>
        [JsonPropertyName("subAccountUser")]
        public string SubAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// Public address of the master account
        /// </summary>
        [JsonPropertyName("master")]
        public string MasterAddress { get; set; } = string.Empty;
        /// <summary>
        /// Spot balances
        /// </summary>
        [JsonPropertyName("spotState")]
        public HyperLiquidBalances SpotBalances { get; set; } = default!;
        /// <summary>
        /// Futures account info
        /// </summary>
        [JsonPropertyName("clearinghouseState")]
        public HyperLiquidFuturesAccount FuturesInfo { get; set; } = default!;
    }
}
