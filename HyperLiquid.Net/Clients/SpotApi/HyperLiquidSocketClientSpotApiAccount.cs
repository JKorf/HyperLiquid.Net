using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using HyperLiquid.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientSpotApiAccount : HyperLiquidSocketClientApiAccount, IHyperLiquidSocketClientSpotApiAccount
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientSpotApiAccount(ILogger logger, HyperLiquidSocketClientApi baseClient) :
            base(logger, baseClient)
        {
        }
        #endregion

        #region Get Spot Balances

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidBalance[]>> GetBalancesAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "spotClearinghouseState" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidBalances>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result.As<HyperLiquidBalance[]>(result.Data?.Balances);
        }

        #endregion

        #region Spot Transfer

        /// <inheritdoc />
        public async Task<CallResult> TransferSpotAsync(
            string destinationAddress,
            string asset,
            decimal quantity,
            CancellationToken ct = default)
        {
            var assetId = await HyperLiquidUtils.GetAssetNameAndIdAsync(_baseClient.BaseClient, asset).ConfigureAwait(false);
            if (!assetId)
                return new WebCallResult(assetId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "spotSend" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress },
                { "token", assetId.Data }
            };
            actionParameters.AddString("amount", quantity);
            actionParameters.AddMilliseconds("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<object>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
        }

        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(string? address, Action<DataEvent<HyperLiquidBalanceUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidBalanceUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidBalanceUpdate>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidBalanceUpdate>(_logger, _baseClient, "spotState", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() }
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
