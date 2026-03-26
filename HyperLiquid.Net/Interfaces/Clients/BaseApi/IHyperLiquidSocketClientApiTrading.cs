using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using System.Collections.Generic;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid WebSocket trading endpoints and streams, for placing and managing orders.
    /// </summary>
    public interface IHyperLiquidSocketClientApiTrading
    {
        /// <summary>
        /// Get open orders, will return both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-open-orders" /><br />
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request open orders for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">["<c>dex</c>"] DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOpenOrder[]>> GetOpenOrdersAsync(string? address = null, string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get open orders including with additional info, will return both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-open-orders-with-additional-frontend-info" /><br />
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request open orders for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">["<c>dex</c>"] DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOrder[]>> GetOpenOrdersExtendedAsync(string? address = null, string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get user trades, will return both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-fills" /><br />
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request user trades for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidUserTrade[]>> GetUserTradesAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user trades by time filter, will return both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-fills-by-time" /><br />
        /// </para>
        /// </summary>
        /// <param name="startTime">["<c>startTime</c>"] Filter by start time</param>
        /// <param name="endTime">["<c>endTime</c>"] Filter by end time</param>
        /// <param name="aggregateByTime">["<c>aggregateByTime</c>"] Aggregate by time</param>
        /// <param name="address">["<c>user</c>"] Address to request user trades for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidUserTrade[]>> GetUserTradesByTimeAsync(
            DateTime startTime,
            DateTime? endTime = null,
            bool? aggregateByTime = null,
            string? address = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get order info by id
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-order-status-by-oid-or-cloid" /><br />
        /// </para>
        /// </summary>
        /// <param name="orderId">["<c>oid</c>"] Get order by order id. Either this or clientOrderId should be provided</param>
        /// <param name="clientOrderId">["<c>oid</c>"] Get order by client order id. Either this or orderId should be provided</param>
        /// <param name="address">["<c>user</c>"] Address to request order for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOrderStatus>> GetOrderAsync(long? orderId = null, string? clientOrderId = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user order history, will return both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-a-users-historical-orders" /><br />
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request order for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOrderStatus[]>> GetOrderHistoryAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel an order
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="orderId">Order id</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> CancelOrderAsync(string symbol, long orderId, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel multiple orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s" /><br />
        /// </para>
        /// </summary>
        /// <param name="requests">Cancel requests</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<CallResult[]>> CancelOrdersAsync(IEnumerable<HyperLiquidCancelRequest> requests, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel order by client order id
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s-by-cloid" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> CancelOrderByClientOrderIdAsync(string symbol, string clientOrderId, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Cancel multiple orders by client order id
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-order-s-by-cloid" /><br />
        /// </para>
        /// </summary>
        /// <param name="requests">Cancel requests</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<CallResult[]>> CancelOrdersByClientOrderIdAsync(IEnumerable<HyperLiquidCancelByClientOrderIdRequest> requests, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Place a new order
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#place-an-order" /><br />
        /// </para>
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
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOrderResult>> PlaceOrderAsync(
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
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#place-an-order" /><br />
        /// </para>
        /// </summary>
        /// <param name="orders">Orders to place</param>
        /// <param name="tpSlGrouping">["<c>action.grouping</c>"] Take profit / Stop loss grouping</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<CallResult<HyperLiquidOrderResult>[]>> PlaceMultipleOrdersAsync(
            IEnumerable<HyperLiquidOrderRequest> orders,
            TpSlGrouping? tpSlGrouping = null,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel all orders after the provided timeout has passed. Can be called at an interval to act as deadman switch. Pass null to cancel an existing timeout. This will cancel both Spot and Futures orders. This functionality is only available after achieving a certain trading volume.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#schedule-cancel-dead-mans-switch" /><br />
        /// </para>
        /// </summary>
        /// <param name="timeout">["<c>action.time</c>"] Timeout after which to cancel all order, or null to cancel the countdown</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOrderStatus[]>> CancelAfterAsync(TimeSpan? timeout, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Edit an existing order
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#modify-an-order" /><br />
        /// </para>
        /// </summary>
        /// <param name="orderId">["<c>action.oid</c>"] Edit order by order id, either this or clientOrderId should be provided</param>
        /// <param name="clientOrderId">["<c>action.oid</c>"] Edit order by client order id, either this or orderId should be provided</param>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="side">["<c>action.order.b</c>"] Order side</param>
        /// <param name="orderType">Order type</param>
        /// <param name="quantity">["<c>action.order.s</c>"] Quantity</param>
        /// <param name="price">["<c>action.order.p</c>"] Limit price</param>
        /// <param name="timeInForce">["<c>action.order.t.limit.tif</c>"] Time in force</param>
        /// <param name="reduceOnly">["<c>action.order.r</c>"] Reduce only</param>
        /// <param name="newClientOrderId">["<c>action.order.c</c>"] The new client order id, an optional 128 bit hex string, e.g. 0x1234567890abcdef1234567890abcdef</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="triggerPrice">["<c>action.order.t.trigger.triggerPx</c>"] Trigger order trigger price</param>
        /// <param name="tpSlType">["<c>action.order.t.trigger.tpsl</c>"] Trigger order type</param>
        /// <param name="tpSlGrouping">Trigger order grouping</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> EditOrderAsync(
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
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#modify-multiple-orders" /><br />
        /// </para>
        /// </summary>
        /// <param name="requests">Edit requests</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<CallResult<HyperLiquidOrderResult>[]>> EditOrdersAsync(
            IEnumerable<HyperLiquidEditOrderRequest> requests,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Place a Time Weighted Average Price order
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#place-a-twap-order" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="quantity">["<c>action.twap.s</c>"] Order quantity</param>
        /// <param name="reduceOnly">["<c>action.twap.r</c>"] Reduce only</param>
        /// <param name="minutes">["<c>action.twap.m</c>"] Time of the TWAP in minutes</param>
        /// <param name="randomize">["<c>action.twap.t</c>"] Randomize</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidTwapOrderResult>> PlaceTwapOrderAsync(
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
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#cancel-a-twap-order" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="twapId">["<c>action.t</c>"] TWAP order id</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> CancelTwapOrderAsync(string symbol, long twapId, string? vaultAddress = null, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to order updates, will provided updates for both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: orderUpdates)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? address, Action<DataEvent<HyperLiquidOrderStatus[]>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to open order snapshot updates, will provided updates for both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: openOrders)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">Optional DEX selection</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOpenOrderUpdatesAsync(string? address, string? dex, Action<DataEvent<HyperLiquidOpenOrderUpdate>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user trade updates, will provided updates for both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: userFills)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserTrade[]>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to Time Weighted Average Price trade updates, will provided updates for both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: userTwapSliceFills)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTwapTradeUpdatesAsync(string? address, Action<DataEvent<HyperLiquidTwapStatus[]>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to Time Weighted Average Price order history updates, will provided updates for both Spot and Futures orders
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: userTwapHistory)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTwapOrderUpdatesAsync(string? address, Action<DataEvent<HyperLiquidTwapOrderStatus[]>> onMessage, CancellationToken ct = default);

    }
}
