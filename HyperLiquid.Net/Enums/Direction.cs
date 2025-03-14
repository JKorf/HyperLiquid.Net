using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Direction
    /// </summary>
    [JsonConverter(typeof(EnumConverter<Direction>))]
    public enum Direction
    {
        /// <summary>
        /// Open long
        /// </summary>
        [Map("Open Long")]
        OpenLong,
        /// <summary>
        /// Close long
        /// </summary>
        [Map("Close Long")]
        CloseLong,
        /// <summary>
        /// Open short
        /// </summary>
        [Map("Open Short")]
        OpenShort,
        /// <summary>
        /// Close short
        /// </summary>
        [Map("Close Short")]
        CloseShort,
        /// <summary>
        /// Buy spot order
        /// </summary>
        [Map("Buy")]
        Buy,
        /// <summary>
        /// Sell spot order
        /// </summary>
        [Map("Sell")]
        Sell
    }
}
