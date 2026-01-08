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
    public interface IHyperLiquidRestClientFuturesApiAccount: IHyperLiquidRestClientAccount
    {
        /// <summary>
        /// Get account info, balances and open positions
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-users-perpetuals-account-summary" /></para>
        /// </summary>
        /// <param name="address">Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="dex">The DEX to request data for, leave null for default perp DEX</param>
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

        /// <summary>
        /// Get whether HIP-3 DEX abstraction enabled
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-hip-3-dex-abstraction-state" /></para>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WebCallResult<bool>> GetHip3DexAbstractionAsync(string? user = null, CancellationToken ct = default);

        /// <summary>
        /// Toggle HIP-3 DEX abstraction. If set, actions on HIP-3 perps will automatically transfer collateral from validator-operated USDC perps balance for HIP-3 DEXs where USDC is the collateral token, and spot otherwise. 
        /// When HIP-3 DEX abstraction is active, collateral is returned to the same source (validator-operated USDC perps or spot balance) when released from positions or open orders.
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#enable-hip-3-dex-abstraction" /></para>
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="address"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<WebCallResult> ToggleHip3DexAbstractionAsync(bool enabled, string? address = null, CancellationToken ct = default);
    }
}
