using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using System.Collections.Generic;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid WebSocket exchange data endpoints and streams. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    public interface IHyperLiquidSocketClientApiExchangeData
    {
        /// <summary>
        /// Get mid prices for all assets, includes both Spot and Futures symbols
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#retrieve-mids-for-all-actively-traded-coins" /><br />
        /// </para>
        /// </summary>
        /// <param name="dex">["<c>dex</c>"] Filter by DEX</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<Dictionary<string, decimal>>> GetPricesAsync(string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get order book
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#l2-book-snapshot" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="numberSignificantFigures">["<c>nSigFigs</c>"] Asset name</param>
        /// <param name="mantissa">["<c>mantissa</c>"] Mantissa</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidOrderBook>> GetOrderBookAsync(string symbol, int? numberSignificantFigures = null, int? mantissa = null, CancellationToken ct = default);

        /// <summary>
        /// Get klines/candlestick data
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#candle-snapshot" /><br />
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="interval">["<c>interval</c>"] Kline interval</param>
        /// <param name="startTime">["<c>startTime</c>"] Data start time</param>
        /// <param name="endTime">["<c>endTime</c>"] Data end time</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidKline[]>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to mid price updates, will send updates for both Spot and Futures symbols
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: allMids)
        /// </para>
        /// </summary>
        /// <param name="onMessage">The event handler for the received data</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToPriceUpdatesAsync(Action<DataEvent<Dictionary<string, decimal>>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to kline/candlestick data
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: candle)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="interval">Kline interval</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<HyperLiquidKline>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to order book updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: l2Book)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="nSigFigs">Number of significant figures to use for price, if not provided will use the default from the options</param>
        /// <param name="mantissa">Mantissa to use for price, if not provided will use the default from the options</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidOrderBook>> onMessage, int? nSigFigs = null, int? mantissa = null, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to trade updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: trades)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTrade[]>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to book ticker updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: bbo)
        /// </para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "HYPE/USDC" for spot, or "ETH" for futures</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidBookTicker>> onMessage, CancellationToken ct = default);

    }
}
