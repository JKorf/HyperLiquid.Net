using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using CryptoExchange.Net.Objects;
using System.Linq;
using HyperLiquid.Net.Enums;
using CryptoExchange.Net;
using HyperLiquid.Net.Objects.Models;
using CryptoExchange.Net.Objects.Errors;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    internal partial class HyperLiquidRestClientFuturesSharedApi
    {
        #region Set Futures Tp Sl

        async Task<IExchangeCallResult<SharedId>> ISetFuturesTpSl.SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct)
            => await SetFuturesTpSlAsync(request, ct).ConfigureAwait(false);

        public SetFuturesTpSlOptions SetFuturesTpSlOptions { get; } = new SetFuturesTpSlOptions(_exchangeName, true)
        {
            RequestNotes = "API doesn't return an id for trigger orders",
            ParameterRuleOverrides = [
                RequestParameterRuleOverride<SetTpSlRequest>.Required(x => x.Quantity)
            ]
        };

        public async Task<HttpResult<SharedId>> SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct)
        {
            var validationError = SetFuturesTpSlOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

            var result = await _api.Trading.PlaceOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.PositionSide == SharedPositionSide.Long ? OrderSide.Sell : OrderSide.Buy,
                OrderType.StopMarket,
                request.Quantity!.Value,
                price: request.TriggerPrice,
                triggerPrice: request.TriggerPrice,
                tpSlType: request.TpSlSide == SharedTpSlSide.TakeProfit ? TpSlType.TakeProfit : TpSlType.StopLoss,
                tpSlGrouping: TpSlGrouping.PositionTpSl,
                reduceOnly: true,
                ct: ct).ConfigureAwait(false);

            if (!result.Success)
                return HttpResult.Fail<SharedId>(result);

            // Return
            return HttpResult.Ok(result, new SharedId(result.Data.OrderId.ToString()));
                                
        }

        #endregion
        #region Cancel Futures Tp Sl

        async Task<IExchangeCallResult<bool>> ICancelFuturesTpSl.CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct)
            => await CancelFuturesTpSlAsync(request, ct).ConfigureAwait(false);

        public CancelFuturesTpSlOptions CancelFuturesTpSlOptions { get; } = new CancelFuturesTpSlOptions(_exchangeName, true)
        {
            ParameterRuleOverrides = [
                RequestParameterRuleOverride<CancelTpSlRequest>.Required(x => x.OrderId)
            ]
        };

        public async Task<HttpResult<bool>> CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct)
        {
            var validationError = CancelFuturesTpSlOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<bool>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId) || orderId == 0)
                return HttpResult.Fail<bool>(Exchange, ArgumentError.Invalid(nameof(CancelTpSlRequest.OrderId), "Invalid order id"));

            var result = await _api.Trading.CancelOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                orderId,
                ct: ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<bool>(result);

            // Return
            return HttpResult.Ok(result, true);
                                
        }

        #endregion
    }
}
