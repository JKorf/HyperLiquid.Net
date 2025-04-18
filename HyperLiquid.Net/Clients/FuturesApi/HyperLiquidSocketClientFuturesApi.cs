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
            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesTickerUpdate>(_logger, "activeAssetCtx", "activeAssetCtx-" + symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
            },
            x =>
            {
                x.Data.Ticker.Symbol = symbol;
                onMessage(x.As(x.Data.Ticker).WithSymbol(symbol));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserSymbolUpdatesAsync(string? address, string symbol, Action<DataEvent<HyperLiquidFuturesUserSymbolUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesUserSymbolUpdate>(_logger, "activeAssetData", "activeAssetData-" + symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
                { "user", addressSub },
            },
            x =>
            {
                onMessage(x.WithSymbol(symbol));
            }, false);
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

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserFundingUpdate>(_logger, "userFundings", "userFundings", new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            x =>
            {
                onMessage(x.As(x.Data.Fundings).WithUpdateType(x.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                    .WithDataTimestamp(x.Data.Fundings.Any() ? x.Data.Fundings.Max(x => x.Timestamp) : null));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IHyperLiquidSocketClientFuturesApiShared SharedClient => this;
    }
}
