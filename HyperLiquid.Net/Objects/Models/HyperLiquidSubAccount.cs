using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Sub account
    /// </summary>
    public record HyperLiquidSubAccount
    {
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>subAccountUser</c>"] Public address of the sub account
        /// </summary>
        [JsonPropertyName("subAccountUser")]
        public string SubAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>master</c>"] Public address of the master account
        /// </summary>
        [JsonPropertyName("master")]
        public string MasterAddress { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>spotState</c>"] Spot balances
        /// </summary>
        [JsonPropertyName("spotState")]
        public HyperLiquidBalances SpotBalances { get; set; } = default!;
        /// <summary>
        /// ["<c>clearinghouseState</c>"] Futures account info
        /// </summary>
        [JsonPropertyName("clearinghouseState")]
        public HyperLiquidFuturesAccount FuturesInfo { get; set; } = default!;
    }
}
