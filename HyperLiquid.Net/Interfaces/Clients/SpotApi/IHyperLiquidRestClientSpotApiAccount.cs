using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Enums;
using System;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot account endpoints. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    /// <see cref="IHyperLiquidRestClientAccount"/>
    public interface IHyperLiquidRestClientSpotApiAccount : IHyperLiquidRestClientAccount
    {
        /// <summary>
        /// Get user asset balances
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-a-users-token-balances" /><br />
        /// Endpoint:<br />
        /// POST /info (type: spotClearinghouseState)
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidBalance[]>> GetBalancesAsync(string? address = null, CancellationToken ct = default);
                
        /// <summary>
        /// Send spot assets to another address. This transfer does not touch the EVM bridge.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#l1-spot-transfer" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: spotSend)
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">["<c>destination</c>"] Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="asset">Asset name, for example "HYPE"</param>
        /// <param name="quantity">["<c>amount</c>"] Quantity to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> TransferSpotAsync(
            string destinationAddress,
            string asset,
            decimal quantity,
            CancellationToken ct = default);

    }
}
