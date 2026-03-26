using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using CryptoExchange.Net.Objects.Sockets;
using System;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot WebSocket account endpoints and streams. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    /// <see cref="IHyperLiquidSocketClientApiAccount"/>
    public interface IHyperLiquidSocketClientSpotApiAccount : IHyperLiquidSocketClientApiAccount
    {
        /// <summary>
        /// Get user asset balances
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-a-users-token-balances" /><br />
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidBalance[]>> GetBalancesAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Send spot assets to another address. This transfer does not touch the EVM bridge.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#l1-spot-transfer" /><br />
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">["<c>destination</c>"] Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="asset">Asset name, for example "HYPE"</param>
        /// <param name="quantity">["<c>amount</c>"] Quantity to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> TransferSpotAsync(
            string destinationAddress,
            string asset,
            decimal quantity,
            CancellationToken ct = default);


        /// <summary>
        /// Subscribe to spot balance updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: spotState)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(string? address, Action<DataEvent<HyperLiquidBalanceUpdate>> onMessage, CancellationToken ct = default);
    }
}
