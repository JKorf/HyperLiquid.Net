using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using System;
using System.Collections.Generic;
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
        /// Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Max supply
        /// </summary>
        [JsonPropertyName("maxSupply")]
        public decimal MaxSupply { get; set; }
        /// <summary>
        /// Total supply
        /// </summary>
        [JsonPropertyName("totalSupply")]
        public decimal TotalSupply { get; set; }
        /// <summary>
        /// Circulating supply
        /// </summary>
        [JsonPropertyName("circulatingSupply")]
        public decimal CirculatingSupply { get; set; }
        /// <summary>
        /// Quantity decimals
        /// </summary>
        [JsonPropertyName("szDecimals")]
        public decimal QuantityDecimals { get; set; }
        /// <summary>
        /// Wei decimals
        /// </summary>
        [JsonPropertyName("weiDecimals")]
        public decimal WeiDecimals { get; set; }
        /// <summary>
        /// Mid price
        /// </summary>
        [JsonPropertyName("midPx")]
        public decimal MidPrice { get; set; }
        /// <summary>
        /// Mark price
        /// </summary>
        [JsonPropertyName("markPx")]
        public decimal MarkPrice { get; set; }
        /// <summary>
        /// Previous day price
        /// </summary>
        [JsonPropertyName("prevDayPx")]
        public decimal PreviousDayPrice { get; set; }
        /// <summary>
        /// Genesis
        /// </summary>
        [JsonPropertyName("genesis")]
        public HyperLiquidAssetGenesis Genesis { get; set; } = null!;
        /// <summary>
        /// Deployer
        /// </summary>
        [JsonPropertyName("deployer")]
        public string Deployer { get; set; } = string.Empty;
        /// <summary>
        /// Deploy gas
        /// </summary>
        [JsonPropertyName("deployGas")]
        public decimal? DeployGas { get; set; }
        /// <summary>
        /// Deploy time
        /// </summary>
        [JsonPropertyName("deployTime")]
        public DateTime DeployTime { get; set; }
        /// <summary>
        /// Seeded usdc
        /// </summary>
        [JsonPropertyName("seededUsdc")]
        public decimal SeededUsdc { get; set; }
        /// <summary>
        /// Future emissions
        /// </summary>
        [JsonPropertyName("futureEmissions")]
        public decimal FutureEmissions { get; set; }

        /// <summary>
        /// Non-circulating user balances
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
        /// User balances
        /// </summary>
        [JsonPropertyName("userBalances")]
        public AddressBalance[] UserBalances { get; set; } = [];
        /// <summary>
        /// Existing token balances
        /// </summary>
        [JsonPropertyName("existingTokenBalances")]
        public AddressIndexBalance[] ExistingAssetBalances { get; set; } = [];
    }

    /// <summary>
    /// Address balance
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<AddressBalance, HyperLiquidSourceGenerationContext>))]
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
    [JsonConverter(typeof(ArrayConverter<AddressIndexBalance, HyperLiquidSourceGenerationContext>))]
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
