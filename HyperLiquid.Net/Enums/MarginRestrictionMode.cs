using System.Text.Json.Serialization;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Margin mode restriction on a symbol
    /// </summary>
    [JsonConverter(typeof(EnumConverter<MarginRestrictionMode>))]
    public enum MarginRestrictionMode
    {
        /// <summary>
        /// Margin can not be removed
        /// </summary>
        [Map("strictIsolated")]
        StrictIsolated,
        /// <summary>
        /// Only isolated margin allowed
        /// </summary>
        [Map("noCross")]
        NoCross
    }
}
