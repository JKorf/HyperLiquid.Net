using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures WebSocket account endpoints and streams. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    /// <see cref="IHyperLiquidSocketClientApiAccount"/>
    public interface IHyperLiquidSocketClientFuturesApiAccount : IHyperLiquidSocketClientApiAccount
    {
        /// <summary>
        /// Get account info, balances and open positions
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-users-perpetuals-account-summary" /><br />
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">["<c>dex</c>"] The DEX to request data for, leave null for default perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFuturesAccount>> GetAccountInfoAsync(string? address = null, string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get user funding history
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-a-users-funding-history-or-non-funding-ledger-updates" /><br />
        /// </para>
        /// </summary>
        /// <param name="startTime">["<c>startTime</c>"] Filter by start time</param>
        /// <param name="endTime">["<c>endTime</c>"] Filter by end time</param>
        /// <param name="address">["<c>user</c>"] Address to request funding history for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidUserLedger<HyperLiquidUserFunding>[]>> GetFundingHistoryAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user active symbols
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-users-active-asset-data" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">["<c>coin</c>"] The symbol, for example `ETH`</param>
        /// <param name="address">["<c>user</c>"] Address to request funding history for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFuturesUserSymbolUpdate>> GetUserSymbolAsync(string symbol, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get whether HIP-3 DEX abstraction enabled
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-hip-3-dex-abstraction-state" /><br />
        /// </para>
        /// </summary>
        /// <param name="user">["<c>user</c>"] User address. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CallResult<bool>> GetHip3DexAbstractionAsync(string? user = null, CancellationToken ct = default);

        /// <summary>
        /// Toggle HIP-3 DEX abstraction. If set, actions on HIP-3 perps will automatically transfer collateral from validator-operated USDC perps balance for HIP-3 DEXs where USDC is the collateral token, and spot otherwise. 
        /// When HIP-3 DEX abstraction is active, collateral is returned to the same source (validator-operated USDC perps or spot balance) when released from positions or open orders.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#enable-hip-3-dex-abstraction" /><br />
        /// </para>
        /// </summary>
        /// <param name="enabled">["<c>enabled</c>"] Whether HIP-3 DEX abstraction should be enabled</param>
        /// <param name="address">["<c>user</c>"] User address. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CallResult> ToggleHip3DexAbstractionAsync(bool enabled, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user symbol updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: activeAssetData)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="symbol">Symbol name, for example `ETH`</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserSymbolUpdatesAsync(string? address, string symbol, Action<DataEvent<HyperLiquidFuturesUserSymbolUpdate>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user funding updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: userFundings)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserFundingUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserFunding[]>> onMessage, CancellationToken ct = default);
    }
}
