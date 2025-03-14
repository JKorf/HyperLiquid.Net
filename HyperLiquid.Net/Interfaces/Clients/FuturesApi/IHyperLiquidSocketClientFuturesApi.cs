using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using System.Collections.Generic;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures streams
    /// </summary>
    public interface IHyperLiquidSocketClientFuturesApi : IHyperLiquidSocketClientApi
    {
        /// <summary>
        /// Subscribe to symbol updates
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example `ETH`</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidFuturesTicker>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user symbol updates
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="symbol">Symbol name, for example `ETH`</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserSymbolUpdatesAsync(string? address, string symbol, Action<DataEvent<HyperLiquidFuturesUserSymbolUpdate>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user funding updates
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserFundingUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserFunding[]>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Get the shared socket requests client. This interface is shared with other exhanges to allow for a common implementation for different exchanges.
        /// </summary>
        public IHyperLiquidSocketClientFuturesApiShared SharedClient { get; }
    }
}
