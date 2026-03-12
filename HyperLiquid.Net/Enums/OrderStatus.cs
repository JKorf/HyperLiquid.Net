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
        /// ["<c>filled</c>"] Filled
        /// </summary>
        [Map("filled")]
        Filled,
        /// <summary>
        /// ["<c>open</c>"] Open
        /// </summary>
        [Map("open")]
        Open,
        /// <summary>
        /// ["<c>canceled</c>"] Canceled
        /// </summary>
        [Map("canceled", "reduceOnlyCanceled")]
        Canceled,
        /// <summary>
        /// ["<c>triggered</c>"] Trigger
        /// </summary>
        [Map("triggered")]
        Triggered,
        /// <summary>
        /// ["<c>rejected</c>"] Rejected
        /// </summary>
        [Map("rejected")]
        Rejected,
        /// <summary>
        /// ["<c>marginCanceled</c>"] Margin canceled
        /// </summary>
        [Map("marginCanceled")]
        MarginCanceled,
        /// <summary>
        /// ["<c>insufficientSpotBalanceRejected</c>"] Rejected; insufficient balance
        /// </summary>
        [Map("insufficientSpotBalanceRejected")]
        RejectedInsufficientBalance,
        /// <summary>
        /// ["<c>iocCancelRejected</c>"] Rejected; IOC 
        /// </summary>
        [Map("iocCancelRejected")]
        RejectedIOC,
        /// <summary>
        /// ["<c>badAloPxRejected</c>"] Rejected; price rejected 
        /// </summary>
        [Map("badAloPxRejected")]
        RejectedBadPrice,
        /// <summary>
        /// ["<c>perpMarginRejected</c>"] Rejected; insufficient margin
        /// </summary>
        [Map("perpMarginRejected")]
        RejectedInsufficientMargin,
        /// <summary>
        /// ["<c>minTradeNtlRejected</c>"] Rejected; order value too small
        /// </summary>
        [Map("minTradeNtlRejected")]
        RejectedMinValue,
        /// <summary>
        /// ["<c>siblingFilledCanceled</c>"] Rejected; sibling filled/canceled
        /// </summary>
        [Map("siblingFilledCanceled")]
        RejectedSiblingFilledCanceled,
        /// <summary>
        /// ["<c>reduceOnlyRejected</c>"] Reduce only rejected
        /// </summary>
        [Map("reduceOnlyRejected")]
        ReduceOnlyRejected,

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
