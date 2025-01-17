using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using System.Collections.Generic;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid  streams
    /// </summary>
    public interface IHyperLiquidSocketClientApi : ISocketApiClient, IDisposable
    {
        /// <summary>
        /// Subscribe to mid price updates, will send updates for both Spot and Futures symbols
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToPriceUpdatesAsync(Action<DataEvent<Dictionary<string, decimal>>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to kline/candlestick data
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<HyperLiquidKline>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to order book updates
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidOrderBook>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to trade updates
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<IEnumerable<HyperLiquidTrade>>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to order updates, will provided updates for both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidOrderStatus>>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user trade updates, will provided updates for both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidUserTrade>>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user ledger updates (excluding funding updates)will provided updates for both Spot and Futures changes
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserLedgerUpdatesAsync(string? address, Action<DataEvent<HyperLiquidAccountLedger>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user updates, including Spot and Futures balances
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserUpdate>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to Time Weighted Average Price trade updates, will provided updates for both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTwapTradeUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidTwapStatus>>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to Time Weighted Average Price order history updates, will provided updates for both Spot and Futures orders
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTwapOrderUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidTwapOrderStatus>>> onMessage, CancellationToken ct = default);

    }
}
