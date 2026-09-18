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
        #region Get Balances

        async Task<IExchangeCallResult<SharedBalance[]>> IGetBalances.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
            => await GetBalancesAsync(request, ct).ConfigureAwait(false);

        public GetBalancesOptions GetBalancesOptions { get; } = new GetBalancesOptions(_exchangeName, AccountTypeFilter.Futures)
        {
            ExchangeParameterRules = [
                ExchangeParameterRule.Optional("dex", "DEX to retrieve balances for", "xyz")
            ]
        };

        public async Task<HttpResult<SharedBalance[]>> GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        {
            var validationError = GetBalancesOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedBalance[]>(Exchange, validationError);
            
            string? dex = request.GetParamValue<string>(Exchange, "dex");
            var result = await _api.Account.GetAccountInfoAsync(dex: dex, ct: ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<SharedBalance[]>(result);

            return HttpResult.Ok<SharedBalance[]>(result, [
                new SharedBalance(
                    SupportedTradingModes,
                    "USDC",
                    result.Data.MarginSummary.AccountValue - result.Data.MarginSummary.TotalMarginUsed,
                    result.Data.MarginSummary.AccountValue)
                ]);
                
        }

        #endregion
    }
}
