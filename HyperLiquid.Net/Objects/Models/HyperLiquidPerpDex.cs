using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Perp DEX info
    /// </summary>
    public record HyperLiquidPerpDex
    {
        /// <summary>
        /// Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Full name
        /// </summary>
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Deployer address
        /// </summary>
        [JsonPropertyName("deployer")]
        public string Deployer { get; set; } = string.Empty;
        /// <summary>
        /// Oracle updater address
        /// </summary>
        [JsonPropertyName("oracleUpdater")]
        public string OracleUpdater { get; set; } = string.Empty;
        /// <summary>
        /// Fee recipient address
        /// </summary>
        [JsonPropertyName("feeRecipient")]
        public string FeeRecipient { get; set; } = string.Empty;
        /// <summary>
        /// Asset to streaming open interest cap
        /// </summary>
        [JsonPropertyName("assetToStreamingOiCap")]
        public HyperLiquidAssetValue[] AssetToStreamingOpenInterestCap { get; set; } = [];
        /// <summary>
        /// Sub deployers
        /// </summary>
        [JsonPropertyName("subDeployers")]
        public HyperLiquidSubDeployer[] SubDeployers { get; set; } = [];
        /// <summary>
        /// Deployer fee scale
        /// </summary>
        [JsonPropertyName("deployerFeeScale")]
        public decimal DeployerFeeScale { get; set; }
        /// <summary>
        /// Last deployer fee scale change time
        /// </summary>
        [JsonPropertyName("lastDeployerFeeScaleChangeTime")]
        public DateTime LastDeployerFeeScaleChangeTime { get; set; }
        /// <summary>
        /// Asset to funding multiplier
        /// </summary>
        [JsonPropertyName("assetToFundingMultiplier")]
        public HyperLiquidAssetValue[] AssetToFundingMultiplier { get; set; } = [];
    }

    /// <summary>
    /// Sub deployer info
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidSubDeployer>))]
    public record HyperLiquidSubDeployer
    {
        /// <summary>
        /// Name
        /// </summary>
        [ArrayProperty(0)]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Addresses
        /// </summary>
        [ArrayProperty(1), JsonConversion]
        public string[] Addresses { get; set; } = [];
    }

    /// <summary>
    /// Asset value mapping
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidAssetValue>))]
    public record HyperLiquidAssetValue
    {
        /// <summary>
        /// Asset
        /// </summary>
        [ArrayProperty(0)]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// Cap
        /// </summary>
        [ArrayProperty(1)]
        public decimal? Value { get; set; }
    }
}
