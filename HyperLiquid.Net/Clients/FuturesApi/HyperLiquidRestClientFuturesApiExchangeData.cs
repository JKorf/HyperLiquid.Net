using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using System.Linq;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using CryptoExchange.Net.Objects.Errors;
using System.Net;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientFuturesApiExchangeData : HyperLiquidRestClientExchangeData, IHyperLiquidRestClientFuturesApiExchangeData
    {
        private readonly HyperLiquidRestClientFuturesApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();

        internal HyperLiquidRestClientFuturesApiExchangeData(ILogger logger, HyperLiquidRestClientFuturesApi baseClient) : base(logger, baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Futures Exchange Info

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFuturesSymbol[]>> GetExchangeInfoAsync(string? dex, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "meta" },
                { "dex", dex ?? string.Empty }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidFuturesExchangeInfo>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result.As<HyperLiquidFuturesSymbol[]>(default);

            for (var i = 0; i < result.Data.Symbols.Count(); i++)
            {
                var symbol = result.Data.Symbols.ElementAt(i);
                symbol.Index = i;
                if (symbol.MarginTableId < 50)
                {
                    symbol.MarginTable = new HyperLiquidMarginTableEntry
                    {
                        MarginTiers = [
                            new HyperLiquidMarginTableTier {
                                LowerBound = 0,
                                MaxLeverage = symbol.MarginTableId
                            }
                        ]
                    };
                }
                else
                {
                    symbol.MarginTable = result.Data.MarginTables.Single(x => x.Id == symbol.MarginTableId).Table;
                }
            }

            return result.As<HyperLiquidFuturesSymbol[]>(result.Data?.Symbols);
        }

        #endregion

        #region Get Futures Exchange Info And Tickers

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFuturesExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default)
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

            for (var i = 0; i < result.Data.Tickers.Count(); i++)
                result.Data.Tickers.ElementAt(i).Symbol = result.Data.ExchangeInfo.Symbols.ElementAt(i).Name;

            return result;
        }

        #endregion

        #region Get Funding Rate History

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFundingRate[]>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime? endTime = null, CancellationToken ct = default)
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
            var result = await _baseClient.SendAsync<HyperLiquidFundingRate[]>(request, parameters, ct).ConfigureAwait(false);
            if (result.ResponseStatusCode == (HttpStatusCode)500 && result.Error?.ErrorType == ErrorType.Unknown)
                return result.AsError<HyperLiquidFundingRate[]>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return result;
        }

        #endregion

        #region Get Futures Symbols At Max Open Interest

        /// <inheritdoc />
        public async Task<WebCallResult<string[]>> GetSymbolsAtMaxOpenInterestAsync(CancellationToken ct = default)
        {
            var innerParameters = new ParameterCollection();

            var parameters = new ParameterCollection()
            {
                { "type", "perpsAtOpenInterestCap" },
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<string[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
