using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using HyperLiquid.Net.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Account ledger
    /// </summary>
    [JsonConverter(typeof(AccountLedgerConverter))]
    [SerializationModel]
    public record HyperLiquidAccountLedger
    {
        /// <summary>
        /// Deposits
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidDeposit[]> Deposits { get; set; } = [];
        /// <summary>
        /// Withdrawals
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidWithdrawal[]> Withdrawals { get; set; } = [];
        /// <summary>
        /// Internal transfers
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidInternalTransfer[]> InternalTransfer { get; set; } = [];
        /// <summary>
        /// Liquidations
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidLiquidation[]> Liquidations { get; set; } = [];
        /// <summary>
        /// Spot transfers
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidSpotTransfer[]> SpotTransfers { get; set; } = [];
    }

    /// <summary>
    /// Deposit info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidDeposit
    {
        /// <summary>
        /// USDC
        /// </summary>
        [JsonPropertyName("usdc")]
        public decimal Usdc { get; set; }
    }

    /// <summary>
    /// Withdrawal info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidWithdrawal
    {
        /// <summary>
        /// USDC
        /// </summary>
        [JsonPropertyName("usdc")]
        public decimal Usdc { get; set; }
        /// <summary>
        /// Nonce
        /// </summary>
        [JsonPropertyName("nonce")]
        public long Nonce { get; set; }
        /// <summary>
        /// Fee
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
    }

    /// <summary>
    /// Transfer
    /// </summary>
    [SerializationModel]
    public record HyperLiquidInternalTransfer
    {
        /// <summary>
        /// To futures
        /// </summary>
        [JsonPropertyName("toPerp")]
        public bool ToFutures { get; set; }
        /// <summary>
        /// USDC
        /// </summary>
        [JsonPropertyName("usdc")]
        public decimal Usdc { get; set; }
    }

    /// <summary>
    /// Liquidation
    /// </summary>
    [SerializationModel]
    public record HyperLiquidLiquidation
    {
        /// <summary>
        /// Margin type
        /// </summary>
        [JsonPropertyName("leverageType")]
        public MarginType MarginType { get; set; }
        /// <summary>
        /// Account value. For isolated positions this is the isolated account value
        /// </summary>
        [JsonPropertyName("accountValue")]
        public decimal AccountValue { get; set; }
        /// <summary>
        /// Liquidated positions
        /// </summary>
        [JsonPropertyName("liquidatedPositions")]
        public HyperLiquidLiquidationPosition[] Positions { get; set; } = [];
    }

    /// <summary>
    /// Liquidation position
    /// </summary>
    [SerializationModel]
    public record HyperLiquidLiquidationPosition
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("szi")]
        public decimal Quantity { get; set; }
    }

    /// <summary>
    /// Spot transfer info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidSpotTransfer
    {
        /// <summary>
        /// Token
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// USDC value
        /// </summary>
        [JsonPropertyName("usdcValue")]
        public decimal UsdcValue { get; set; }
        /// <summary>
        /// User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// Destination
        /// </summary>
        [JsonPropertyName("destination")]
        public string Destination { get; set; } = string.Empty;
        /// <summary>
        /// Fee
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
    }
}
