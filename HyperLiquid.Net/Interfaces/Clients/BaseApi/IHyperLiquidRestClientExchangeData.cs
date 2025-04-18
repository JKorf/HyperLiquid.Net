using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid exchange data endpoints. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    public interface IHyperLiquidRestClientExchangeData
    {
        /// <summary>
        /// Get mid prices for all assets, includes both Spot and Futures symbols
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-mids-for-all-actively-traded-coins" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<Dictionary<string, decimal>>> GetPricesAsync(CancellationToken ct = default);

        /// <summary>
        /// Get order book
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#l2-book-snapshot" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="numberSignificantFigures">Asset name</param>
        /// <param name="mantissa">Mantissa</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidOrderBook>> GetOrderBookAsync(string symbol, int? numberSignificantFigures = null, int? mantissa = null, CancellationToken ct = default);

        /// <summary>
        /// Get klines/candlestick data
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#candle-snapshot" /></para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="startTime">Data start time</param>
        /// <param name="endTime">Data end time</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidKline[]>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    }
}
