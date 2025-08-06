using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using System;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures trading endpoints, placing and managing orders.
    /// </summary>
    /// <see cref="IHyperLiquidRestClientTrading"/>
    public interface IHyperLiquidRestClientFuturesApiTrading : IHyperLiquidRestClientTrading
    {
        /// <summary>
        /// Set leverage for a symbol
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#update-leverage" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "ETH"</param>
        /// <param name="leverage">New leverage</param>
        /// <param name="marginType">Margin type</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> SetLeverageAsync(string symbol, int leverage, MarginType marginType, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Add or remove margin from isolated position
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#update-isolated-margin" /></para>
        /// </summary>
        /// <param name="symbol">Symbol name, for example "ETH"</param>
        /// <param name="updateValue">Change value</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> UpdateIsolatedMarginAsync(string symbol, decimal updateValue, DateTime? expireAfter = null, CancellationToken ct = default);
    }
}
