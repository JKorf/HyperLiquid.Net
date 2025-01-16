using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Interfaces.Clients.Api;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Enums;
using System.Linq;
using HyperLiquid.Net.Utils;

namespace HyperLiquid.Net.Clients.Api
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientApiExchangeData : IHyperLiquidRestClientApiExchangeData
    {
        private readonly HyperLiquidRestClientApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();

        internal HyperLiquidRestClientApiExchangeData(ILogger logger, HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Spot Exchange Info

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidSpotExchangeInfo>> GetSpotExchangeInfoAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "spotMeta" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidSpotExchangeInfo>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Futures Exchange Info

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<HyperLiquidFuturesSymbol>>> GetFuturesExchangeInfoAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "meta" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidFuturesExchangeInfo>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result.As<IEnumerable<HyperLiquidFuturesSymbol>>(default);

            for (var i = 0; i < result.Data.Symbols.Count(); i++)
                result.Data.Symbols.ElementAt(i).Index = i;

            return result.As<IEnumerable<HyperLiquidFuturesSymbol>>(result.Data?.Symbols);
        }

        #endregion

        #region Get Spot Exchange Info And Tickers

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidExchangeInfoAndTickers>> GetSpotExchangeInfoAndTickersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "spotMetaAndAssetCtxs" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidExchangeInfoAndTickers>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Futures Exchange Info And Tickers

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFuturesExchangeInfoAndTickers>> GetFuturesExchangeInfoAndTickersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "metaAndAssetCtxs" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidFuturesExchangeInfoAndTickers>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            for (var i = 0; i < result.Data.ExchangeInfo.Symbols.Count(); i++)
                result.Data.ExchangeInfo.Symbols.ElementAt(i).Index = i;

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

        #region Get Prices

        /// <inheritdoc />
        public async Task<WebCallResult<Dictionary<string, decimal>>> GetPricesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "allMids" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<Dictionary<string, decimal>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Order Book

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOrderBook>> GetOrderBookAsync(string asset, int? numberSignificantFigures = null, int? mantissa = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "l2Book" },
                { "coin", asset }
            };

            parameters.AddOptional("nSigFigs", numberSignificantFigures);
            parameters.AddOptional("mantissa", mantissa);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidOrderBook>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Klines

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<HyperLiquidKline>>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            var coin = symbol;
            if (symbol.Contains("/"))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<IEnumerable<HyperLiquidKline>>(spotName.Error);

                coin = spotName.Data;
            }

            var innerParameters = new ParameterCollection();
            innerParameters.Add("coin", coin);
            innerParameters.AddEnum("interval", interval);
            innerParameters.AddOptionalMilliseconds("startTime", startTime);
            innerParameters.AddOptionalMilliseconds("endTime", endTime);

            var parameters = new ParameterCollection()
            {
                { "type", "candleSnapshot" },
                { "req", innerParameters }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<IEnumerable<HyperLiquidKline>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Funding Rate History

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<HyperLiquidFundingRate>>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime? endTime = null, CancellationToken ct = default)
        {
            var innerParameters = new ParameterCollection();
            var parameters = new ParameterCollection()
            {
                { "type", "fundingHistory" },
            };
            parameters.Add("coin", symbol);
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<IEnumerable<HyperLiquidFundingRate>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Futures Symbols At Max Open Interest

        /// <inheritdoc />
        public async Task<WebCallResult<IEnumerable<string>>> GetFuturesSymbolsAtMaxOpenInterestAsync(CancellationToken ct = default)
        {
            var innerParameters = new ParameterCollection();

            var parameters = new ParameterCollection()
            {
                { "type", "perpsAtOpenInterestCap" },
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<IEnumerable<string>>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
