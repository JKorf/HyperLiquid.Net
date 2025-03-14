using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Order status
    /// </summary>
    [JsonConverter(typeof(EnumConverter<OrderStatus>))]
    public enum OrderStatus
    {
        /// <summary>
        /// Filled
        /// </summary>
        [Map("filled")]
        Filled,
        /// <summary>
        /// Open
        /// </summary>
        [Map("open")]
        Open,
        /// <summary>
        /// Canceled
        /// </summary>
        [Map("canceled")]
        Canceled,
        /// <summary>
        /// Trigger
        /// </summary>
        [Map("triggered")]
        Triggered,
        /// <summary>
        /// Rejected
        /// </summary>
        [Map("rejected")]
        Rejected,
        /// <summary>
        /// Margin canceled
        /// </summary>
        [Map("marginCanceled")]
        MarginCanceled
    }
}
