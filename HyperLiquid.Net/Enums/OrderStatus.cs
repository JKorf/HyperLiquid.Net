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
        [Map("canceled", "reduceOnlyCanceled")]
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
        MarginCanceled,
        /// <summary>
        /// Rejected; insufficient balance
        /// </summary>
        [Map("insufficientSpotBalanceRejected")]
        RejectedInsufficientBalance,
        /// <summary>
        /// Rejected; IOC 
        /// </summary>
        [Map("iocCancelRejected")]
        RejectedIOC,
        /// <summary>
        /// Rejected; price rejected 
        /// </summary>
        [Map("badAloPxRejected")]
        RejectedBadPrice,
        /// <summary>
        /// Rejected; insufficient margin
        /// </summary>
        [Map("perpMarginRejected")]
        RejectedInsufficientMargin,
        /// <summary>
        /// Rejected; order value too small
        /// </summary>
        [Map("minTradeNtlRejected")]
        RejectedMinValue,
        /// <summary>
        /// Rejected; sibling filled/canceled
        /// </summary>
        [Map("siblingFilledCanceled")]
        RejectedSiblingFilledCanceled,

        /// <summary>
        /// Waiting for main order to fill before placing this order
        /// </summary>
        WaitingFill,
        /// <summary>
        /// Waiting for trigger price to be reached before placing this order
        /// </summary>
        WaitingTrigger
    }
}
