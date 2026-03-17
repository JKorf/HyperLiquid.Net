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
    /// HyperLiquid futures WebSocket exchange data endpoints and streams. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    /// <see cref="IHyperLiquidSocketClientApiExchangeData"/>
    public interface IHyperLiquidSocketClientFuturesApiExchangeData : IHyperLiquidSocketClientApiExchangeData
    {
        /// <summary>
        /// Get all Perp dexes
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-all-perpetual-dexs" /><br />
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidPerpDex[]>> GetPerpDexesAsync(CancellationToken ct = default);

        /// <summary>
        /// Get exchange info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals" /><br />
        /// Endpoint:<br />
        /// POST /info (type: meta)
        /// </para>
        /// </summary>
        /// <param name="dex">["<c>dex</c>"] DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFuturesSymbol[]>> GetExchangeInfoAsync(string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get exchange info for all perp dexes
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals" /><br />
        /// Endpoint:<br />
        /// POST /info (type: allPerpMetas)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFuturesDexInfo[]>> GetExchangeInfoAllDexesAsync(CancellationToken ct = default);

        /// <summary>
        /// Get exchange and ticker info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-perpetuals-asset-contexts-includes-mark-price-current-funding-open-interest-etc" /><br />
        /// Endpoint:<br />
        /// POST /info (type: metaAndAssetCtxs)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFuturesExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get funding rate history
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-historical-funding-rates" /><br />
        /// Endpoint:<br />
        /// POST /info (type: fundingHistory)
        /// </para>
        /// </summary>
        /// <param name="symbol">["<c>coin</c>"] Symbol, for example "ETH"</param>
        /// <param name="startTime">["<c>startTime</c>"] Filter by start time</param>
        /// <param name="endTime">["<c>endTime</c>"] Filter by end time</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFundingRate[]>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime? endTime = null, CancellationToken ct = default);

        /// <summary>
        /// Get futures symbols at max open interest
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#query-perps-at-open-interest-caps" /><br />
        /// Endpoint:<br />
        /// POST /info (type: perpsAtOpenInterestCap)
        /// </para>
        /// </summary>
        /// <param name="dex">["<c>dex</c>"] DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<string[]>> GetSymbolsAtMaxOpenInterestAsync(string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get Perp DEX market limits
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-builder-deployed-perp-market-limits" /><br />
        /// Endpoint:<br />
        /// POST /info (type: perpDexLimits)
        /// </para>
        /// </summary>
        /// <param name="dex">["<c>dex</c>"] DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidPerpDexLimit>> GetPerpDexMarketLimitsAsync(string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get Perp DEX market status
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#get-perp-market-status" /><br />
        /// Endpoint:<br />
        /// POST /info (type: perpDexStatus)
        /// </para>
        /// </summary>
        /// <param name="dex">["<c>dex</c>"] DEX name, for example `xyz`, null for default Perp DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidPerpDexStatus>> GetPerpDexMarketStatusAsync(string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to symbol updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: activeAssetCtx)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example `ETH`</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidFuturesTicker>> onMessage, CancellationToken ct = default);

    }
}
