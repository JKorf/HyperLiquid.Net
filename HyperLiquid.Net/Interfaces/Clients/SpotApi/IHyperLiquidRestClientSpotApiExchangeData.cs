using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot exchange data endpoints. Exchange data includes market data (tickers, order books, etc) and system status.
    /// </summary>
    /// <see cref="IHyperLiquidRestClientExchangeData"/>
    public interface IHyperLiquidRestClientSpotApiExchangeData : IHyperLiquidRestClientExchangeData
    {
        /// <summary>
        /// Get exchange info
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-spot-metadata" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidSpotExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default);

        /// <summary>
        /// Get exchange and ticker info
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-spot-asset-contexts" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get information on an asset
        /// <para><a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-information-about-a-token" /></para>
        /// </summary>
        /// <param name="assetId">The asset id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidAssetInfo>> GetAssetInfoAsync(string assetId, CancellationToken ct = default);
    }
}
