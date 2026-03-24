using CryptoExchange.Net.Converters.SystemTextJson;
using System;
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
        /// ["<c>openOrders</c>"] Open orders
        /// </summary>
        [JsonPropertyName("openOrders")]
        public HyperLiquidOrder[] OpenOrders { get; set; } = default!;
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
        /// <summary>
        /// ["<c>meta</c>"] Futures exchange info
        /// </summary>
        [JsonPropertyName("meta")]
        public HyperLiquidFuturesExchangeInfo FuturesExchangeInfo { get; set; } = default!;
        /// <summary>
        /// ["<c>spotAssetCtxs</c>"] Spot tickers
        /// </summary>
        [JsonPropertyName("spotAssetCtxs")]
        public HyperLiquidTicker[] SpotTickers { get; set; } = default!;
        /// <summary>
        /// ["<c>perpsAtOpenInterestCap</c>"] Spot tickers
        /// </summary>
        [JsonPropertyName("perpsAtOpenInterestCap")]
        public string[] PerpsAtOpenInterestCap { get; set; } = [];
    }
}
