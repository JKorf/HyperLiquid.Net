using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using System.Collections.Generic;
using CryptoExchange.Net;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Utils;
using System.Linq;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Objects.Sockets;
using CryptoExchange.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using System.Text.Json;
using HyperLiquid.Net.Interfaces.Clients;
using System.Net.WebSockets;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using HyperLiquid.Net.Clients.MessageHandlers;

namespace HyperLiquid.Net.Clients.BaseApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientApi : SocketApiClient, IHyperLiquidSocketClientApi
    {
        #region fields
        private static readonly MessagePath _channelPath = MessagePath.Get().Property("channel");
        private static readonly MessagePath _subscriptionTopicPath = MessagePath.Get().Property("data").Property("subscription").Property("type");
        private static readonly MessagePath _subscriptionCoinPath = MessagePath.Get().Property("data").Property("subscription").Property("coin");
        private static readonly MessagePath _subscriptionIntervalPath = MessagePath.Get().Property("data").Property("subscription").Property("interval");
        private static readonly MessagePath _subscriptionUserPath = MessagePath.Get().Property("data").Property("subscription").Property("user");
        
        private static readonly MessagePath _dataPath = MessagePath.Get().Property("data");
        private static readonly MessagePath _symbolPath = MessagePath.Get().Property("data").Property("s");
        private static readonly MessagePath _itemSymbolPath = MessagePath.Get().Property("data").Index(0).Property("coin");
        private static readonly MessagePath _bookSymbolPath = MessagePath.Get().Property("data").Property("coin");
        private static readonly MessagePath _klineIntervalPath = MessagePath.Get().Property("data").Property("i");

        protected IHyperLiquidRestClient _restClient;

        protected override ErrorMapping ErrorMapping => HyperLiquidErrors.Errors;

        internal new HyperLiquidSocketOptions ClientOptions => (HyperLiquidSocketOptions)base.ClientOptions;
        #endregion

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientApi(ILogger logger, HyperLiquidSocketOptions options, SocketApiOptions apiOptions) :
            base(logger, options.Environment.SocketClientAddress!, options, apiOptions)
        {
            RateLimiter = HyperLiquidExchange.RateLimiter.HyperLiquidSocket;

            _restClient = new HyperLiquidRestClient(x =>
            {
                x.Environment = options.Environment;
                x.Proxy = options.Proxy;
            });

            RegisterPeriodicQuery(
                "Ping",
                TimeSpan.FromSeconds(30),
                x => new HyperLiquidPingQuery(),
                (connection, result) =>
                {
                    if (result.Error?.ErrorType == ErrorType.Timeout)
                    {
                        // Ping timeout, reconnect
                        _logger.LogWarning("[Sckt {SocketId}] Ping response timeout, reconnecting", connection.SocketId);
                        _ = connection.TriggerReconnectAsync();
                    }
                });
        }
        #endregion

        /// <inheritdoc />
        protected override IByteMessageAccessor CreateAccessor(WebSocketMessageType type) => new SystemTextJsonByteMessageAccessor(HyperLiquidExchange._serializerContext);
        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(HyperLiquidExchange._serializerContext);
        /// <inheritdoc />
        public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType) => new HyperLiquidSocketMessageHandler();

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new HyperLiquidAuthenticationProvider(credentials);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToPriceUpdatesAsync(Action<DataEvent<Dictionary<string, decimal>>> onMessage, CancellationToken ct = default)
        {
            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_restClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidMidsUpdate>>((receiveTime, originalData, invocations, data) =>
            {
                var mappingResult = HyperLiquidUtils.GetSymbolNameFromExchangeName(ClientOptions.Environment.Name, data.Data.Mids.Keys);
                var dictData = data.Data.Mids.ToDictionary(x =>
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(x.Key))
                        return mappingResult.TryGetValue(x.Key, out var name) ? name : x.Key;

                    return x.Key;
                }, x => x.Value);

                onMessage(
                    new DataEvent<Dictionary<string, decimal>>(dictData, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                    );
            });


            var subscription = new HyperLiquidSubscription<HyperLiquidMidsUpdate>(_logger, this, "allMids", "allMids", null, internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<HyperLiquidKline>> onMessage, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidKline>>((receiveTime, originalData, invocation, data) =>
            {
                data.Data.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidKline>(data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                    );
            });

            var intervalStr = EnumConverter.GetString(interval);
            var subscription = new HyperLiquidSubscription<HyperLiquidKline>(_logger, this, "candle", $"candle-{coin}-{intervalStr}", new Dictionary<string, object>
            {
                { "coin", coin },
                { "interval", intervalStr }
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidOrderBook>> onMessage, int? nSigFigs = null, int? mantissa = null, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidOrderBook>>((receiveTime, originalData, invocation, data) =>
            {
                data.Data.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidOrderBook>(data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Timestamp)
                    );
            });

            var parameters = new Dictionary<string, object>
            {
                { "coin", coin }
            };
            
            parameters.AddOptionalParameter("nSigFigs", nSigFigs);
            parameters.AddOptionalParameter("mantissa", mantissa);
            
            var subscription = new HyperLiquidSubscription<HyperLiquidOrderBook>(_logger, this, "l2Book", "l2Book-" + coin, parameters, internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTrade[]>> onMessage, CancellationToken ct = default)
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

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTrade[]>>((receiveTime, originalData, invocation, data) =>
            {
                foreach (var trade in data.Data)
                    trade.Symbol = symbol;

                onMessage(
                    new DataEvent<HyperLiquidTrade[]>(data.Data, receiveTime, originalData)
                        .WithUpdateType(invocation == 1 ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithSymbol(symbol)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Max(x => x.Timestamp))
                    );
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidTrade[]>(_logger, this, "trades", "trades-" + coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? address, Action<DataEvent<HyperLiquidOrderStatus[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_restClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidOrderStatus[]>>((receiveTime, originalData, invocation, data) =>
            {
                foreach (var order in data.Data)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.Order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(ClientOptions.Environment.Name, order.Order.ExchangeSymbol);
                        if (symbolName == null)
                            continue;

                        order.Order.Symbol = symbolName.Data;
                        order.Order.SymbolType = SymbolType.Spot;
                    }
                    else
                    {
                        order.Order.Symbol = order.Order.ExchangeSymbol;
                        order.Order.SymbolType = SymbolType.Futures;
                    }
                }

                onMessage(
                    new DataEvent<HyperLiquidOrderStatus[]>(data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.Max(x => x.Timestamp))
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidOrderStatus[]>(_logger, this, "orderUpdates", "orderUpdates", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserLedgerUpdatesAsync(string? address, Action<DataEvent<HyperLiquidAccountLedger>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidLedgerUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidAccountLedger>(data.Data.Ledger, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidLedgerUpdate>(_logger, this, "userNonFundingLedgerUpdates", "userNonFundingLedgerUpdates", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidUserUpdate>(data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.ServerTime)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserUpdate>(_logger, this, "webData2", "webData2", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserTrade[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_restClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserTradeUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                foreach (var order in data.Data.Trades)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(ClientOptions.Environment.Name, order.ExchangeSymbol);
                        if (symbolName == null)
                            continue;

                        order.Symbol = symbolName.Data;
                        order.SymbolType = SymbolType.Spot;
                    }
                    else
                    {
                        order.Symbol = order.ExchangeSymbol;
                        order.SymbolType = SymbolType.Futures;
                    }
                }

                onMessage(
                    new DataEvent<HyperLiquidUserTrade[]>(data.Data.Trades, receiveTime, originalData)
                        .WithStreamId(data.Channel)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithDataTimestamp(data.Data.Trades.Any() ? data.Data.Trades.Max(x => x.Timestamp) : null)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserTradeUpdate>(_logger, this, "userFills", "userFills", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserEventUpdatesAsync(
            string? address,
            Action<DataEvent<HyperLiquidUserTrade[]>>? onTradeUpdate = null,
            Action<DataEvent<HyperLiquidUserFunding>>? onFundingUpdate = null,
            Action<DataEvent<HyperLiquidLiquidationUpdate>>? onLiquidationUpdate = null,
            Action<DataEvent<HyperLiquidNonUserCancelation[]>>? onNonUserCancelation = null,
            CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserEventUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                if (data.Data.Trades?.Any() == true)
                {
                    onTradeUpdate?.Invoke(
                        new DataEvent<HyperLiquidUserTrade[]>(data.Data.Trades!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }

                if (data.Data.Funding != null)
                {
                    onFundingUpdate?.Invoke(
                        new DataEvent<HyperLiquidUserFunding>(data.Data.Funding!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }

                if (data.Data.Liquidation != null)
                {
                    onLiquidationUpdate?.Invoke(
                        new DataEvent<HyperLiquidLiquidationUpdate>(data.Data.Liquidation!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }

                if (data.Data.NonUserCancelations?.Any() == true)
                {
                    onNonUserCancelation?.Invoke(
                        new DataEvent<HyperLiquidNonUserCancelation[]>(data.Data.NonUserCancelations!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserEventUpdate>(_logger, this, "userEvents", "userEvents", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTwapTradeUpdatesAsync(string? address, Action<DataEvent<HyperLiquidTwapStatus[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_restClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTwapTradeUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                foreach (var order in data.Data.Trades)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(ClientOptions.Environment.Name, order.ExchangeSymbol);
                        if (symbolName == null)
                            continue;

                        order.Symbol = symbolName.Data;
                        order.SymbolType = SymbolType.Spot;
                    }
                    else
                    {
                        order.Symbol = order.ExchangeSymbol;
                        order.SymbolType = SymbolType.Futures;
                    }
                }

                onMessage(
                    new DataEvent<HyperLiquidTwapStatus[]>(data.Data.Trades, receiveTime, originalData)
                        .WithStreamId(data.Channel)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithDataTimestamp(data.Data.Trades.Any() ? data.Data.Trades.Max(x => x.Timestamp) : null)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidTwapTradeUpdate>(_logger, this, "userTwapSliceFills", "userTwapSliceFills", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTwapOrderUpdatesAsync(string? address, Action<DataEvent<HyperLiquidTwapOrderStatus[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_restClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTwapOrderUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                foreach (var order in data.Data.History)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.TwapInfo.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(ClientOptions.Environment.Name, order.TwapInfo.ExchangeSymbol);
                        if (symbolName == null)
                            continue;

                        order.TwapInfo.Symbol = symbolName.Data;
                        order.TwapInfo.SymbolType = SymbolType.Spot;
                    }
                    else
                    {
                        order.TwapInfo.Symbol = order.TwapInfo.ExchangeSymbol;
                        order.TwapInfo.SymbolType = SymbolType.Futures;
                    }
                }

                onMessage(
                    new DataEvent<HyperLiquidTwapOrderStatus[]>(data.Data.History, receiveTime, originalData)
                        .WithStreamId(data.Channel)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithDataTimestamp(data.Data.History.Any() ? data.Data.History.Max(x => x.Timestamp) : null)
                    );
            });

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidTwapOrderUpdate>(_logger, this, "userTwapHistory", "userTwapHistory", new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override string? GetListenerIdentifier(IMessageAccessor message)
        {
            var channel = message.GetValue<string>(_channelPath);
            if (channel == "subscriptionResponse")
            {
                var type = message.GetValue<string>(_subscriptionTopicPath);
                var coin = message.GetValue<string?>(_subscriptionCoinPath);
                var interval = message.GetValue<string?>(_subscriptionIntervalPath);
                var user = message.GetValue<string?>(_subscriptionUserPath);
                var id = channel + "-" + type;
                if (coin != null)
                    id += "-" + coin;
                if (interval != null)
                    id += "-" + interval;
                if (user != null)
                    id += "-" + user;

                return id;
            }

            if (channel == "error")
            {
                var errorMessage = message.GetValue<string?>(_dataPath);
                if (errorMessage == null)
                    return null;

                if (errorMessage.StartsWith("Invalid subscription")
                    || errorMessage.StartsWith("Already subscribed")
                    || errorMessage.StartsWith("Already unsubscribed"))
                {
                    // error message format: "Invalid subscription {\"type\":\"candle\",\"interval\":\"1d\",\"coin\":\"TST2\"}"
                    var json = errorMessage.Replace("Invalid subscription ", "")
                                           .Replace("Already subscribed: ", "")
                                           .Replace("Already unsubscribed: ", "");
                    JsonDocument jsonDoc;
                    try
                    {
                        jsonDoc = JsonDocument.Parse(json);
                    }
                    catch (Exception)
                    {
                        return channel;
                    }

                    var type = jsonDoc.RootElement.GetProperty("type").GetString();
                    var coin = jsonDoc.RootElement.TryGetProperty("coin", out var coinProp) ? coinProp.GetString() : null;
                    var interval = jsonDoc.RootElement.TryGetProperty("interval", out var intervalProp) ? intervalProp.GetString() : null;
                    var user = jsonDoc.RootElement.TryGetProperty("user", out var userProp) ? userProp.GetString() : null;
                    var id = "error-" + type;
                    if (coin != null)
                        id += "-" + coin;
                    if (interval != null)
                        id += "-" + interval;
                    if (user != null)
                        id += "-" + user;

                    return id;
                }
            }
            
            if (channel == "user")
                return "userEvents";

            if (channel == "trades")
                return channel + "-" + message.GetValue<string>(_itemSymbolPath);

            if (channel == "l2Book" || channel == "activeSpotAssetCtx" || channel == "activeAssetCtx" || channel == "activeAssetData")
                return channel + "-" + message.GetValue<string>(_bookSymbolPath);

            if (channel == "candle")
                return channel + "-" + message.GetValue<string>(_symbolPath) + "-" + message.GetValue<string>(_klineIntervalPath);

            var symbol = message.GetValue<string>(_symbolPath);
            if (symbol != null)
                return channel + "-" + symbol;

            return channel;
        }

        private void ValidateAddress(string? address)
        {
            if (address != null && (!address.StartsWith("0x") || address.Length != 42))
                throw new ArgumentException("Address should be in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000");
        }

        /// <inheritdoc />
        protected override Task<Query?> GetAuthenticationRequestAsync(SocketConnection connection) => Task.FromResult<Query?>(null);

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => HyperLiquidExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverDate);
    }
}
