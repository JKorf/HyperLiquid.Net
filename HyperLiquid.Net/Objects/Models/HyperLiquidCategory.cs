using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Perp category
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperLiquidCategory>))]
    public record HyperLiquidCategory
    {
        /// <summary>
        /// Name
        /// </summary>
        [ArrayProperty(0)]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Category
        /// </summary>
        [ArrayProperty(1)]
        public string Category { get; set; } = string.Empty;
    }
}
