using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User update
    /// </summary>
    [SerializationModel]
    public record HyperLiquidWebDataV3Update
    {
        /// <summary>
        /// ["<c>userState</c>"] User state
        /// </summary>
        [JsonPropertyName("userState")]
        public HyperLiquidUserV3State UserState { get; set; } = default!;
        /// <summary>
        /// ["<c>userState</c>"] Perp dex states
        /// </summary>
        [JsonPropertyName("perpDexStates")]
        public PerpDexState[] PerpDexStates { get; set; } = [];
    }

    /// <summary>
    /// Perp dex info
    /// </summary>
    public record PerpDexState
    {
        /// <summary>
        /// ["<c>totalVaultEquity</c>"] Total vault equity
        /// </summary>
        [JsonPropertyName("totalVaultEquity")]
        public decimal TotalVaultEquity { get; set; }
        /// <summary>
        /// ["<c>perpsAtOpenInterestCap</c>"] Perps at open interest cap
        /// </summary>
        [JsonPropertyName("perpsAtOpenInterestCap")]
        public string[] PerpsAtOpenInterestCap { get; set; } = [];
        /// <summary>
        /// ["<c>leadingVaults</c>"] Leading vaults
        /// </summary>
        [JsonPropertyName("leadingVaults")]
        public HyperliquidLeadingVault[] LeadingVaults { get; set; } = [];
    }

    /// <summary>
    /// Leading vault
    /// </summary>
    public record HyperliquidLeadingVault
    {
        /// <summary>
        /// ["<c>address</c>"] Address
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// User state
    /// </summary>
    public record HyperLiquidUserV3State
    {
        /// <summary>
        /// ["<c>agentAddress</c>"] Agent address
        /// </summary>
        [JsonPropertyName("agentAddress")]
        public string AgentAddress { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>agentValidUntil</c>"] Agent valid until
        /// </summary>
        [JsonPropertyName("agentValidUntil")]
        public DateTime? AgentValidUntil { get; set; }
        /// <summary>
        /// ["<c>cumLedger</c>"] Total value of ledger
        /// </summary>
        [JsonPropertyName("cumLedger")]
        public decimal CumLedger { get; set; }
        /// <summary>
        /// ["<c>serverTime</c>"] Server time
        /// </summary>
        [JsonPropertyName("serverTime")]
        public DateTime ServerTime { get; set; }
        /// <summary>
        /// ["<c>isVault</c>"] Is vault
        /// </summary>
        [JsonPropertyName("isVault")]
        public bool IsVault { get; set; }
        /// <summary>
        /// ["<c>user</c>"] User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>dexAbstractionEnabled</c>"] DEX abstraction enabled
        /// </summary>
        [JsonPropertyName("dexAbstractionEnabled")]
        public bool DexAbstractionEnabled { get; set; }
        /// <summary>
        /// ["<c>optOutOfSpotDusting</c>"] User
        /// </summary>
        [JsonPropertyName("optOutOfSpotDusting")]
        public bool? OptOutOfSpotDusting { get; set; }
    }
}
