using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Type of symbol
    /// </summary>
    [JsonConverter(typeof(EnumConverter<SymbolType>))]
    public enum SymbolType
    {
        /// <summary>
        /// Spot symbol
        /// </summary>
        Spot,
        /// <summary>
        /// Futures symbol
        /// </summary>
        Futures
    }
}
