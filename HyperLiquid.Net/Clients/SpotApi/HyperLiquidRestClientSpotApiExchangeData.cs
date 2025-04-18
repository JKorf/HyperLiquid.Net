using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Utils;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientSpotApiExchangeData : HyperLiquidRestClientExchangeData, IHyperLiquidRestClientSpotApiExchangeData
    {
        private readonly HyperLiquidRestClientSpotApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();

        internal HyperLiquidRestClientSpotApiExchangeData(ILogger logger, HyperLiquidRestClientSpotApi baseClient) : base(logger, baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Spot Exchange Info

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidSpotExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "spotMeta" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidSpotExchangeInfo>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Spot Exchange Info And Tickers

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "spotMetaAndAssetCtxs" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidExchangeInfoAndTickers>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            foreach (var ticker in result.Data.Tickers)
            {
                var nameResult = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, ticker.Symbol!).ConfigureAwait(false);
                if (nameResult)
                    ticker.Symbol = nameResult.Data;
            }

            return result;
        }

        #endregion

        #region Get Asset Info

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidAssetInfo>> GetAssetInfoAsync(string assetId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "tokenDetails" },
                { "tokenId", assetId }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidAssetInfo>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

    }
}
