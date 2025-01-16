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
using HyperLiquid.Net.Interfaces.Clients.Api;
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

namespace HyperLiquid.Net.Clients.Api
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientApi : SocketApiClient, IHyperLiquidSocketClientApi
    {
        #region fields
        private static readonly MessagePath _channelPath = MessagePath.Get().Property("channel");
        private static readonly MessagePath _subscriptionTopicPath = MessagePath.Get().Property("data").Property("subscription").Property("type");
        private static readonly MessagePath _symbolPath = MessagePath.Get().Property("data").Property("s");
        private static readonly MessagePath _itemSymbolPath = MessagePath.Get().Property("data").Index(0).Property("coin");
        private static readonly MessagePath _bookSymbolPath = MessagePath.Get().Property("data").Property("coin");
        #endregion

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientApi(ILogger logger, HyperLiquidSocketOptions options) :
            base(logger, options.Environment.SocketClientAddress!, options, options.Options)
        {
            RateLimiter = HyperLiquidExchange.RateLimiter.HyperLiquidSocket;

            RegisterPeriodicQuery(
                "Ping",
                TimeSpan.FromSeconds(30),
                x => new HyperLiquidPingQuery(),
                (connection, result) =>
                {
                    if (result.Error?.Message.Equals("Query timeout") == true)
                    {
                        // Ping timeout, reconnect
                        _logger.LogWarning("[Sckt {SocketId}] Ping response timeout, reconnecting", connection.SocketId);
                        _ = connection.TriggerReconnectAsync();
                    }
                });
        }
        #endregion

        /// <inheritdoc />
        protected override IByteMessageAccessor CreateAccessor() => new SystemTextJsonByteMessageAccessor();
        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer();

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new HyperLiquidAuthenticationProvider(credentials);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToPriceUpdatesAsync(Action<DataEvent<Dictionary<string, decimal>>> onMessage, CancellationToken ct = default)
        {
            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var subscription = new HyperLiquidSubscription<HyperLiquidMidsUpdate>(_logger, "allMids", "allMids", null, x =>
            {
                var mappingResult = HyperLiquidUtils.GetSymbolNameFromExchangeName(x.Data.Mids.Keys);
                onMessage(x.As(x.Data.Mids.ToDictionary(x =>
                {
                    if (x.Key.StartsWith("@"))
                        return mappingResult.TryGetValue(x.Key, out var name) ? name : x.Key;

                    return x.Key;
                }, x => x.Value)));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToKlineUpdatesAsync(string symbol, KlineInterval interval, Action<DataEvent<HyperLiquidKline>> onMessage, CancellationToken ct = default)
        {
            var coin = symbol;
            if (symbol.Contains("/"))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<UpdateSubscription>(spotName.Error);

                coin = spotName.Data;
            }

            var subscription = new HyperLiquidSubscription<HyperLiquidKline>(_logger, "candle", "candle-" + coin, new Dictionary<string, object>
            {
                { "coin", coin },
                { "interval", EnumConverter.GetString(interval) }
            },
            x => {
                x.Data.Symbol = coin;
                onMessage(x.WithSymbol(coin));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderBookUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidOrderBook>> onMessage, CancellationToken ct = default)
        {
            var coin = symbol;
            if (symbol.Contains("/"))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<UpdateSubscription>(spotName.Error);

                coin = spotName.Data;
            }

            var subscription = new HyperLiquidSubscription<HyperLiquidOrderBook>(_logger, "l2Book", "l2Book-" + coin, new Dictionary<string, object>
            {
                { "coin", coin }
            },
            x => {
                x.Data.Symbol = coin;
                onMessage(x.WithSymbol(coin));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTradeUpdatesAsync(string symbol, Action<DataEvent<IEnumerable<HyperLiquidTrade>>> onMessage, CancellationToken ct = default)
        {
            var coin = symbol;
            if (symbol.Contains("/"))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<UpdateSubscription>(spotName.Error);

                coin = spotName.Data;
            }

            var subscription = new HyperLiquidSubscription<IEnumerable<HyperLiquidTrade>>(_logger, "trades", "trades-" + coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            x => {
                foreach(var trade in x.Data)
                    trade.Symbol = coin;
                onMessage(x.WithSymbol(coin));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSpotSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidTicker>> onMessage, CancellationToken ct = default)
        {
            var coin = symbol;
            if (symbol.Contains("/"))
            {
                // Spot symbol
                var spotName = await HyperLiquidUtils.GetExchangeNameFromSymbolNameAsync(symbol).ConfigureAwait(false);
                if (!spotName)
                    return new WebCallResult<UpdateSubscription>(spotName.Error);

                coin = spotName.Data;
            }

            var subscription = new HyperLiquidSubscription<HyperLiquidTickerUpdate>(_logger, "activeAssetCtx", "activeSpotAssetCtx-" + coin, new Dictionary<string, object>
            {
                { "coin", coin },
            },
            x =>
            {
                onMessage(x.As(x.Data.Ticker).WithSymbol(coin));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeFuturesSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidFuturesTicker>> onMessage, CancellationToken ct = default)
        {
            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesTickerUpdate>(_logger, "activeAssetCtx", "activeAssetCtx-" + symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
            },
            x =>
            {
                onMessage(x.As(x.Data.Ticker).WithSymbol(symbol));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeFuturesUserSymbolUpdatesAsync(string? address, string symbol, Action<DataEvent<HyperLiquidFuturesUserSymbolUpdate>> onMessage, CancellationToken ct = default)
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
        public async Task<CallResult<UpdateSubscription>> SubscribeToNotificationUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidTrade>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

#warning Check update model

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<IEnumerable<HyperLiquidTrade>>(_logger, "notification", "notification-" + address, new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            onMessage, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidTrade>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

#warning Check update model

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<IEnumerable<HyperLiquidTrade>>(_logger, "webData2", "webData2-" + address, new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            onMessage, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidOrderStatus>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<IEnumerable<HyperLiquidOrderStatus>>(_logger, "orderUpdates", "orderUpdates", new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            x =>
            {
                foreach (var order in x.Data)
                {
                    if (order.Order.ExchangeSymbol.StartsWith("@"))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(order.Order.ExchangeSymbol);
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

                onMessage(x);
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidUserTrade>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserTradeUpdate>(_logger, "userFills", "userFills", new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            x =>
            {
                foreach (var order in x.Data.Trades)
                {
                    if (order.ExchangeSymbol.StartsWith("@"))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(order.ExchangeSymbol);
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

                onMessage(x.As(x.Data.Trades).WithUpdateType(x.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserFundingUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidUserFunding>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserFundingUpdate>(_logger, "userFundings", "userFundings", new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            x =>
            {
                onMessage(x.As(x.Data.Fundings).WithUpdateType(x.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTwapTradeUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidTwapStatus>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidTwapTradeUpdate>(_logger, "userTwapSliceFills", "userTwapSliceFills", new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            x =>
            {
                foreach (var order in x.Data.Trades)
                {
                    if (order.ExchangeSymbol.StartsWith("@"))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(order.ExchangeSymbol);
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

                onMessage(x.As(x.Data.Trades).WithUpdateType(x.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTwapOrderUpdatesAsync(string? address, Action<DataEvent<IEnumerable<HyperLiquidTwapOrderStatus>>> onMessage, CancellationToken ct = default)
        {
            if (address == null && AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var addressSub = address ?? AuthenticationProvider!.ApiKey;
            var subscription = new HyperLiquidSubscription<HyperLiquidTwapOrderUpdate>(_logger, "userTwapHistory", "userTwapHistory", new Dictionary<string, object>
            {
                { "user", addressSub },
            },
            x =>
            {
                foreach (var order in x.Data.History)
                {
                    if (order.TwapInfo.ExchangeSymbol.StartsWith("@"))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(order.TwapInfo.ExchangeSymbol);
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

                onMessage(x.As(x.Data.History).WithUpdateType(x.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update));
            }, false);
            return await SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override string? GetListenerIdentifier(IMessageAccessor message)
        {
            var channel = message.GetValue<string>(_channelPath);
            if (channel == "subscriptionResponse")
            {
                var type = message.GetValue<string>(_subscriptionTopicPath);
                return channel + "-" + type;
            }

            if (channel == "trades")
                return channel + "-" + message.GetValue<string>(_itemSymbolPath);

            if (channel == "l2Book" || channel == "activeSpotAssetCtx" || channel == "activeAssetCtx" || channel == "activeAssetData")
                return channel + "-" + message.GetValue<string>(_bookSymbolPath);

            var symbol = message.GetValue<string>(_symbolPath);
            if (symbol != null)
                return channel + "-" + symbol;

            return channel;
        }

        /// <inheritdoc />
        protected override Task<Query?> GetAuthenticationRequestAsync(SocketConnection connection) => Task.FromResult<Query?>(null);

        /// <inheritdoc />
        public IHyperLiquidSocketClientApiShared SharedClient => this;

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => HyperLiquidExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverDate);
    }
}
