using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid account endpoints. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    public interface IHyperLiquidRestClientAccount
    {
        /// <summary>
        /// Get user trading fee rates
        /// </summary>
        /// <param name="address">Address to request open orders for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidFeeInfo>> GetFeeInfoAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// This generalized method is used to transfer tokens between different perp DEXs, spot balance, users, and/or sub-accounts. Use "" to specify the default USDC perp DEX and "spot" to specify spot. Only the collateral token can be transferred to or from a perp DEX.
        /// </summary>
        /// <param name="destination">Destination address</param>
        /// <param name="sourceDex">Source Dex, empty string for the default USDC DEX, "spot" for spot</param>
        /// <param name="destinationDex">Destination Dex, empty string for the default USDC DEX, "spot" for spot</param>
        /// <param name="tokenName">Token name, for example `PURR`</param>
        /// <param name="tokenId">Token id, for example `0xc4bf3f870c0e9465323c0b6ed28096c2`</param>
        /// <param name="quantity">Quantity to send</param>
        /// <param name="fromSubAccount">Source sub account</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> SendAssetAsync(
            string destination,
            string sourceDex,
            string destinationDex,
            string tokenName,
            string tokenId,
            decimal quantity,
            string? fromSubAccount = null,
            CancellationToken ct = default);
    }
}
