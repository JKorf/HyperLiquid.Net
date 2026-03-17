using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientFuturesApiTrading : HyperLiquidSocketClientApiTrading, IHyperLiquidSocketClientFuturesApiTrading
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientFuturesApiTrading(ILogger logger, HyperLiquidSocketClientApi baseClient)
            : base(logger, baseClient)
        {
        }
        #endregion


        #region Set Leverage

        /// <inheritdoc />
        public async Task<CallResult> SetLeverageAsync(
            string symbol,
            int leverage,
            MarginType marginType,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<object>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Update Isolated Margin

        /// <inheritdoc />
        public async Task<CallResult> UpdateIsolatedMarginAsync(
            string symbol,
            decimal updateValue,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<object>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion
    }
}
