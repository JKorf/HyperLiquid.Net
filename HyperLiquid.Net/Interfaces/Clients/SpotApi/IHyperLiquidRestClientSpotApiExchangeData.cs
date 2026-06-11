using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Utils;

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
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-spot-metadata" /><br />
        /// Endpoint:<br />
        /// POST /info (type: spotMeta)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<HyperLiquidSpotExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default);

        /// <summary>
        /// Get exchange and ticker info. For tickers:
        /// Names starting with '@' failed to map to a live symbol, potentially due to delisting. <br />
        /// Names starting with '#' are HIP-4 outcomes, details can be retrieved with <see cref="HyperLiquidUtils.GetOutcomeInfoAsync(Net.Clients.HyperLiquidRestClient, string)" /> or <see cref="IHyperLiquidRestClientSpotApiExchangeData.GetQuestionsAndOutcomesInfoAsync(CancellationToken)"/>.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-spot-asset-contexts" /><br />
        /// Endpoint:<br />
        /// POST /info (type: spotMetaAndAssetCtxs)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<HyperLiquidExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get information on an asset
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-information-about-a-token" /><br />
        /// Endpoint:<br />
        /// POST /info (type: tokenDetails)
        /// </para>
        /// </summary>
        /// <param name="assetId">["<c>tokenId</c>"] The asset id</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<HyperLiquidAssetInfo>> GetAssetInfoAsync(string assetId, CancellationToken ct = default);

        /// <summary>
        /// Get HIP-4 questions and outcomes info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-outcome-metadata" /><br />
        /// Endpoint:<br />
        /// POST /info (type: outcomeMeta)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<HyperLiquidQuestionsAndOutcomesInfo>> GetQuestionsAndOutcomesInfoAsync(CancellationToken ct = default);

        /// <summary>
        /// Get HIP-4 settled outcome info
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-information-about-a-settled-outcome" /><br />
        /// Endpoint:<br />
        /// POST /info (type: settledOutcome)
        /// </para>
        /// </summary>
        /// <param name="outcomeId">The outcome id</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<HyperLiquidSettledOutcome>> GetSettledOutcomeAsync(long outcomeId, CancellationToken ct = default);
    }
}
