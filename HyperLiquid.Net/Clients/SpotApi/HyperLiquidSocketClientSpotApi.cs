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
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientSpotApi : HyperLiquidSocketClientApi, IHyperLiquidSocketClientSpotApi
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientSpotApi(ILogger logger, HyperLiquidSocketOptions options) :
            base(logger, options, options.SpotOptions)
        {
        }
        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTicker>> onMessage, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_restClient, symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<UpdateSubscription>(spotName.Error);

                coin = spotName.Data;
            }

            var subscription = new HyperLiquidSubscription<HyperLiquidTickerUpdate>(_logger, this, "activeAssetCtx", "activeSpotAssetCtx-" + coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            x =>
            {
                x.Data.Ticker.Symbol = symbol;
                onMessage(x.As(x.Data.Ticker).WithSymbol(symbol));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IHyperLiquidSocketClientSpotApiShared SharedClient => this;
    }
}
