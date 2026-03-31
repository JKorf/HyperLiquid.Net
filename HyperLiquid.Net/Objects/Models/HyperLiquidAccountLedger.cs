using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using HyperLiquid.Net.Enums;
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
        public HyperLiquidUserLedger<HyperLiquidDeposit>[] Deposits { get; set; } = [];
        /// <summary>
        /// Withdrawals
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidWithdrawal>[] Withdrawals { get; set; } = [];
        /// <summary>
        /// Internal transfers
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidInternalTransfer>[] InternalTransfer { get; set; } = [];
        /// <summary>
        /// Liquidations
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidLiquidation>[] Liquidations { get; set; } = [];
        /// <summary>
        /// Spot transfers
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidSpotTransfer>[] SpotTransfers { get; set; } = [];
        /// <summary>
        /// External transfers (send type)
        /// </summary>
        public HyperLiquidUserLedger<HyperLiquidExternalTransfer>[] ExternalTransfers { get; set; } = [];
    }

    /// <summary>
    /// Deposit info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidDeposit
    {
        /// <summary>
        /// ["<c>usdc</c>"] USDC
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
        /// ["<c>usdc</c>"] USDC
        /// </summary>
        [JsonPropertyName("usdc")]
        public decimal Usdc { get; set; }
        /// <summary>
        /// ["<c>nonce</c>"] Nonce
        /// </summary>
        [JsonPropertyName("nonce")]
        public long Nonce { get; set; }
        /// <summary>
        /// ["<c>fee</c>"] Fee
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
        /// ["<c>toPerp</c>"] To futures
        /// </summary>
        [JsonPropertyName("toPerp")]
        public bool ToFutures { get; set; }
        /// <summary>
        /// ["<c>usdc</c>"] USDC
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
        /// ["<c>leverageType</c>"] Margin type
        /// </summary>
        [JsonPropertyName("leverageType")]
        public MarginType MarginType { get; set; }
        /// <summary>
        /// ["<c>accountValue</c>"] Account value. For isolated positions this is the isolated account value
        /// </summary>
        [JsonPropertyName("accountValue")]
        public decimal AccountValue { get; set; }
        /// <summary>
        /// ["<c>liquidatedPositions</c>"] Liquidated positions
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
        /// ["<c>coin</c>"] Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>szi</c>"] Quantity
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
        /// ["<c>token</c>"] Token
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>amount</c>"] Quantity
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// ["<c>usdcValue</c>"] USDC value
        /// </summary>
        [JsonPropertyName("usdcValue")]
        public decimal UsdcValue { get; set; }
        /// <summary>
        /// ["<c>user</c>"] User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>destination</c>"] Destination
        /// </summary>
        [JsonPropertyName("destination")]
        public string Destination { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>fee</c>"] Fee
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
    }

    /// <summary>
    /// External transfer info (send type)
    /// </summary>
    [SerializationModel]
    public record HyperLiquidExternalTransfer
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
        /// Source dex
        /// </summary>
        [JsonPropertyName("sourceDex")]
        public string SourceDex { get; set; } = string.Empty;
        /// <summary>
        /// Destination dex
        /// </summary>
        [JsonPropertyName("destinationDex")]
        public string DestinationDex { get; set; } = string.Empty;
        /// <summary>
        /// Fee
        /// </summary>
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
        /// <summary>
        /// Native token fee
        /// </summary>
        [JsonPropertyName("nativeTokenFee")]
        public decimal NativeTokenFee { get; set; }
        /// <summary>
        /// Nonce
        /// </summary>
        [JsonPropertyName("nonce")]
        public long Nonce { get; set; }
        /// <summary>
        /// Fee token
        /// </summary>
        [JsonPropertyName("feeToken")]
        public string FeeToken { get; set; } = string.Empty;
    }
}
