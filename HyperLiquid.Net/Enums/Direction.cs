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
        /// ["<c>Open Long</c>"] Open long
        /// </summary>
        [Map("Open Long")]
        OpenLong,
        /// <summary>
        /// ["<c>Close Long</c>"] Close long
        /// </summary>
        [Map("Close Long")]
        CloseLong,
        /// <summary>
        /// ["<c>Open Short</c>"] Open short
        /// </summary>
        [Map("Open Short")]
        OpenShort,
        /// <summary>
        /// ["<c>Close Short</c>"] Close short
        /// </summary>
        [Map("Close Short")]
        CloseShort,
        /// <summary>
        /// ["<c>Buy</c>"] Buy spot order
        /// </summary>
        [Map("Buy")]
        Buy,
        /// <summary>
        /// ["<c>Sell</c>"] Sell spot order
        /// </summary>
        [Map("Sell")]
        Sell,
        /// <summary>
        /// ["<c>Long > Short</c>"] Long to short order
        /// </summary>
        [Map("Long > Short")]
        LongToShort,
        /// <summary>
        /// ["<c>Short > Long</c>"] Short to long order
        /// </summary>
        [Map("Short > Long")]
        ShortToLong,
        /// <summary>
        /// ["<c>Spot Dust Conversion</c>"] Spot dust conversion
        /// </summary>
        [Map("Spot Dust Conversion")]
        SpotDustConversion,
        /// <summary>
        /// ["<c>Auto-Deleveraging</c>"] Auto deleveraging
        /// </summary>
        [Map("Auto-Deleveraging")]
        AutoDeleveraging
    }
}
