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
using System;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientFuturesApiTrading : HyperLiquidRestClientApiTrading, IHyperLiquidRestClientFuturesApiTrading
    {
        private static readonly ParameterSerializationSettings _parameterSerializationSettings = new ParameterSerializationSettings()
        {
            Decimal = DecimalSerialization.String,
            Sort = false
        };
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
        public async Task<HttpResult> SetLeverageAsync(
            string symbol,
            int leverage,
            MarginType marginType,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId.Success)
                return HttpResult.Fail(_baseClient.Exchange, symbolId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "updateLeverage" },
                { "asset", symbolId.Data }
            };
            actionParameters.Add("isCross", marginType == MarginType.Cross);
            actionParameters.Add("leverage", leverage);
            parameters.Add("action", actionParameters);
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAsync<HyperLiquidResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Update Isolated Margin

        /// <inheritdoc />
        public async Task<HttpResult> UpdateIsolatedMarginAsync(
            string symbol,
            decimal updateValue,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId.Success)
                return HttpResult.Fail(_baseClient.Exchange, symbolId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "updateIsolatedMargin" },
                { "asset", symbolId.Data },
                { "isBuy", true }
            };
            actionParameters.Add("ntli", updateValue);
            parameters.Add("action", actionParameters);
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAsync<HyperLiquidResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion
    }
}
