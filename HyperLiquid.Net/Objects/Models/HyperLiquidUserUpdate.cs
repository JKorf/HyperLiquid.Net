using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User update
    /// </summary>
    [SerializationModel]
    public record HyperLiquidUserUpdate
    {
        /// <summary>
        /// Total value of ledger
        /// </summary>
        [JsonPropertyName("cumLedger")]
        public decimal CumLedger { get; set; }
        /// <summary>
        /// Server time
        /// </summary>
        [JsonPropertyName("serverTime")]
        public DateTime ServerTime { get; set; }
        /// <summary>
        /// Is vault
        /// </summary>
        [JsonPropertyName("isVault")]
        public bool IsVault { get; set; }
        /// <summary>
        /// User
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
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
