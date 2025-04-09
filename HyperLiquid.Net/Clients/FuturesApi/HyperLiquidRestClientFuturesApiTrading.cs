using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using HyperLiquid.Net.Utils;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientFuturesApiTrading : HyperLiquidRestClientTrading, IHyperLiquidRestClientFuturesApiTrading
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly HyperLiquidRestClientFuturesApi _baseClient;
        private readonly ILogger _logger;

        internal HyperLiquidRestClientFuturesApiTrading(ILogger logger, HyperLiquidRestClientFuturesApi baseClient) : base(logger, baseClient)
        {
            _baseClient = baseClient;
            _logger = logger;
        }

        #region Set Leverage

        /// <inheritdoc />
        public async Task<WebCallResult> SetLeverageAsync(string symbol, int leverage, MarginType marginType, CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "updateLeverage" },
                { "asset", symbolId.Data }
            };
            actionParameters.Add("isCross", marginType == MarginType.Cross);
            actionParameters.Add("leverage", leverage);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAsync<HyperLiquidResponse>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Update Isolated Margin

        /// <inheritdoc />
        public async Task<WebCallResult> UpdateIsolatedMarginAsync(string symbol, decimal updateValue, CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "updateIsolatedMargin" },
                { "asset", symbolId.Data },
                { "isBuy", true }
            };
            actionParameters.Add("ntli", updateValue);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAsync<HyperLiquidResponse>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion
    }
}
