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
    internal partial class HyperLiquidSocketClientSpotApiExchangeData : HyperLiquidSocketClientApiExchangeData, IHyperLiquidSocketClientSpotApiExchangeData
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientSpotApiExchangeData(ILogger logger, HyperLiquidSocketClientApi baseClient) :
            base(logger, baseClient)
        {
        }
        #endregion



        #region Get Spot Exchange Info

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidSpotExchangeInfo>> GetExchangeInfoAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "spotMeta" }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidSpotExchangeInfo>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Spot Exchange Info And Tickers

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "spotMetaAndAssetCtxs" }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidExchangeInfoAndTickers>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            if (!result)
                return result;

            foreach (var ticker in result.Data.Tickers)
            {
                var nameResult = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, ticker.Symbol!).ConfigureAwait(false);
                if (nameResult)
                    ticker.Symbol = nameResult.Data;
            }

            return result;
        }

        #endregion

        #region Get Asset Info

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidAssetInfo>> GetAssetInfoAsync(string assetId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "tokenDetails" },
                { "tokenId", assetId }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidAssetInfo>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion


        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTicker>> onMessage, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<UpdateSubscription>(spotName.Error);

                coin = spotName.Data;
            }

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTickerUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                data.Data.Ticker.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidTicker>(HyperLiquidExchange.ExchangeName, data.Data.Ticker, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithSymbol(symbol)
                    );
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidTickerUpdate>(_logger, _baseClient, "activeAssetCtx", coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            internalHandler, false, "activeSpotAssetCtx");
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
