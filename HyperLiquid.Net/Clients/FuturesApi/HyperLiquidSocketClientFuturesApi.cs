using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using System.Collections.Generic;
using CryptoExchange.Net;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Utils;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using System.Linq;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientFuturesApi : HyperLiquidSocketClientApi, IHyperLiquidSocketClientFuturesApi
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientFuturesApi(ILogger logger, HyperLiquidSocketOptions options) :
            base(logger, options, options.FuturesOptions)
        {
        }
        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidFuturesTicker>> onMessage, CancellationToken ct = default)
        {
            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidFuturesTickerUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                data.Data.Ticker.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidFuturesTicker>(data.Data.Ticker, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithSymbol(symbol)
                    );
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesTickerUpdate>(_logger, this, "activeAssetCtx", symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserSymbolUpdatesAsync(string? address, string symbol, Action<DataEvent<HyperLiquidFuturesUserSymbolUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidFuturesUserSymbolUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidFuturesUserSymbolUpdate>(data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithSymbol(symbol)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesUserSymbolUpdate>(_logger, this, "activeAssetData", symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserFundingUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserFunding[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_restClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserFundingUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidUserFunding[]>(data.Data.Fundings, receiveTime, originalData)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Fundings.Any() ? data.Data.Fundings.Max(x => x.Timestamp) : null)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserFundingUpdate>(_logger, this, "userFundings", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IHyperLiquidSocketClientFuturesApiShared SharedClient => this;
    }
}
