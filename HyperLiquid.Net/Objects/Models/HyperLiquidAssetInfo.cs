using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Asset info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidAssetInfo
    {
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>maxSupply</c>"] Max supply
        /// </summary>
        [JsonPropertyName("maxSupply")]
        public decimal MaxSupply { get; set; }
        /// <summary>
        /// ["<c>totalSupply</c>"] Total supply
        /// </summary>
        [JsonPropertyName("totalSupply")]
        public decimal TotalSupply { get; set; }
        /// <summary>
        /// ["<c>circulatingSupply</c>"] Circulating supply
        /// </summary>
        [JsonPropertyName("circulatingSupply")]
        public decimal CirculatingSupply { get; set; }
        /// <summary>
        /// ["<c>szDecimals</c>"] Quantity decimals
        /// </summary>
        [JsonPropertyName("szDecimals")]
        public decimal QuantityDecimals { get; set; }
        /// <summary>
        /// ["<c>weiDecimals</c>"] Wei decimals
        /// </summary>
        [JsonPropertyName("weiDecimals")]
        public decimal WeiDecimals { get; set; }
        /// <summary>
        /// ["<c>midPx</c>"] Mid price
        /// </summary>
        [JsonPropertyName("midPx")]
        public decimal MidPrice { get; set; }
        /// <summary>
        /// ["<c>markPx</c>"] Mark price
        /// </summary>
        [JsonPropertyName("markPx")]
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// ["<c>prevDayPx</c>"] Previous day price
        /// </summary>
        [JsonPropertyName("prevDayPx")]
        public decimal PreviousDayPrice { get; set; }
        /// <summary>
        /// ["<c>genesis</c>"] Genesis
        /// </summary>
        [JsonPropertyName("genesis")]
        public HyperLiquidAssetGenesis Genesis { get; set; } = null!;
        /// <summary>
        /// ["<c>deployer</c>"] Deployer
        /// </summary>
        [JsonPropertyName("deployer")]
        public string Deployer { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>deployGas</c>"] Deploy gas
        /// </summary>
        [JsonPropertyName("deployGas")]
        public decimal? DeployGas { get; set; }
        /// <summary>
        /// ["<c>deployTime</c>"] Deploy time
        /// </summary>
        [JsonPropertyName("deployTime")]
        public DateTime DeployTime { get; set; }
        /// <summary>
        /// ["<c>seededUsdc</c>"] Seeded usdc
        /// </summary>
        [JsonPropertyName("seededUsdc")]
        public decimal SeededUsdc { get; set; }
        /// <summary>
        /// ["<c>futureEmissions</c>"] Future emissions
        /// </summary>
        [JsonPropertyName("futureEmissions")]
        public decimal FutureEmissions { get; set; }

        /// <summary>
        /// ["<c>nonCirculatingUserBalances</c>"] Non-circulating user balances
        /// </summary>
        [JsonPropertyName("nonCirculatingUserBalances")]
        public AddressBalance[] NonCirculatingUserBalances { get; set; } = [];
    }

    /// <summary>
    /// Genesis balances
    /// </summary>
    [SerializationModel]
    public record HyperLiquidAssetGenesis
    {
        /// <summary>
        /// ["<c>userBalances</c>"] User balances
        /// </summary>
        [JsonPropertyName("userBalances")]
        public AddressBalance[] UserBalances { get; set; } = [];
        /// <summary>
        /// ["<c>existingTokenBalances</c>"] Existing token balances
        /// </summary>
        [JsonPropertyName("existingTokenBalances")]
        public AddressIndexBalance[] ExistingAssetBalances { get; set; } = [];
    }

    /// <summary>
    /// Address balance
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<AddressBalance>))]
    [SerializationModel]
    public record AddressBalance
    {
        /// <summary>
        /// Address
        /// </summary>
        [ArrayProperty(0)]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Balance
        /// </summary>
        [ArrayProperty(1)]
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// Address index balance reference
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<AddressIndexBalance>))]
    [SerializationModel]
    public record AddressIndexBalance
    {
        /// <summary>
        /// Address index
        /// </summary>
        [ArrayProperty(0)]
        public int Index { get; set; }
        /// <summary>
        /// Balance
        /// </summary>
        [ArrayProperty(1)]
        public decimal Balance { get; set; }
    }
}
