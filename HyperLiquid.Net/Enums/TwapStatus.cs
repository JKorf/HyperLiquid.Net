using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// TWAP order status
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TwapStatus>))]
    public enum TwapStatus
    {
        /// <summary>
        /// ["<c>activated</c>"] Activated
        /// </summary>
        [Map("activated")]
        Activated,
        /// <summary>
        /// ["<c>terminated</c>"] Terminated
        /// </summary>
        [Map("terminated")]
        Terminated,
        /// <summary>
        /// ["<c>finished</c>"] Finished
        /// </summary>
        [Map("finished")]
        Finished,
        /// <summary>
        /// ["<c>error</c>"] Error
        /// </summary>
        [Map("error")]
        Error
    }
}
