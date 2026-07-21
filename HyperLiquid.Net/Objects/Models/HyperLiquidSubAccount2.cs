using HyperLiquid.Net.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Sub account
    /// </summary>
    public record HyperLiquidSubAccount2
    {
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// ["<c>subAccountUser</c>"] Public address of the sub account
        /// </summary>
        [JsonPropertyName("subAccountUser")]
        public string SubAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>master</c>"] Public address of the master account
        /// </summary>
        [JsonPropertyName("master")]
        public string? MasterAddress { get; set; }
        /// <summary>
        /// ["<c>spotState</c>"] Spot balances
        /// </summary>
        [JsonPropertyName("spotState")]
        public HyperLiquidBalances SpotBalances { get; set; } = default!;
        /// <summary>
        /// ["<c>dexToClearinghouseState</c>"] Futures info
        /// </summary>
        [JsonPropertyName("dexToClearinghouseState")]
        [JsonConverter(typeof(ArrayDictConverter<HyperLiquidFuturesAccount>))]
        public Dictionary<string, HyperLiquidFuturesAccount> FuturesInfo { get; set; } = default!;
        /// <summary>
        /// ["<c>abstraction</c>"] Abstraction
        /// </summary>
        [JsonPropertyName("abstraction")]
        public string Abstraction { get; set; } = string.Empty;
    }
}
