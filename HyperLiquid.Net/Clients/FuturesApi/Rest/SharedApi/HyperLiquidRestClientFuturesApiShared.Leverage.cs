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
        public SharedLeverageSettingMode LeverageSettingType => SharedLeverageSettingMode.PerSymbol;
        #region Get Leverage

        async Task<IExchangeCallResult<SharedLeverage>> IGetLeverage.GetLeverageAsync(GetLeverageRequest request, CancellationToken ct)
            => await GetLeverageAsync(request, ct).ConfigureAwait(false);

        public GetLeverageOptions GetLeverageOptions { get; } = new GetLeverageOptions(_exchangeName, true)
        {
            ExchangeParameterRules = [
                ExchangeParameterRule.Optional("dex", "DEX to retrieve leverage for", "xyz")
            ]
        };
        public async Task<HttpResult<SharedLeverage>> GetLeverageAsync(GetLeverageRequest request, CancellationToken ct)
        {
            var validationError = GetLeverageOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedLeverage>(Exchange, validationError);

            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            var result = await _api.Account.GetAccountInfoAsync(dex: dex, ct: ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<SharedLeverage>(result);

            var position = result.Data.Positions.SingleOrDefault(x => x.Position.Symbol == request.Symbol!.GetSymbol(FormatSymbol));
            if (position == null)
                return HttpResult.Fail<SharedLeverage>(result, new ServerError(new ErrorInfo(ErrorType.NoPosition, "Not found")));

            return HttpResult.Ok(result, new SharedLeverage(position.Position.Leverage!.Value));
                                
        }

        #endregion
        #region Set Leverage

        async Task<IExchangeCallResult<SharedLeverage>> ISetLeverage.SetLeverageAsync(SetLeverageRequest request, CancellationToken ct)
            => await SetLeverageAsync(request, ct).ConfigureAwait(false);

        public SetLeverageOptions SetLeverageOptions { get; } = new SetLeverageOptions(_exchangeName)
        {
            ParameterRuleOverrides = [
                RequestParameterRuleOverride<SetLeverageRequest>.Required(x => x.MarginMode)
            ]
        };
        public async Task<HttpResult<SharedLeverage>> SetLeverageAsync(SetLeverageRequest request, CancellationToken ct)
        {
            var validationError = SetLeverageOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedLeverage>(Exchange, validationError);

            var result = await _api.Trading.SetLeverageAsync(symbol: request.Symbol!.GetSymbol(FormatSymbol), (int)request.Leverage, request.MarginMode == SharedMarginMode.Isolated ? Enums.MarginType.Isolated : Enums.MarginType.Cross, ct: ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<SharedLeverage>(result);

            return HttpResult.Ok(result, new SharedLeverage(request.Leverage)
            {
                MarginMode = request.MarginMode
            });
                                
        }

        #endregion
    }
}
