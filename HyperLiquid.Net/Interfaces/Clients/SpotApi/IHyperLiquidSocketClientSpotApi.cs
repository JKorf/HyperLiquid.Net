using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot streams
    /// </summary>
    /// <see cref="IHyperLiquidSocketClientApi"/>
    public interface IHyperLiquidSocketClientSpotApi : IHyperLiquidSocketClientApi
    {
        /// <summary>
        /// Subscribe to spot symbol updates
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example `HYPE/USDC`</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTicker>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Get the shared socket requests client. This interface is shared with other exchanges to allow for a common implementation for different exchanges.
        /// </summary>
        public IHyperLiquidSocketClientSpotApiShared SharedClient { get; }
    }
}
