using System;
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
using System.Collections.Generic;

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

        public async Task<WebCallResult<HyperLiquidPerpDex[]>> GetPerpDexesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpDexs" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidPerpDex[]>(request, parameters, ct).ConfigureAwait(false);

            return result;
        }

        public async Task<WebCallResult<HyperLiquidFuturesDexInfo[]>> GetExchangeInfoAllDexesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "allPerpMetas" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidFuturesExchangeInfo[]>(request, parameters, ct).ConfigureAwait(false);

            for (var j = 0; j < result.Data.Length; j++)
            {
                var data = result.Data[j];
                for (var i = 0; i < data.Symbols.Length; i++)
                {
                    var symbol = data.Symbols.ElementAt(i);
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
                        symbol.MarginTable = data.MarginTables.Single(x => x.Id == symbol.MarginTableId).Table;
                    }
                }
            }

            int index = 0;
            return result.As(result.Data.Select(x =>
            {
                var symbolName = x.Symbols.FirstOrDefault()?.Name;
                return new HyperLiquidFuturesDexInfo
                {
                    Name = symbolName?.Contains(':') == true ? symbolName.Split(':')[0] : "default",
                    Symbols = x.Symbols,
                    Index = index++
                };
            }).ToArray());
        }

        #region Get Futures Exchange Info

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFuturesSymbol[]>> GetExchangeInfoAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "meta" }
            };
            parameters.AddOptional("dex", dex);

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
        public async Task<WebCallResult<string[]>> GetSymbolsAtMaxOpenInterestAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpsAtOpenInterestCap" },
            };
            parameters.AddOptional("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<string[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Perp DEX Market Limits

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidPerpDexLimit>> GetPerpDexMarketLimitsAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpDexLimits" },
            };
            parameters.AddOptional("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidPerpDexLimit>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Perp Market Status

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidPerpDexStatus>> GetPerpDexMarketStatusAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpDexStatus" },
            };
            parameters.AddOptional("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidPerpDexStatus>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
