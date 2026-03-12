using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Position type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<PositionType>))]
    public enum PositionType
    {
        /// <summary>
        /// ["<c>oneWay</c>"] One way
        /// </summary>
        [Map("oneWay")]
        OneWay
    }
}
