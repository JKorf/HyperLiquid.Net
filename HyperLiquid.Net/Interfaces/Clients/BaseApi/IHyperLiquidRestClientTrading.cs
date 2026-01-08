using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Objects.Models;
using System;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid trading endpoints, placing and managing orders.
    /// </summary>
    public interface IHyperLiquidRestClientTrading
    {
        /// <summary>
        /// Get open orders, will return both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-open-orders" /></para>
        /// </summary>
        /// <param name="address">Address to request open orders for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOpenOrder[]>> GetOpenOrdersAsync(string? address = null, string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get open orders including with additional info, will return both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-open-orders-with-additional-frontend-info" /></para>
        /// </summary>
        /// <param name="address">Address to request open orders for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOrder[]>> GetOpenOrdersExtendedAsync(string? address = null, string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get user trades, will return both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-fills" /></para>
        /// </summary>
        /// <param name="address">Address to request user trades for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidUserTrade[]>> GetUserTradesAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user trades by time filter, will return both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-fills-by-time" /></para>
        /// </summary>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="aggregateByTime">Aggregate by time</param>
        /// <param name="address">Address to request user trades for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidUserTrade[]>> GetUserTradesByTimeAsync(
            DateTime startTime,
            DateTime? endTime = null,
            bool? aggregateByTime = null,
            string? address = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get order info by id
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-order-status-by-oid-or-cloid" /></para>
        /// </summary>
        /// <param name="orderId">Get order by order id. Either this or clientOrderId should be provided</param>
        /// <param name="clientOrderId">Get order by client order id. Either this or orderId should be provided</param>
        /// <param name="address">Address to request order for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOrderStatus>> GetOrderAsync(long? orderId = null, string? clientOrderId = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user order history, will return both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-historical-orders" /></para>
        /// </summary>
        /// <param name="address">Address to request order for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOrderStatus[]>> GetOrderHistoryAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="orderId">Order id</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> CancelOrderAsync(string symbol, long orderId, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel multiple orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s" /></para>
        /// </summary>
        /// <param name="requests">Cancel requests</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<CallResult[]>> CancelOrdersAsync(IEnumerable<HyperLiquidCancelRequest> requests, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel order by client order id
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s-by-cloid" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> CancelOrderByClientOrderIdAsync(string symbol, string clientOrderId, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel multiple orders by client order id
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s-by-cloid" /></para>
        /// </summary>
        /// <param name="requests">Cancel requests</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<CallResult[]>> CancelOrdersByClientOrderIdAsync(IEnumerable<HyperLiquidCancelByClientOrderIdRequest> requests, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Place a new order
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#place-an-order" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="side">Order side</param>
        /// <param name="orderType">Order type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="price">Limit price. For market orders pass the current price of the symbol for max slippage calculation.</param>
        /// <param name="timeInForce">Time in force</param>
        /// <param name="reduceOnly">Reduce only</param>
        /// <param name="triggerPrice">Trigger order trigger price</param>
        /// <param name="tpSlType">Trigger order type</param>
        /// <param name="tpSlGrouping">Trigger order grouping</param>
        /// <param name="clientOrderId">Client order id, an optional 128 bit hex string, e.g. 0x1234567890abcdef1234567890abcdef</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOrderResult>> PlaceOrderAsync(
            string symbol,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal price,
            TimeInForce? timeInForce = null,
            bool? reduceOnly = null,
            string? clientOrderId = null,
            decimal? triggerPrice = null,
            TpSlType? tpSlType = null,
            TpSlGrouping? tpSlGrouping = null,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default
            );

        /// <summary>
        /// Place multiple new orders in a single call
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#place-an-order" /></para>
        /// </summary>
        /// <param name="orders">Orders to place</param>
        /// <param name="tpSlGrouping">Take profit / Stop loss grouping</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<CallResult<HyperLiquidOrderResult>[]>> PlaceMultipleOrdersAsync(
            IEnumerable<HyperLiquidOrderRequest> orders,
            TpSlGrouping? tpSlGrouping = null,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel all orders after the provided timeout has passed. Can be called at an interval to act as deadman switch. Pass null to cancel an existing timeout. This will cancel both Spot and Futures orders. This functionality is only available after achieving a certain trading volume.
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#schedule-cancel-dead-mans-switch" /></para>
        /// </summary>
        /// <param name="timeout">Timeout after which to cancel all order, or null to cancel the countdown</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOrderStatus[]>> CancelAfterAsync(TimeSpan? timeout, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Edit an existing order
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#modify-an-order" /></para>
        /// </summary>
        /// <param name="orderId">Edit order by order id, either this or clientOrderId should be provided</param>
        /// <param name="clientOrderId">Edit order by client order id, either this or orderId should be provided</param>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="side">Order side</param>
        /// <param name="orderType">Order type</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="price">Limit price</param>
        /// <param name="timeInForce">Time in force</param>
        /// <param name="reduceOnly">Reduce only</param>
        /// <param name="newClientOrderId">The new client order id, an optional 128 bit hex string, e.g. 0x1234567890abcdef1234567890abcdef</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="triggerPrice">Trigger order trigger price</param>
        /// <param name="tpSlType">Trigger order type</param>
        /// <param name="tpSlGrouping">Trigger order grouping</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> EditOrderAsync(
            string symbol,
            long? orderId,
            string? clientOrderId,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal price,
            TimeInForce? timeInForce = null,
            bool? reduceOnly = null,
            string? newClientOrderId = null,
            decimal? triggerPrice = null,
            TpSlType? tpSlType = null,
            TpSlGrouping? tpSlGrouping = null,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Edit multiple orders in a single call
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#modify-multiple-orders" /></para>
        /// </summary>
        /// <param name="requests">Edit requests</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<CallResult<HyperLiquidOrderResult>[]>> EditOrdersAsync(
            IEnumerable<HyperLiquidEditOrderRequest> requests,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Place a Time Weighted Average Price order
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#place-a-twap-order" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="quantity">Order quantity</param>
        /// <param name="reduceOnly">Reduce only</param>
        /// <param name="minutes">Time of the TWAP in minutes</param>
        /// <param name="randomize">Randomize</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidTwapOrderResult>> PlaceTwapOrderAsync(
            string symbol, 
            OrderSide orderSide, 
            decimal quantity, 
            bool reduceOnly, 
            int minutes, 
            bool randomize, 
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel a Time Weighted Average Price order
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-a-twap-order" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="twapId">TWAP order id</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> CancelTwapOrderAsync(string symbol, long twapId, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);
    }
}
