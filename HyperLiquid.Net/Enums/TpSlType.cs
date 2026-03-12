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
        /// ["<c>tp</c>"] Take profit
        /// </summary>
        [Map("tp")]
        TakeProfit,
        /// <summary>
        /// ["<c>sl</c>"] Stop loss
        /// </summary>
        [Map("sl")]
        StopLoss
    }
}
