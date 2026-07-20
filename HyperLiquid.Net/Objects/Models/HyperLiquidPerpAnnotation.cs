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
    /// Perp annotations
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidPerpAnnotation>))]
    public record HyperLiquidPerpAnnotation
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        [ArrayProperty(0)]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Annotations
        /// </summary>
        [ArrayProperty(1)]
        [JsonConversion]
        public HyperLiquidPerpAnnotationDetails Annotations { get; set; } = default!;
    }

    /// <summary>
    /// Annotations
    /// </summary>
    public record HyperLiquidPerpAnnotationDetails
    {
        /// <summary>
        /// ["<c>category</c>"] Category
        /// </summary>
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>displayName</c>"] Display name
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
        /// <summary>
        /// ["<c>keywords</c>"] Keywords
        /// </summary>
        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; } = [];
    }
}
