using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Order type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<OrderType>))]
    public enum OrderType
    {
        /// <summary>
        /// Limit
        /// </summary>
        [Map("Limit")]
        Limit,
        /// <summary>
        /// Market
        /// </summary>
        [Map("Market")]
        Market,
        /// <summary>
        /// Stop Market
        /// </summary>
        [Map("Stop Market")]
        StopMarket,
        /// <summary>
        /// Stop Limit
        /// </summary>
        [Map("Stop Limit")]
        StopLimit,
        /// <summary>
        /// Stop Market
        /// </summary>
        [Map("Take Profit Market")]
        TakeProfitMarket,
        /// <summary>
        /// Stop Limit
        /// </summary>
        [Map("Take Profit", "Take Profit Limit")]
        TakeProfit
    }
}
