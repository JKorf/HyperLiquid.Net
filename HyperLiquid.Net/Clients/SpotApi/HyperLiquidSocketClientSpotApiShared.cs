using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using System.Threading.Tasks;
using System.Threading;
using System;
using CryptoExchange.Net.Objects.Sockets;
using System.Linq;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net;

namespace HyperLiquid.Net.Clients.SpotApi
{
    internal partial class HyperLiquidSocketClientSpotApi : IHyperLiquidSocketClientSpotApiShared
    {
        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidSpot";

        public TradingMode[] SupportedTradingModes => new[] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();
        public SharedClientInfo Discover() => SharedUtils.GetClientInfo(this);

        #region Ticker client
        SubscribeTickerOptions ITickerSocketClient.SubscribeTickerOptions { get; } = new SubscribeTickerOptions(_exchangeName);
        async Task<WebSocketResult<UpdateSubscription>> ITickerSocketClient.SubscribeToTickerUpdatesAsync(SubscribeTickerRequest request, Action<DataEvent<SharedSpotTicker>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.SubscribeToSymbolUpdatesAsync(symbol, update => 
            handler(update.ToType(new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, update.Data.Symbol), update.Data.Symbol!, update.Data.MidPrice, null, null, update.Data.BaseVolume, update.Data.MidPrice == null ? null : Math.Round((update.Data.MidPrice.Value / update.Data.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
            {
                QuoteVolume = update.Data.QuoteVolume
            })), ct).ConfigureAwait(false);

            return result;
        }
        #endregion

        #region Trade client

        SubscribeTradeOptions ITradeSocketClient.SubscribeTradeOptions { get; } = new SubscribeTradeOptions(_exchangeName, false);
        async Task<WebSocketResult<UpdateSubscription>> ITradeSocketClient.SubscribeToTradeUpdatesAsync(SubscribeTradeRequest request, Action<DataEvent<SharedTrade[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeTradeOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.SubscribeToTradeUpdatesAsync(symbol, update =>
            {
                if (update.UpdateType == SocketUpdateType.Snapshot)
                    return;

                handler(update.ToType<SharedTrade[]>(update.Data.Select(x =>
                    new SharedTrade(request.Symbol, symbol, x.Quantity, x.Price, x.Timestamp)
                    {
                        Side = x.Side == Enums.OrderSide.Sell ? SharedOrderSide.Sell : SharedOrderSide.Buy,
                    }).ToArray()
                ));
            }, ct).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region Kline client
        SubscribeKlineOptions IKlineSocketClient.SubscribeKlineOptions { get; } = new SubscribeKlineOptions(_exchangeName, false,
            SharedKlineInterval.OneMinute,
            SharedKlineInterval.ThreeMinutes,
            SharedKlineInterval.FiveMinutes,
            SharedKlineInterval.FifteenMinutes,
            SharedKlineInterval.ThirtyMinutes,
            SharedKlineInterval.OneHour,
            SharedKlineInterval.TwoHours,
            SharedKlineInterval.FourHours,
            SharedKlineInterval.EightHours,
            SharedKlineInterval.TwelveHours,
            SharedKlineInterval.OneDay,
            SharedKlineInterval.OneWeek,
            SharedKlineInterval.OneMonth);
        async Task<WebSocketResult<UpdateSubscription>> IKlineSocketClient.SubscribeToKlineUpdatesAsync(SubscribeKlineRequest request, Action<DataEvent<SharedKline>> handler, CancellationToken ct)
        {
            var interval = (Enums.KlineInterval)request.Interval;
            var validationError = SharedClient.SubscribeKlineOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.SubscribeToKlineUpdatesAsync(symbol, interval, 
                update =>
                {
                    if (update.UpdateType == SocketUpdateType.Snapshot)
                        return;

                    handler(update.ToType(new SharedKline(request.Symbol, symbol, update.Data.OpenTime, update.Data.ClosePrice, update.Data.HighPrice, update.Data.LowPrice, update.Data.OpenPrice, update.Data.Volume)));
                }, ct).ConfigureAwait(false);

            return result;
        }
        #endregion

        #region Order Book client
        SubscribeOrderBookOptions IOrderBookSocketClient.SubscribeOrderBookOptions { get; } = new SubscribeOrderBookOptions(_exchangeName, false, new[] { 20 });
        async Task<WebSocketResult<UpdateSubscription>> IOrderBookSocketClient.SubscribeToOrderBookUpdatesAsync(SubscribeOrderBookRequest request, Action<DataEvent<SharedOrderBook>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeOrderBookOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.SubscribeToOrderBookUpdatesAsync(symbol, update => handler(update.ToType(new SharedOrderBook(update.Data.Levels.Asks, update.Data.Levels.Bids))), ct: ct).ConfigureAwait(false);

            return result;
        }
        #endregion

        #region Spot Order client

        SubscribeSpotOrderOptions ISpotOrderSocketClient.SubscribeSpotOrderOptions { get; } = new SubscribeSpotOrderOptions(_exchangeName, true);
        async Task<WebSocketResult<UpdateSubscription>> ISpotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync(SubscribeSpotOrderRequest request, Action<DataEvent<SharedSpotOrder[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeSpotOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await Trading.SubscribeToOrderUpdatesAsync(null,
                update =>
                {
                    if (update.UpdateType == SocketUpdateType.Snapshot)
                        return;

                    var spotOrders = update.Data.Where(x => x.Order.SymbolType == Enums.SymbolType.Spot);
                    if (!spotOrders.Any())
                        return;

                    handler(update.ToType<SharedSpotOrder[]>(spotOrders.Select(x =>
                        new SharedSpotOrder(
                            ExchangeSymbolCache.ParseSymbol(_topicId, x.Order.Symbol!),
                            x.Order.Symbol!,
                            x.Order.OrderId.ToString(),
                            x.Order.OrderType == Enums.OrderType.Limit || x.Order.OrderType == Enums.OrderType.Limit ? SharedOrderType.Limit : x.Order.OrderType == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Other,
                            x.Order.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            ParseOrderStatus(x.Status),
                            x.Order.Timestamp)
                        {
                            OrderPrice = x.Order.Price,
                            OrderQuantity = new SharedOrderQuantity(x.Order.Quantity),
                            QuantityFilled = new SharedOrderQuantity(x.Order.Quantity - x.Order.QuantityRemaining),
                            UpdateTime = x.Timestamp,
                            TriggerPrice = x.Order.TriggerPrice,
                            IsTriggerOrder = x.Order.TriggerPrice > 0,
                            ClientOrderId = x.Order.ClientOrderId
                        }
                    ).ToArray()));
                },
                ct: ct).ConfigureAwait(false);

            return result;
        }

        private SharedOrderStatus ParseOrderStatus(Enums.OrderStatus status)
        {
            if (status == Enums.OrderStatus.Open) return SharedOrderStatus.Open;
            if (status == Enums.OrderStatus.Filled) return SharedOrderStatus.Filled;
            if (status == Enums.OrderStatus.Canceled
                || status == Enums.OrderStatus.Rejected
                || status == Enums.OrderStatus.MarginCanceled
                || status == Enums.OrderStatus.RejectedInsufficientBalance
                || status == Enums.OrderStatus.RejectedIOC
                || status == Enums.OrderStatus.RejectedBadPrice
                || status == Enums.OrderStatus.RejectedInsufficientMargin
                || status == Enums.OrderStatus.RejectedSiblingFilledCanceled
                || status == Enums.OrderStatus.ReduceOnlyRejected
                || status == Enums.OrderStatus.RejectedMinValue
                || status == Enums.OrderStatus.PositionIncreaseAtOpenInterestCapRejected
                || status == Enums.OrderStatus.PositionFlipAtOpenInterestCapRejected
                || status == Enums.OrderStatus.TooAggressiveAtOpenInterestCapRejected
                || status == Enums.OrderStatus.OpenInterestIncreaseRejected
                || status == Enums.OrderStatus.OpenInterestCapCanceled
                || status == Enums.OrderStatus.VaultWithdrawalCanceled
                || status == Enums.OrderStatus.SelfTradeCanceled
                || status == Enums.OrderStatus.DelistedCanceled
                || status == Enums.OrderStatus.LiquidatedCanceled
                || status == Enums.OrderStatus.ScheduledCancel
                || status == Enums.OrderStatus.RejectedTick
                || status == Enums.OrderStatus.RejectedBadTriggerPrice
                || status == Enums.OrderStatus.RejectedNoLiquidity
                || status == Enums.OrderStatus.RejectedOracle
                || status == Enums.OrderStatus.RejectedPerpMaxPosition)
            {
                return SharedOrderStatus.Canceled;
            }

            return SharedOrderStatus.Unknown;
        }
        #endregion

        #region User Trade client

        SubscribeUserTradeOptions IUserTradeSocketClient.SubscribeUserTradeOptions { get; } = new SubscribeUserTradeOptions(_exchangeName, true);
        async Task<WebSocketResult<UpdateSubscription>> IUserTradeSocketClient.SubscribeToUserTradeUpdatesAsync(SubscribeUserTradeRequest request, Action<DataEvent<SharedUserTrade[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeUserTradeOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await Trading.SubscribeToUserTradeUpdatesAsync(null,
                update =>
                {
                    if (update.UpdateType == SocketUpdateType.Snapshot)
                        return;

                    var spotOrders = update.Data.Where(x => x.SymbolType == Enums.SymbolType.Spot);
                    if (!spotOrders.Any())
                        return;

                    handler(update.ToType<SharedUserTrade[]>(spotOrders.Select(x =>
                        new SharedUserTrade(
                            ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol),
                            x.Symbol!,
                            x.OrderId.ToString(),
                            x.TradeId.ToString(),
                            x.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            x.Quantity,
                            x.Price,
                            x.Timestamp)
                        {
                            Fee = x.Fee,
                            FeeAsset = HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.FeeToken),
                            Role = x.Crossed ? SharedRole.Taker : SharedRole.Maker,
                        }
                    ).ToArray()));
                },
                ct: ct).ConfigureAwait(false);

            return result;
        }
        #endregion

        #region Balance client
        SubscribeBalanceOptions IBalanceSocketClient.SubscribeBalanceOptions { get; } = new SubscribeBalanceOptions(_exchangeName, false);
        async Task<WebSocketResult<UpdateSubscription>> IBalanceSocketClient.SubscribeToBalanceUpdatesAsync(SubscribeBalancesRequest request, Action<DataEvent<SharedBalance[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeBalanceOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await Account.SubscribeToUserUpdatesAsync(
                null,
                update => handler(update.ToType<SharedBalance[]>(update.Data.SpotBalances.Balances.Select(x => new SharedBalance(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Asset), x.Total - x.Hold, x.Total)).ToArray())),
                ct: ct).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region Book Ticker client

        SubscribeBookTickerOptions IBookTickerSocketClient.SubscribeBookTickerOptions { get; } = new SubscribeBookTickerOptions(_exchangeName, false);
        async Task<WebSocketResult<UpdateSubscription>> IBookTickerSocketClient.SubscribeToBookTickerUpdatesAsync(SubscribeBookTickerRequest request, Action<DataEvent<SharedBookTicker>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeBookTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.SubscribeToBookTickerUpdatesAsync(symbol, update => 
            handler(update.ToType(
                new SharedBookTicker(
                    ExchangeSymbolCache.ParseSymbol(_topicId, update.Data.Symbol),
                    update.Data.Symbol,
                    update.Data.BestAsk.Price,
                    update.Data.BestAsk.Quantity,
                    update.Data.BestBid.Price,
                    update.Data.BestBid.Quantity))), ct).ConfigureAwait(false);

            return result;
        }

        #endregion
    }
}
