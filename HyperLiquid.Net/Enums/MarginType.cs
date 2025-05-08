using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;
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
        Cross,
        /// <summary>
        /// Isolated margin
        /// </summary>
        Isolated
    }
}
