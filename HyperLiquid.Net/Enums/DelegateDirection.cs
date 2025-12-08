using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Direction
    /// </summary>
    [JsonConverter(typeof(EnumConverter<DelegateDirection>))]
    public enum DelegateDirection
    {
        /// <summary>
        /// Delegate
        /// </summary>
        Delegate,
        /// <summary>
        /// Undelegate
        /// </summary>
        Undelegate
    }
}
