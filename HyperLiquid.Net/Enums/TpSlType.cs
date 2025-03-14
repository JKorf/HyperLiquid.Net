using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// TakeProfit/StopLoss type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TpSlType>))]
    public enum TpSlType
    {
        /// <summary>
        /// Take profit
        /// </summary>
        [Map("tp")]
        TakeProfit,
        /// <summary>
        /// Stop loss
        /// </summary>
        [Map("sl")]
        StopLoss
    }
}
