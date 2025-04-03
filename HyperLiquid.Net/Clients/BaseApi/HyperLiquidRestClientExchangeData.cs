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
using CryptoExchange.Net.Objects.Sockets;

namespace HyperLiquid.Net.Clients.BaseApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientExchangeData
    {
        private readonly HyperLiquidRestClientApi _baseClient;
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();

        internal HyperLiquidRestClientExchangeData(ILogger logger, HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Prices

        /// <inheritdoc />
        public async Task<WebCallResult<Dictionary<string, decimal>>> GetPricesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "allMids" }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<Dictionary<string, decimal>>(request, parameters, ct).ConfigureAwait(false);
            
            var resultMapped = new Dictionary<string, decimal>();
            foreach (var item in result.Data)
            {
                var nameRes = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, item.Key).ConfigureAwait(false);
                resultMapped.Add(nameRes.Data ?? item.Key, item.Value);
            }

            return result.As(resultMapped);
        }

        #endregion

        #region Get Order Book

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOrderBook>> GetOrderBookAsync(string symbol, int? numberSignificantFigures = null, int? mantissa = null, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<HyperLiquidOrderBook>(spotName.Error);

                coin = spotName.Data;
            }

            var parameters = new ParameterCollection()
            {
                { "type", "l2Book" },
                { "coin", coin }
            };

            parameters.AddOptional("nSigFigs", numberSignificantFigures);
            parameters.AddOptional("mantissa", mantissa);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidOrderBook>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Klines

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidKline[]>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<HyperLiquidKline[]>(spotName.Error);

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
            return await _baseClient.SendAsync<HyperLiquidKline[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
