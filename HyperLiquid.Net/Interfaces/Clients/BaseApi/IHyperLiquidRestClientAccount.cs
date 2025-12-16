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
    }
}
