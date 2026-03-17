using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Clients.BaseApi;
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
    internal partial class HyperLiquidSocketClientFuturesApiAccount : HyperLiquidSocketClientApiAccount, IHyperLiquidSocketClientFuturesApiAccount
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientFuturesApiAccount(ILogger logger, HyperLiquidSocketClientApi baseClient)
            : base(logger, baseClient)
        {
        }
        #endregion

        #region Get Futures Account

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidFuturesAccount>> GetAccountInfoAsync(string? address = null, string? dex = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "clearinghouseState" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.AddOptional("dex", dex);
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFuturesAccount>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Funding History

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidUserLedger<HyperLiquidUserFunding>[]>> GetFundingHistoryAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userFunding" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidUserLedger<HyperLiquidUserFunding>[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get User Symbol

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidFuturesUserSymbolUpdate>> GetUserSymbolAsync(string symbol, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "activeAssetData" },
                { "coin", symbol },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFuturesUserSymbolUpdate>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get HIP-3 DEX Abstraction

        /// <inheritdoc />
        public async Task<CallResult<bool>> GetHip3DexAbstractionAsync(string? user = null, CancellationToken ct = default)
        {
            if (user == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(user), "User needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userDexAbstraction" },
                { "user", user ?? _baseClient.AuthenticationProvider!.Key }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<bool>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Toggle HIP-3 DEX Abstraction

        /// <inheritdoc />
        public async Task<CallResult> ToggleHip3DexAbstractionAsync(bool enabled, string? user = null, CancellationToken ct = default)
        {
            if (user == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(user), "User needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "userDexAbstraction" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "user", user ?? _baseClient.AuthenticationProvider!.Key }
            };

            actionParameters.Add("enabled", enabled);
            actionParameters.AddMilliseconds("nonce", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<object>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserSymbolUpdatesAsync(string? address, string symbol, Action<DataEvent<HyperLiquidFuturesUserSymbolUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidFuturesUserSymbolUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidFuturesUserSymbolUpdate>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithSymbol(symbol)
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesUserSymbolUpdate>(_logger, _baseClient, "activeAssetData", symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserFundingUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserFunding[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserFundingUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                DateTime? timestamp = data.Data.Fundings.Length != 0 ? data.Data.Fundings.Max(x => x.Timestamp) : null;
                if (!data.Data.IsSnapshot && timestamp != null)
                    _baseClient.UpdateTimeOffset(timestamp!.Value);

                onMessage(
                    new DataEvent<HyperLiquidUserFunding[]>(HyperLiquidExchange.ExchangeName, data.Data.Fundings, receiveTime, originalData)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserFundingUpdate>(_logger, _baseClient, "userFundings", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

    }
}
