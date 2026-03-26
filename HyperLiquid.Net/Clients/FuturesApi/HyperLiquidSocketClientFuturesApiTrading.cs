using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using HyperLiquid.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBalanceAndPositionUpdatesAsync(string? address, string? dex, Action<DataEvent<HyperLiquidPositionUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidPositionUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                _baseClient.UpdateTimeOffset(data.Data.Data.Timestamp);

                onMessage(
                    new DataEvent<HyperLiquidPositionUpdate>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Data.Timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidPositionUpdate>(_logger, _baseClient, "clearinghouseState", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
                { "dex", dex ?? "" },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBalanceAndPositionUpdatesAllDexesAsync(string? address, Action<DataEvent<HyperLiquidAllDexPositionUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidAllDexPositionUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidAllDexPositionUpdate>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidAllDexPositionUpdate>(_logger, _baseClient, "allDexsClearinghouseState", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() }
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
