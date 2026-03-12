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
        /// ["<c>Limit</c>"] Limit
        /// </summary>
        [Map("Limit")]
        Limit,
        /// <summary>
        /// ["<c>Market</c>"] Market
        /// </summary>
        [Map("Market")]
        Market,
        /// <summary>
        /// ["<c>Stop Market</c>"] Stop Market
        /// </summary>
        [Map("Stop Market")]
        StopMarket,
        /// <summary>
        /// ["<c>Stop Limit</c>"] Stop Limit
        /// </summary>
        [Map("Stop Limit")]
        StopLimit,
        /// <summary>
        /// ["<c>Take Profit Market</c>"] Stop Market
        /// </summary>
        [Map("Take Profit Market")]
        TakeProfitMarket,
        /// <summary>
        /// ["<c>Take Profit</c>"] Stop Limit
        /// </summary>
        [Map("Take Profit", "Take Profit Limit")]
        TakeProfit
    }
}
