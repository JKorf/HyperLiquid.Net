using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures exchange data endpoints. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    /// <see cref="IHyperLiquidRestClientExchangeData"/>
    public interface IHyperLiquidRestClientFuturesApiExchangeData : IHyperLiquidRestClientExchangeData
    {
        /// <summary>
        /// Get all Perp dexes
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-all-perpetual-dexs" /><br />
        /// Endpoint:<br />
        /// POST /info (type: perpDexs)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidPerpDex[]>> GetPerpDexesAsync(CancellationToken ct = default);

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
        Task<WebCallResult<HyperLiquidFuturesSymbol[]>> GetExchangeInfoAsync(string? dex = null, CancellationToken ct = default);

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
        Task<WebCallResult<HyperLiquidFuturesDexInfo[]>> GetExchangeInfoAllDexesAsync(CancellationToken ct = default);

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
        Task<WebCallResult<HyperLiquidFuturesExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default);

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
        Task<WebCallResult<HyperLiquidFundingRate[]>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime? endTime = null, CancellationToken ct = default);

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
        Task<WebCallResult<string[]>> GetSymbolsAtMaxOpenInterestAsync(string? dex = null, CancellationToken ct = default);

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
        Task<WebCallResult<HyperLiquidPerpDexLimit>> GetPerpDexMarketLimitsAsync(string? dex = null, CancellationToken ct = default);

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
        Task<WebCallResult<HyperLiquidPerpDexStatus>> GetPerpDexMarketStatusAsync(string? dex = null, CancellationToken ct = default);
    }
}
