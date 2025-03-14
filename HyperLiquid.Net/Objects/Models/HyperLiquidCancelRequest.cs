using CryptoExchange.Net.Converters.SystemTextJson;
namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Cancel request
    /// </summary>
    [SerializationModel]
    public record HyperLiquidCancelRequest
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Order id
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidCancelRequest(string symbol, long orderId)
        {
            Symbol = symbol;
            OrderId = orderId;
        }
    }

    /// <summary>
    /// Cancel request
    /// </summary>
    [SerializationModel]
    public record HyperLiquidCancelByClientOrderIdRequest
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Client order id
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidCancelByClientOrderIdRequest(string symbol, string orderId)
        {
            Symbol = symbol;
            OrderId = orderId;
        }
    }
}
