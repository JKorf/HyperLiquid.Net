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
    internal partial class HyperLiquidSocketClientFuturesSharedApi
    {

        #region Subscribe Futures Orders

        async Task<WebSocketResult<UpdateSubscription>> IFuturesOrderSocketClient.SubscribeToFuturesOrderUpdatesAsync(SubscribeFuturesOrderRequest request, Action<DataEvent<SharedFuturesOrder[]>> handler, CancellationToken ct)
            => await SubscribeToFuturesOrderUpdatesAsync(request, x => handler(x.ToType<SharedFuturesOrder[]>(x.Data)), ct).ConfigureAwait(false);

        public SubscribeFuturesOrderOptions SubscribeFuturesOrderOptions { get; } = new SubscribeFuturesOrderOptions(_exchangeName, true);
        public async Task<WebSocketResult<UpdateSubscription>> SubscribeToFuturesOrderUpdatesAsync(SubscribeFuturesOrderRequest request, Action<DataEvent<SharedFuturesOrderUpdate[]>> handler, CancellationToken ct)
        {
            var validationError = SubscribeFuturesOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await _api.Trading.SubscribeToOrderUpdatesAsync(null,
                update =>
                {
                    if (update.UpdateType == SocketUpdateType.Snapshot)
                        return;

                    var futuresOrders = update.Data.Where(x => x.Order.SymbolType == Enums.SymbolType.Futures);
                    if (!futuresOrders.Any())
                        return;

                    handler(update.ToType<SharedFuturesOrderUpdate[]>(futuresOrders.Select(x =>
                        new SharedFuturesOrderUpdate(
                            ExchangeSymbolCache.ParseSymbol(_topicId, _api.EnvironmentName, null, x.Order.Symbol!),
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

        #endregion

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

        public SharedFeeDeductionType FuturesFeeDeductionType => SharedFeeDeductionType.AddToCost;
        public SharedFeeAssetType FuturesFeeAssetType => SharedFeeAssetType.QuoteAsset;

        public SharedOrderType[] FuturesSupportedOrderTypes { get; } = new[] { SharedOrderType.Limit, SharedOrderType.Market };
        public SharedTimeInForce[] FuturesSupportedTimeInForce { get; } = new[] { SharedTimeInForce.GoodTillCanceled, SharedTimeInForce.ImmediateOrCancel };
        public SharedQuantitySupport FuturesSupportedOrderQuantity { get; } = new SharedQuantitySupport(
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset);

        public string GenerateClientOrderId() => ExchangeHelpers.RandomHexString(16)!.ToLowerInvariant();
        #region Place Futures Order

        PlaceFuturesOrderOptions IPlaceFuturesOrder.PlaceFuturesOrderOptions => PlaceFuturesOrderOptions;
        async Task<IExchangeCallResult<SharedId>> IPlaceFuturesOrder.PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct)
            => await PlaceFuturesOrderAsync(request, ct).ConfigureAwait(false);

        public PlaceFuturesOrderSocketOptions PlaceFuturesOrderOptions { get; } = new PlaceFuturesOrderSocketOptions(_exchangeName, false)
        {
            ParameterRuleOverrides = [
                RequestParameterRuleOverride<PlaceSpotOrderRequest>.Required(x => x.Price)
            ],
            ExchangeParameterRules = [
                ExchangeParameterRule.Optional("vaultAddress", "Vault address to place the order on behalf of", "0x123...")
            ]
        };
        public async Task<QueryResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct)
        {
            var validationError = PlaceFuturesOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return QueryResult.Fail<SharedId>(Exchange, validationError);

            var result = await _api.Trading.PlaceOrderAsync(
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

        #endregion
        #region Cancel Futures Order

        CancelFuturesOrderOptions ICancelFuturesOrder.CancelFuturesOrderOptions => CancelFuturesOrderOptions;
        async Task<IExchangeCallResult<SharedId>> ICancelFuturesOrder.CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct)
            => await CancelFuturesOrderAsync(request, ct).ConfigureAwait(false);

        public CancelFuturesOrderSocketOptions CancelFuturesOrderOptions { get; } = new CancelFuturesOrderSocketOptions(_exchangeName, true)
        {
            ExchangeParameterRules = [
                ExchangeParameterRule.Optional("vaultAddress", "Vault address to cancel the order on behalf of", "0x123...")
            ]
        };
        public async Task<QueryResult<SharedId>> CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = CancelFuturesOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return QueryResult.Fail<SharedId>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId))
                return QueryResult.Fail<SharedId>(Exchange, ArgumentError.Invalid(nameof(CancelOrderRequest.OrderId), "Invalid order id"));

            var order = await _api.Trading.CancelOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                orderId,
                vaultAddress: request.GetParamValue<string?>(Exchange, "vaultAddress"),
                ct: ct).ConfigureAwait(false);
            if (!order.Success)
                return QueryResult.Fail<SharedId>(order);

            return QueryResult.Ok(order, new SharedId(request.OrderId));

        }

        #endregion

        private Enums.TimeInForce? GetTimeInForce(SharedTimeInForce? tif, SharedOrderType type)
        {
            if (tif == SharedTimeInForce.ImmediateOrCancel) return Enums.TimeInForce.ImmediateOrCancel;
            if (tif == SharedTimeInForce.GoodTillCanceled) return Enums.TimeInForce.GoodTillCanceled;
            if (type == SharedOrderType.LimitMaker) return Enums.TimeInForce.PostOnly;

            return null;
        }
    }
}
