using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Futures exchange info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFuturesExchangeInfo
    {
        /// <summary>
        /// ["<c>universe</c>"] Symbols
        /// </summary>
        [JsonPropertyName("universe")]
        public HyperLiquidFuturesSymbol[] Symbols { get; set; } = [];
        /// <summary>
        /// ["<c>marginTables</c>"] Margin tables
        /// </summary>
        [JsonPropertyName("marginTables")]
        public HyperLiquidMarginTable[] MarginTables { get; set; } = [];
    }

    /// <summary>
    /// Futures symbol info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFuturesSymbol
    {
        /// <summary>
        /// ["<c>name</c>"] Symbol name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>szDecimals</c>"] Decimal places for quantities
        /// </summary>
        [JsonPropertyName("szDecimals")]
        public int QuantityDecimals { get; set; }
        /// <summary>
        /// ["<c>maxLeverage</c>"] Max leverage
        /// </summary>
        [JsonPropertyName("maxLeverage")]
        public int MaxLeverage { get; set; }
        /// <summary>
        /// ["<c>onlyIsolated</c>"] Only isolated margin
        /// </summary>
        [JsonPropertyName("onlyIsolated")]
        public bool OnlyIsolated { get; set; }
        /// <summary>
        /// ["<c>isDelisted</c>"] Is delisted
        /// </summary>
        [JsonPropertyName("isDelisted")]
        public bool IsDelisted { get; set; }
        /// <summary>
        /// ["<c>marginTableId</c>"] Margin table id
        /// </summary>
        [JsonPropertyName("marginTableId")]
        public int MarginTableId { get; set; }
        /// <summary>
        /// Margin table
        /// </summary>
        [JsonIgnore]
        public HyperLiquidMarginTableEntry MarginTable { get; set; } = default!;
        /// <summary>
        /// Index
        /// </summary>
        [JsonIgnore]
        public int Index { get; set; }
    }
}
