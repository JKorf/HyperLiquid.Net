using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Place order request
    /// </summary>
    [SerializationModel]
    public record HyperLiquidOrderRequest
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Order side
        /// </summary>
        public OrderSide Side { get; set; }
        /// <summary>
        /// Order type
        /// </summary>
        public OrderType OrderType { get; set; }
        /// <summary>
        /// Time in force
        /// </summary>
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Order price
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Reduce only
        /// </summary>
        public bool? ReduceOnly { get; set; }
        /// <summary>
        /// Client order id
        /// </summary>
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Trigger price
        /// </summary>
        public decimal? TriggerPrice { get; set; }
        /// <summary>
        /// Take profit / stop loss type
        /// </summary>
        public TpSlType? TpSlType { get; set; }

        /// <summary>
        /// Max Slippage for market orders
        /// </summary>
        public decimal? MaxSlippage { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidOrderRequest(
            string symbol,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal price,
            TimeInForce? timeInForce = null,
            bool? reduceOnly = null,
            decimal? triggerPrice = null,
            TpSlType? tpSlType = null,
            string? clientOrderId = null,
            decimal? maxSlippage = null
            )
        {
            Symbol = symbol;
            Side = side;
            OrderType = orderType;
            Quantity = quantity;
            Price = price;
            ReduceOnly = reduceOnly;
            TriggerPrice = triggerPrice;
            TpSlType = tpSlType;
            ClientOrderId = clientOrderId;
            TimeInForce = timeInForce;
            MaxSlippage = maxSlippage;
        }
    }
}
