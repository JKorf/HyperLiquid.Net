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
        #region Position client
        public SubscribePositionOptions SubscribePositionOptions { get; } = new SubscribePositionOptions(_exchangeName, false);
        public async Task<WebSocketResult<UpdateSubscription>> SubscribeToPositionUpdatesAsync(SubscribePositionRequest request, Action<DataEvent<SharedPosition[]>> handler, CancellationToken ct)
        {
            var validationError = SubscribePositionOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await _api.Account.SubscribeToUserUpdatesAsync(
                null,
                update => handler(update.ToType<SharedPosition[]>(update.Data.FuturesInfo.Positions.Select(x => 
                new SharedPosition(
                    ExchangeSymbolCache.ParseSymbol(_topicId, _api.EnvironmentName, null, x.Position.Symbol),
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
    }
}
