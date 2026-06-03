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
    internal class HyperLiquidRestClientSpotApiExchangeData : HyperLiquidRestClientApiExchangeData, IHyperLiquidRestClientSpotApiExchangeData
    {
        private static readonly ParameterSerializationSettings _parameterSerializationSettings = new ParameterSerializationSettings()
        {
            Decimal = DecimalSerialization.String,
            Sort = false
        };
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
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
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
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
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
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "tokenDetails" },
                { "tokenId", assetId }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidAssetInfo>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Outcomes Info

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidQuestionsAndOutcomesInfo>> GetQuestionsAndOutcomesInfoAsync(CancellationToken ct = default)
        {
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "outcomeMeta" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidQuestionsAndOutcomesInfo>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Settled Outcome

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidSettledOutcome>> GetSettledOutcomeAsync(long outcomeId, CancellationToken ct = default)
        {
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "settledOutcome" },
                { "outcome", outcomeId }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidSettledOutcome>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion
    }
}
