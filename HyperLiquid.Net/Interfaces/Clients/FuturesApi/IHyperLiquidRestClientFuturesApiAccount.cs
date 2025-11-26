using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Objects.Models;
using System;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures account endpoints. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    /// <see cref="IHyperLiquidRestClientAccount"/>
    public interface IHyperLiquidRestClientFuturesApiAccount : IHyperLiquidRestClientAccount
    {
        /// <summary>
        /// Get account info, balances and open positions
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-users-perpetuals-account-summary" /></para>
        /// </summary>
        /// <param name="address">Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">Perpetual dex name (e.g. xyz). Defaults to the empty string which represents the first perpetual dex.</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidFuturesAccount>> GetAccountInfoAsync(string? address = null, string? dex = null, CancellationToken ct = default);

        /// <summary>
        /// Get user funding history
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-a-users-funding-history-or-non-funding-ledger-updates" /></para>
        /// </summary>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="address">Address to request funding history for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidUserLedger<HyperLiquidUserFunding>[]>> GetFundingHistoryAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user active symbols
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-users-active-asset-data" /></para>
        /// </summary>
        /// <param name="symbol">The symbol, for example `ETH`</param>
        /// <param name="address">Address to request funding history for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidFuturesUserSymbolUpdate>> GetUserSymbolAsync(string symbol, string? address = null, CancellationToken ct = default);
    }
}
