﻿using HyperLiquid.Net.Converters;
using HyperLiquid.Net.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Account ledger
    /// </summary>
    [JsonConverter(typeof(AccountLedgerConverter))]
    public record HyperLiquidAccountLedger
    {
        /// <summary>
        /// Deposits
        /// </summary>
        public IEnumerable<HyperLiquidUserLedger<HyperLiquidDeposit>> Deposits { get; set; } = [];
        /// <summary>
        /// Withdrawals
        /// </summary>
        public IEnumerable<HyperLiquidUserLedger<HyperLiquidWithdrawal>> Withdrawals { get; set; } = [];
        /// <summary>
        /// Internal transfers
        /// </summary>
        public IEnumerable<HyperLiquidUserLedger<HyperLiquidInternalTransfer>> InternalTransfer { get; set; } = [];
        /// <summary>
        /// Liquidations
        /// </summary>
        public IEnumerable<HyperLiquidUserLedger<HyperLiquidLiquidation>> Liquidations { get; set; } = [];
        /// <summary>
        /// Spot transfers
        /// </summary>
        public IEnumerable<HyperLiquidUserLedger<HyperLiquidSpotTransfer>> SpotTransfers { get; set; } = [];
    }

    /// <summary>
    /// Deposit info
    /// </summary>
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
        public IEnumerable<HyperLiquidLiquidationPosition> Positions { get; set; } = [];
    }

    /// <summary>
    /// Liquidation position
    /// </summary>
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
