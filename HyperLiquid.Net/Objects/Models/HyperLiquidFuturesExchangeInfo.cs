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
        /// Symbols
        /// </summary>
        [JsonPropertyName("universe")]
        public HyperLiquidFuturesSymbol[] Symbols { get; set; } = [];
        /// <summary>
        /// Margin tables
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
        /// Symbol name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Decimal places for quantities
        /// </summary>
        [JsonPropertyName("szDecimals")]
        public int QuantityDecimals { get; set; }
        /// <summary>
        /// Max leverage
        /// </summary>
        [JsonPropertyName("maxLeverage")]
        public int MaxLeverage { get; set; }
        /// <summary>
        /// Only isolated margin
        /// </summary>
        [JsonPropertyName("onlyIsolated")]
        public bool OnlyIsolated { get; set; }
        /// <summary>
        /// Is delisted
        /// </summary>
        [JsonPropertyName("isDelisted")]
        public bool IsDelisted { get; set; }
        /// <summary>
        /// Margin table id
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
