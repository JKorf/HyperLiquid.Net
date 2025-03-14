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
        /// Activated
        /// </summary>
        [Map("activated")]
        Activated,
        /// <summary>
        /// Terminated
        /// </summary>
        [Map("terminated")]
        Terminated,
        /// <summary>
        /// Finished
        /// </summary>
        [Map("finished")]
        Finished,
        /// <summary>
        /// Error
        /// </summary>
        [Map("error")]
        Error
    }
}
