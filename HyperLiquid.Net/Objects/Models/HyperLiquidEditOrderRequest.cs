using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Edit order request
    /// </summary>
    [SerializationModel]
    public record HyperLiquidEditOrderRequest
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Order id
        /// </summary>
        public long? OrderId { get; set; }
        /// <summary>
        /// Client order id
        /// </summary>
        public string? ClientOrderId { get; set; }
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
        /// Price
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
        /// New client order id
        /// </summary>
        public string? NewClientOrderId { get; set; }
        /// <summary>
        /// Trigger price
        /// </summary>
        public decimal? TriggerPrice { get; set; }
        /// <summary>
        /// Take profit / Stop loss type
        /// </summary>
        public TpSlType? TpSlType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="orderId">Order id of the order to edit, either this or clientOrderId should be provided</param>
        /// <param name="clientOrderId">Client order id of the order to edit, either this or orderId should be provided</param>
        /// <param name="side">Order side</param>
        /// <param name="orderType">Order type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="price">Price</param>
        /// <param name="timeInForce">Time in force</param>
        /// <param name="reduceOnly">Reduce only</param>
        /// <param name="triggerPrice">Trigger price</param>
        /// <param name="tpSlType">Take profit / Stop loss type</param>
        /// <param name="newClientOrderId">New client order id</param>
        public HyperLiquidEditOrderRequest(
            string symbol,
            long? orderId,
            string? clientOrderId,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal price,
            TimeInForce? timeInForce = null,
            bool? reduceOnly = null,
            decimal? triggerPrice = null,
            TpSlType? tpSlType = null,
            string? newClientOrderId = null
            )
        {
            Symbol = symbol;
            OrderId = orderId;
            ClientOrderId = clientOrderId;
            Side = side;
            OrderType = orderType;
            Quantity = quantity;
            Price = price;
            ReduceOnly = reduceOnly;
            TriggerPrice = triggerPrice;
            TpSlType = tpSlType;
            NewClientOrderId = newClientOrderId;
            TimeInForce = timeInForce;
        }
    }
}
