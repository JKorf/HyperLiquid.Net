using System.Text.Json.Serialization;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Margin type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<MarginType>))]
    public enum MarginType
    {
        /// <summary>
        /// Cross margin
        /// </summary>
        [Map("cross")]
        Cross,
        /// <summary>
        /// Isolated margin
        /// </summary>
        [Map("isolated")]
        Isolated
    }
}
