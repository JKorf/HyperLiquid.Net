using CryptoExchange.Net;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
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

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal partial class HyperLiquidSocketClientApiExchangeData : IHyperLiquidSocketClientApiExchangeData
    {
        protected readonly HyperLiquidSocketClientApi _baseClient;
        protected readonly ILogger _logger;

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientApiExchangeData(ILogger logger, HyperLiquidSocketClientApi baseClient)
        {
            _logger = logger;
            _baseClient = baseClient;
        }
        #endregion

        #region Get Prices

        /// <inheritdoc />
        public async Task<CallResult<Dictionary<string, decimal>>> GetPricesAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "allMids" }
            };
            parameters.AddOptional("dex", dex);
            var result = await _baseClient.QueryInternalAsync(new HyperLiquidRequestQuery<Dictionary<string, decimal>>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);

            var resultMapped = new Dictionary<string, decimal>();
            foreach (var item in result.Data)
            {
                var nameRes = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, item.Key).ConfigureAwait(false);
                resultMapped.Add(nameRes.Data ?? item.Key, item.Value);
            }

            return result.As(resultMapped);
        }

        #endregion

        #region Get Order Book

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidOrderBook>> GetOrderBookAsync(string symbol, int? numberSignificantFigures = null, int? mantissa = null, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<HyperLiquidOrderBook>(spotName.Error);

                coin = spotName.Data;
            }

            var parameters = new ParameterCollection()
            {
                { "type", "l2Book" },
                { "coin", coin }
            };

            parameters.AddOptional("nSigFigs", numberSignificantFigures);
            parameters.AddOptional("mantissa", mantissa);

            var result = await _baseClient.QueryInternalAsync(new HyperLiquidRequestQuery<HyperLiquidOrderBook>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Klines

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidKline[]>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            var coin = symbol;
            if (HyperLiquidUtils.SymbolIsExchangeSpotSymbol(coin))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<HyperLiquidKline[]>(spotName.Error);

                coin = spotName.Data;
            }

            var innerParameters = new ParameterCollection();
            innerParameters.Add("coin", coin);
            innerParameters.AddEnum("interval", interval);
            innerParameters.AddOptionalMilliseconds("startTime", startTime);
            innerParameters.AddOptionalMilliseconds("endTime", endTime);

            var parameters = new ParameterCollection()
            {
                { "type", "candleSnapshot" },
                { "req", innerParameters }
            };

            var result = await _baseClient.QueryInternalAsync(new HyperLiquidRequestQuery<HyperLiquidKline[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            //if (result.ResponseStatusCode == (HttpStatusCode)500 && result.Error?.ErrorType == ErrorType.Unknown)
            //    return result.AsError<HyperLiquidKline[]>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return result;
        }

        #endregion

        /// <inheritdoc />
        public Task<CallResult<UpdateSubscription>> SubscribeToPriceUpdatesAsync(Action<DataEvent<Dictionary<string, decimal>>> onMessage, CancellationToken ct = default)
            => SubscribeToPriceUpdatesAsync(null, onMessage, ct);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToPriceUpdatesAsync(string? dex, Action<DataEvent<Dictionary<string, decimal>>> onMessage, CancellationToken ct = default)
        {
            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidMidsUpdate>>((receiveTime, originalData, invocations, data) =>
            {
                var mappingResult = HyperLiquidUtils.GetSymbolNameFromExchangeName(_baseClient.ClientOptions.Environment.Name, data.Data.Mids.Keys);
                var dictData = data.Data.Mids.ToDictionary(x =>
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(x.Key))
                        return mappingResult.TryGetValue(x.Key, out var name) ? name : x.Key;

                    return x.Key;
                }, x => x.Value);

                onMessage(
                    new DataEvent<Dictionary<string, decimal>>(HyperLiquidExchange.ExchangeName, dictData, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                    );
            });


            var subscription = new HyperLiquidSubscription<HyperLiquidMidsUpdate>(_logger, _baseClient, "allMids", null, new Dictionary<string, object>
            {
                { "dex", dex ?? "" }
            }, internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<HyperLiquidKline>> onMessage, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidKline>>((receiveTime, originalData, invocation, data) =>
            {
                data.Data.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidKline>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                    );
            });

            var intervalStr = EnumConverter.GetString(interval);
            var subscription = new HyperLiquidSubscription<HyperLiquidKline>(_logger, _baseClient, "candle", $"{coin}{interval}", new Dictionary<string, object>
            {
                { "coin", coin },
                { "interval", intervalStr }
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidOrderBook>> onMessage, int? nSigFigs = null, int? mantissa = null, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidOrderBook>>((receiveTime, originalData, invocation, data) =>
            {
                _baseClient.UpdateTimeOffset(data.Data.Timestamp);

                data.Data.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidOrderBook>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var parameters = new Dictionary<string, object>
            {
                { "coin", coin }
            };
            
            parameters.AddOptionalParameter("nSigFigs", nSigFigs);
            parameters.AddOptionalParameter("mantissa", mantissa);
            
            var subscription = new HyperLiquidSubscription<HyperLiquidOrderBook>(_logger, _baseClient, "l2Book", coin, parameters, internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTrade[]>> onMessage, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTrade[]>>((receiveTime, originalData, invocation, data) =>
            {
                var timestamp = data.Data.Max(x => x.Timestamp);
                if (invocation != 1)
                    _baseClient.UpdateTimeOffset(timestamp);

                foreach (var trade in data.Data)
                    trade.Symbol = symbol;

                onMessage(
                    new DataEvent<HyperLiquidTrade[]>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(invocation == 1 ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidTrade[]>(_logger, _baseClient, "trades", coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync( subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToBookTickerUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidBookTicker>> onMessage, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidBookTicker>>((receiveTime, originalData, invocation, data) =>
            {
                _baseClient.UpdateTimeOffset(data.Data.Timestamp);

                data.Data.Symbol = symbol;

                onMessage(
                    new DataEvent<HyperLiquidBookTicker>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidBookTicker>(_logger, _baseClient, "bbo", coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
