using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Spot balances
    /// </summary>
    [SerializationModel]
    public record HyperLiquidBalances
    {
        /// <summary>
        /// ["<c>balances</c>"] Balances
        /// </summary>
        [JsonPropertyName("balances")]
        public HyperLiquidBalance[] Balances { get; set; } = [];
        /// <summary>
        /// ["<c>portfolioMarginEnabled</c>"] Portfolio margin enabled
        /// </summary>
        [JsonPropertyName("portfolioMarginEnabled")]
        public bool? PortfolioMarginEnabled { get; set; }
        /// <summary>
        /// ["<c>portfolioMarginRatio</c>"] Portfolio margin ratio
        /// </summary>
        [JsonPropertyName("portfolioMarginRatio")]
        public decimal? PortfolioMarginRatio { get; set; }
        /// <summary>
        /// ["<c>tokenToPortfolioBorrowRatio</c>"] Portfolio margin ratio
        /// </summary>
        [JsonConverter(typeof(TokenToConverter))]
        [JsonPropertyName("tokenToPortfolioBorrowRatio")]
        public Dictionary<long, decimal>? TokenPortfolioBorrowRatio { get; set; }
        /// <summary>
        /// ["<c>tokenToPortfolioSupplyRatio</c>"] Portfolio margin ratio
        /// </summary>
        [JsonConverter(typeof(TokenToConverter))]
        [JsonPropertyName("tokenToPortfolioSupplyRatio")]
        public Dictionary<long, decimal>? TokenPortfolioSupplyRatio { get; set; }
        /// <summary>
        /// ["<c>tokenToAvailableAfterMaintenance</c>"] Portfolio margin ratio
        /// </summary>
        [JsonConverter(typeof(TokenToConverter))]
        [JsonPropertyName("tokenToAvailableAfterMaintenance")]
        public Dictionary<long, decimal>? TokenAvailableAfterMaintenance { get; set; }
    }

    /// <summary>
    /// Balance info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidBalance
    {
        /// <summary>
        /// ["<c>coin</c>"] Asset
        /// </summary>
        [JsonPropertyName("coin")]
        public string Asset { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>token</c>"] Token
        /// </summary>
        [JsonPropertyName("token")]
        public int Token { get; set; }
        /// <summary>
        /// ["<c>hold</c>"] In holding
        /// </summary>
        [JsonPropertyName("hold")]
        public decimal Hold { get; set; }
        /// <summary>
        /// ["<c>total</c>"] Total
        /// </summary>
        [JsonPropertyName("total")]
        public decimal Total { get; set; }
        /// <summary>
        /// ["<c>entryNtl</c>"] Entry notional
        /// </summary>
        [JsonPropertyName("entryNtl")]
        public decimal EntryNotional { get; set; }
        /// <summary>
        /// ["<c>ltv</c>"] LTV
        /// </summary>
        [JsonPropertyName("ltv")]
        public decimal? Ltv { get; set; }
        /// <summary>
        /// ["<c>supplied</c>"] Supplied
        /// </summary>
        [JsonPropertyName("supplied")]
        public decimal? Supplied { get; set; }
    }
}
