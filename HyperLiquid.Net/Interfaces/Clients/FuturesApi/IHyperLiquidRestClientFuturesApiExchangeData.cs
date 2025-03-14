using System;
using System.Collections.Generic;
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
    public interface IHyperLiquidRestClientFuturesApiExchangeData : IHyperLiquidRestClientExchangeData
    {
        /// <summary>
        /// Get exchange info
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidFuturesSymbol[]>> GetExchangeInfoAsync(CancellationToken ct = default);

        /// <summary>
        /// Get exchange and ticker info
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-perpetuals-asset-contexts-includes-mark-price-current-funding-open-interest-etc" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidFuturesExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get funding rate history
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-historical-funding-rates" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "ETH"</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidFundingRate[]>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime? endTime = null, CancellationToken ct = default);

        /// <summary>
        /// Get futures symbols at max open interest
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<string[]>> GetSymbolsAtMaxOpenInterestAsync(CancellationToken ct = default);
    }
}
