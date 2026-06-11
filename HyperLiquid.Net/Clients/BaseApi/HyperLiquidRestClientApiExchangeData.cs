using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Utils;
using CryptoExchange.Net.Objects.Errors;
using System.Net;

namespace HyperLiquid.Net.Clients.BaseApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientApiExchangeData
    {
        private static readonly ParameterSerializationSettings _parameterSerializationSettings = new ParameterSerializationSettings()
        {
            Decimal = DecimalSerialization.String,
            Sort = false
        };
        private readonly HyperLiquidRestClientApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();

        internal HyperLiquidRestClientApiExchangeData(ILogger logger, HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Prices

        /// <inheritdoc />
        public async Task<HttpResult<Dictionary<string, decimal>>> GetPricesAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "allMids" }
            };
            parameters.Add("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<Dictionary<string, decimal>>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            if (result.Data == null)
                return HttpResult.Fail<Dictionary<string, decimal>>(result, new ServerError(new ErrorInfo(ErrorType.InvalidParameter, "DEX not found")));

            var resultMapped = new Dictionary<string, decimal>();
            foreach (var item in result.Data)
            {
                var nameRes = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, item.Key).ConfigureAwait(false);
                resultMapped.Add(nameRes.Data ?? item.Key, item.Value);
            }

            return HttpResult.Ok(result, resultMapped);
        }

        #endregion

        #region Get Order Book

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidOrderBook>> GetOrderBookAsync(string symbol, int? numberSignificantFigures = null, int? mantissa = null, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName.Success)
                    return HttpResult.Fail<HyperLiquidOrderBook>(_baseClient.ExchangeName, spotName.Error);

                coin = spotName.Data;
            }

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "l2Book" },
                { "coin", coin }
            };

            parameters.Add("nSigFigs", numberSignificantFigures);
            parameters.Add("mantissa", mantissa);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrderBook>(request, parameters, ct).ConfigureAwait(false);
            if (result.Data == null)
                return HttpResult.Fail<HyperLiquidOrderBook>(result, new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));
            
            return result;
        }

        #endregion

        #region Get Klines

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidKline[]>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName.Success)
                    return HttpResult.Fail<HyperLiquidKline[]>(_baseClient.ExchangeName, spotName.Error);

                coin = spotName.Data;
            }

            var innerParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            innerParameters.Add("coin", coin);
            innerParameters.Add("interval", interval);
            innerParameters.Add("startTime", startTime);
            innerParameters.Add("endTime", endTime);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "candleSnapshot" },
                { "req", innerParameters }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidKline[]>(request, parameters, ct).ConfigureAwait(false);
            if (result.ResponseStatusCode == (HttpStatusCode)500 && result.Error?.ErrorType == ErrorType.Unknown)
                return HttpResult.Fail<HyperLiquidKline[]>(result, new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return result;
        }

        #endregion
    }
}
