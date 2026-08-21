using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    internal partial class HyperLiquidSocketClientFuturesApi : IHyperLiquidSocketClientFuturesApiShared
    {
        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidFutures";

        public TradingMode[] SupportedTradingModes => new[] { TradingMode.PerpetualLinear };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();
        public SharedClientInfo Discover() => SharedUtils.GetClientInfo(HyperLiquidExchange.Metadata, this);

        #region Ticker client
        SubscribeTickerOptions ITickerSocketClient.SubscribeTickerOptions { get; } = new SubscribeTickerOptions(_exchangeName);
        async Task<WebSocketResult<UpdateSubscription>> ITickerSocketClient.SubscribeToTickerUpdatesAsync(SubscribeTickerRequest request, Action<DataEvent<SharedSpotTicker>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.SubscribeToSymbolUpdatesAsync(symbol, update =>
            handler(update.ToType(
                new SharedSpotTicker(
                    ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, update.Data.Symbol),
                    update.Data.Symbol!,
                    update.Data.MidPrice, 
                    null,
                    null, 
                    new SharedOrderQuantity(null, update.Data.NotionalVolume), 
                    update.Data.MidPrice == null ? null : Math.Round((update.Data.MidPrice.Value / update.Data.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
            {
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
                    new SharedTrade(request.Symbol, symbol, new SharedOrderQuantity(x.Quantity), x.Price, x.Timestamp)
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

                    handler(update.ToType(
                        new SharedKline(
                            request.Symbol, 
                            symbol, 
                            update.Data.OpenTime, 
                            update.Data.ClosePrice,
                            update.Data.HighPrice, 
                            update.Data.LowPrice,
                            update.Data.OpenPrice,
                            new SharedOrderQuantity(update.Data.Volume))));
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
            var result = await ExchangeData.SubscribeToOrderBookUpdatesAsync(symbol, update => handler(
                update.ToType(
                    new SharedOrderBook(SharedQuantityType.BaseAsset, update.Data.Levels.Asks, update.Data.Levels.Bids))), ct: ct).ConfigureAwait(false);

            return result;
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

                    var futuresOrders = update.Data.Where(x => x.SymbolType == Enums.SymbolType.Futures);
                    if (!futuresOrders.Any())
                        return;

                    handler(update.ToType<SharedUserTrade[]>(futuresOrders.Select(x =>
                        new SharedUserTrade(
                            ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Symbol),
                            x.Symbol!,
                            x.OrderId.ToString(),
                            x.TradeId.ToString(),
                            x.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            new SharedOrderQuantity(x.Quantity),
                            x.Price,
                            x.Timestamp)
                        {
                            Fee = x.Fee,
                            FeeAsset = x.FeeToken,
                            Role = x.Crossed ? SharedRole.Taker : SharedRole.Maker,
                        }
                    ).ToArray()));
                },
                ct: ct).ConfigureAwait(false);

            return result;
        }
        #endregion

        #region Futures Order client

        SubscribeFuturesOrderOptions IFuturesOrderSocketClient.SubscribeFuturesOrderOptions { get; } = new SubscribeFuturesOrderOptions(_exchangeName, true);
        async Task<WebSocketResult<UpdateSubscription>> IFuturesOrderSocketClient.SubscribeToFuturesOrderUpdatesAsync(SubscribeFuturesOrderRequest request, Action<DataEvent<SharedFuturesOrder[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeFuturesOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await Trading.SubscribeToOrderUpdatesAsync(null,
                update =>
                {
                    if (update.UpdateType == SocketUpdateType.Snapshot)
                        return;

                    var futuresOrders = update.Data.Where(x => x.Order.SymbolType == Enums.SymbolType.Futures);
                    if (!futuresOrders.Any())
                        return;

                    handler(update.ToType<SharedFuturesOrder[]>(futuresOrders.Select(x =>
                        new SharedFuturesOrder(
                            ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Order.Symbol!),
                            x.Order.Symbol!,
                            x.Order.OrderId.ToString(),
                            x.Order.OrderType == Enums.OrderType.Limit || x.Order.OrderType == Enums.OrderType.Limit ? SharedOrderType.Limit : x.Order.OrderType == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Other,
                            x.Order.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                            ParseOrderStatus(x.Status),
                            x.Order.Timestamp)
                        {
                            OrderPrice = x.Order.Price,
                            OrderQuantity = new SharedOrderQuantity(x.Order.Quantity, contractQuantity: x.Order.Quantity),
                            QuantityFilled = new SharedOrderQuantity(x.Order.Quantity - x.Order.QuantityRemaining, contractQuantity: x.Order.Quantity - x.Order.QuantityRemaining),
                            UpdateTime = x.Timestamp,
                            ReduceOnly = x.Order.ReduceOnly,
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

        #region Balance client
        SubscribeBalanceOptions IBalanceSocketClient.SubscribeBalanceOptions { get; } = new SubscribeBalanceOptions(_exchangeName, false);
        async Task<WebSocketResult<UpdateSubscription>> IBalanceSocketClient.SubscribeToBalanceUpdatesAsync(SubscribeBalancesRequest request, Action<DataEvent<SharedBalance[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribeBalanceOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await Account.SubscribeToUserUpdatesAsync(
                null,
                update => handler(update.ToType<SharedBalance[]>([
                    new SharedBalance(
                        SupportedTradingModes,
                        "USDC",
                        update.Data.FuturesInfo.MarginSummary.AccountValue - update.Data.FuturesInfo.MarginSummary.TotalMarginUsed,
                        update.Data.FuturesInfo.MarginSummary.AccountValue
                        )])),
                ct: ct).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region Position client
        SubscribePositionOptions IPositionSocketClient.SubscribePositionOptions { get; } = new SubscribePositionOptions(_exchangeName, false);
        async Task<WebSocketResult<UpdateSubscription>> IPositionSocketClient.SubscribeToPositionUpdatesAsync(SubscribePositionRequest request, Action<DataEvent<SharedPosition[]>> handler, CancellationToken ct)
        {
            var validationError = SharedClient.SubscribePositionOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await Account.SubscribeToUserUpdatesAsync(
                null,
                update => handler(update.ToType<SharedPosition[]>(update.Data.FuturesInfo.Positions.Select(x => 
                new SharedPosition(
                    ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Position.Symbol),
                    x.Position.Symbol,
                    new SharedOrderQuantity(Math.Abs(x.Position.PositionQuantity ?? 0)),
                    update.DataTime ?? update.ReceiveTime)
                {
                    AverageOpenPrice = x.Position.AverageEntryPrice,
                    Leverage = x.Position.Leverage!.Value,
                    LiquidationPrice = x.Position.LiquidationPrice,
                    PositionMode = SharedPositionMode.OneWay,
                    PositionSide = x.Position.PositionQuantity < 0 ? SharedPositionSide.Short : SharedPositionSide.Long,
                    UnrealizedPnl = x.Position.UnrealizedPnl
                }).ToArray())),
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
                    ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, update.Data.Symbol),
                    update.Data.Symbol,
                    update.Data.BestAsk.Price,
                    new SharedOrderQuantity(update.Data.BestAsk.Quantity),
                    update.Data.BestBid.Price,
                    new SharedOrderQuantity(update.Data.BestBid.Quantity)))), ct).ConfigureAwait(false);

            return result;
        }

        #endregion

        #region Futures Order Client

        SharedFeeDeductionType IFuturesOrderManagementSocketClient.FuturesFeeDeductionType => SharedFeeDeductionType.AddToCost;
        SharedFeeAssetType IFuturesOrderManagementSocketClient.FuturesFeeAssetType => SharedFeeAssetType.QuoteAsset;

        SharedOrderType[] IFuturesOrderManagementSocketClient.FuturesSupportedOrderTypes { get; } = new[] { SharedOrderType.Limit, SharedOrderType.Market };
        SharedTimeInForce[] IFuturesOrderManagementSocketClient.FuturesSupportedTimeInForce { get; } = new[] { SharedTimeInForce.GoodTillCanceled, SharedTimeInForce.ImmediateOrCancel };
        SharedQuantitySupport IFuturesOrderManagementSocketClient.FuturesSupportedOrderQuantity { get; } = new SharedQuantitySupport(
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset);

        string IFuturesOrderManagementSocketClient.GenerateClientOrderId() => ExchangeHelpers.RandomHexString(16)!.ToLowerInvariant();

        PlaceFuturesOrderSocketOptions IFuturesOrderManagementSocketClient.PlaceFuturesOrderOptions { get; } = new PlaceFuturesOrderSocketOptions(_exchangeName, false)
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(PlaceSpotOrderRequest.Price), typeof(decimal), "Price for the order. For market orders this should be the current symbol price", 21.5m)
            },
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("vaultAddress", typeof(string), "Vault address to place the order on behalf of", "0x123...")
            }
        };
        async Task<QueryResult<SharedId>> IFuturesOrderManagementSocketClient.PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.PlaceFuturesOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return QueryResult.Fail<SharedId>(Exchange, validationError);

            var result = await Trading.PlaceOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                request.OrderType == SharedOrderType.Limit || request.OrderType == SharedOrderType.LimitMaker ? Enums.OrderType.Limit : Enums.OrderType.Market,
                quantity: request.Quantity?.QuantityInBaseAsset ?? request.Quantity?.QuantityInContracts ?? 0,
                price: request.Price!.Value,
                reduceOnly: request.ReduceOnly,
                timeInForce: GetTimeInForce(request.TimeInForce, request.OrderType),
                clientOrderId: request.ClientOrderId,
                vaultAddress: request.GetParamValue<string?>(Exchange, "vaultAddress"),
                ct: ct).ConfigureAwait(false);

            if (!result.Success)
                return QueryResult.Fail<SharedId>(result);

            return QueryResult.Ok(result, new SharedId(result.Data.OrderId.ToString()));

        }

        CancelFuturesOrderSocketOptions IFuturesOrderManagementSocketClient.CancelFuturesOrderOptions { get; } = new CancelFuturesOrderSocketOptions(_exchangeName, true)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("vaultAddress", typeof(string), "Vault address to cancel the order on behalf of", "0x123...")
            }
        };
        async Task<QueryResult<SharedId>> IFuturesOrderManagementSocketClient.CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.CancelFuturesOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return QueryResult.Fail<SharedId>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId))
                return QueryResult.Fail<SharedId>(Exchange, ArgumentError.Invalid(nameof(CancelOrderRequest.OrderId), "Invalid order id"));

            var order = await Trading.CancelOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                orderId,
                vaultAddress: request.GetParamValue<string?>(Exchange, "vaultAddress"),
                ct: ct).ConfigureAwait(false);
            if (!order.Success)
                return QueryResult.Fail<SharedId>(order);

            return QueryResult.Ok(order, new SharedId(request.OrderId));

        }

        private Enums.TimeInForce? GetTimeInForce(SharedTimeInForce? tif, SharedOrderType type)
        {
            if (tif == SharedTimeInForce.ImmediateOrCancel) return Enums.TimeInForce.ImmediateOrCancel;
            if (tif == SharedTimeInForce.GoodTillCanceled) return Enums.TimeInForce.GoodTillCanceled;
            if (type == SharedOrderType.LimitMaker) return Enums.TimeInForce.PostOnly;

            return null;
        }
        #endregion
    }
}
