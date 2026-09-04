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
        #region Subscribe Balances

        public SubscribeBalanceOptions SubscribeBalanceOptions { get; } = new SubscribeBalanceOptions(_exchangeName, false);
        public async Task<WebSocketResult<UpdateSubscription>> SubscribeToBalanceUpdatesAsync(SubscribeBalancesRequest request, Action<DataEvent<SharedBalance[]>> handler, CancellationToken ct)
        {
            var validationError = SubscribeBalanceOptions.ValidateRequest(request, this);
            if (validationError != null)
                return WebSocketResult.Fail<UpdateSubscription>(_exchangeName, validationError);

            var result = await _api.Account.SubscribeToUserUpdatesAsync(
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
    }
}
