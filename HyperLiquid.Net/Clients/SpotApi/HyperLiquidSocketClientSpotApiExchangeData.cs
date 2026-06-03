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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientSpotApiExchangeData : HyperLiquidSocketClientApiExchangeData, IHyperLiquidSocketClientSpotApiExchangeData
    {
        private static readonly ParameterSerializationSettings _parameterSerializationSettings = new ParameterSerializationSettings()
        {
            Decimal = DecimalSerialization.String,
            Sort = false
        };

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
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
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
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
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
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "tokenDetails" },
                { "tokenId", assetId }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidAssetInfo>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion


        #region Get Outcomes Info

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidQuestionsAndOutcomesInfo>> GetQuestionsAndOutcomesInfoAsync(CancellationToken ct = default)
        {
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "outcomeMeta" }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidQuestionsAndOutcomesInfo>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Settled Outcome

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidSettledOutcome>> GetSettledOutcomeAsync(long outcomeId, CancellationToken ct = default)
        {
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "settledOutcome" },
                { "outcome", outcomeId }
            };
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidSettledOutcome>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
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


        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOutcomeInfoUpdatesAsync(
            Action<DataEvent<long>>? onOutcomeCreateUpdate = null,
            Action<DataEvent<HyperLiquidOutcomeInfo>>? onOutcomeSettleUpdate = null,
            Action<DataEvent<HyperLiquidQuestion>>? onQuestionUpdate = null,
            Action<DataEvent<long>>? onQuestionSettleUpdate = null,
            CancellationToken ct = default)
        {
            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidOutcomeUpdate[]>>((receiveTime, originalData, invocation, data) =>
            {
                foreach(var update in data.Data)
                {
                    if (update.OutcomeSettled != null)
                    {
                        onOutcomeCreateUpdate?.Invoke(new DataEvent<long>(HyperLiquidExchange.ExchangeName, update.OutcomeSettled.Value, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                    }

                    else if (update.OutcomeCreated != null)
                    {
                        onOutcomeSettleUpdate?.Invoke(new DataEvent<HyperLiquidOutcomeInfo>(HyperLiquidExchange.ExchangeName, update.OutcomeCreated, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                    }
                    else if (update.QuestionUpdated != null)
                    {
                        onQuestionUpdate?.Invoke(new DataEvent<HyperLiquidQuestion>(HyperLiquidExchange.ExchangeName, update.QuestionUpdated, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                    }
                    else if (update.QuestionSettled != null)
                    {
                        onQuestionSettleUpdate?.Invoke(new DataEvent<long>(HyperLiquidExchange.ExchangeName, update.QuestionSettled.Value, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                    }
                }
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidOutcomeUpdate[]>(_logger, _baseClient, "outcomeMetaUpdates", null, new Dictionary<string, object>
            {
            },
            internalHandler, false, "outcomeMetaUpdates");
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
