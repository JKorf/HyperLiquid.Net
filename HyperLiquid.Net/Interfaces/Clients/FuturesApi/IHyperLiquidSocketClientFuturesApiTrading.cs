using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures WebSocket trading endpoints and stream for placing and managing orders.
    /// </summary>
    /// <see cref="IHyperLiquidSocketClientApiAccount"/>
    public interface IHyperLiquidSocketClientFuturesApiTrading : IHyperLiquidSocketClientApiTrading
    {
        /// <summary>
        /// Set leverage for a symbol
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#update-leverage" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: updateLeverage)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "ETH"</param>
        /// <param name="leverage">["<c>leverage</c>"] New leverage</param>
        /// <param name="marginType">Margin type</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> SetLeverageAsync(
            string symbol,
            int leverage,
            MarginType marginType,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Add or remove margin from isolated position
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#update-isolated-margin" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: updateIsolatedMargin)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "ETH"</param>
        /// <param name="updateValue">["<c>ntli</c>"] Change value</param>
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> UpdateIsolatedMarginAsync(
            string symbol,
            decimal updateValue,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to futures account margin and position snapshot updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: clearinghouseState)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">Optional DEX selection</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBalanceAndPositionUpdatesAsync(string? address, string? dex, Action<DataEvent<HyperLiquidPositionUpdate>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to futures account margin and position snapshot updates for all dexes
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: clearinghouseState)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBalanceAndPositionUpdatesAllDexesAsync(string? address, Action<DataEvent<HyperLiquidAllDexPositionUpdate>> onMessage, CancellationToken ct = default);
    }
}
