using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Enums;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects.Errors;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Clients.SpotApi
{
    internal partial class HyperLiquidRestClientSpotSharedApi
    {
        #region Get Spot Ticker

        async Task<ICallResult<SharedSpotTicker>> IGetSpotTicker.GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct)
            => await GetSpotTickerAsync(request, ct).ConfigureAwait(false);

        public GetSpotTickerOptions GetSpotTickerOptions { get; } = new GetSpotTickerOptions(_exchangeName);
        public async Task<HttpResult<SharedSpotTicker>> GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct)
        {
            var validationError = GetSpotTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotTicker>(Exchange, validationError);

                    var symbolName = request.Symbol!.GetSymbol(FormatSymbol);
                    var result = await _api.ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedSpotTicker>(result);

                    var symbol = result.Data.Tickers.SingleOrDefault(x => x.Symbol == symbolName);
                    if (symbol == null)
                        return HttpResult.Fail<SharedSpotTicker>(result, new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                    return HttpResult.Ok(result, 
                        new SharedSpotTicker(
                            ExchangeSymbolCache.ParseSymbol(_topicId, _api.EnvironmentName, null, symbol.Symbol!),
                            symbol.Symbol!,
                            symbol.MidPrice, 
                            null, 
                            null,
                            new SharedOrderQuantity(symbol.BaseVolume, symbol.QuoteVolume),
                            (symbol.MidPrice == null || symbol.PreviousDayPrice == 0) ? null : Math.Round((symbol.MidPrice.Value / symbol.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
                    {
                    });
                
        }

        #endregion

        #region Get All Spot Tickers

        async Task<ICallResult<SharedSpotTicker[]>> IGetAllSpotTickers.GetAllSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
            => await GetAllSpotTickersAsync(request, ct).ConfigureAwait(false);

        Task<HttpResult<SharedSpotTicker[]>> ISpotTickerRestClient.GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
            => GetAllSpotTickersAsync(request, ct);
        GetAllSpotTickersOptions ISpotTickerRestClient.GetSpotTickersOptions => GetAllSpotTickersOptions;

        public GetAllSpotTickersOptions GetAllSpotTickersOptions { get; } = new GetAllSpotTickersOptions(_exchangeName);
        public async Task<HttpResult<SharedSpotTicker[]>> GetAllSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
        {
            var validationError = GetAllSpotTickersOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotTicker[]>(Exchange, validationError);

                    var result = await _api.ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedSpotTicker[]>(result);

                    return HttpResult.Ok(result, result.Data.Tickers.Select(x => 
                        new SharedSpotTicker(
                            ExchangeSymbolCache.ParseSymbol(_topicId, _api.EnvironmentName, null, x.Symbol!),
                            x.Symbol!, 
                            x.MidPrice, 
                            null,
                            null, 
                            new SharedOrderQuantity(x.BaseVolume, x.QuoteVolume),
                            (x.MidPrice == null || x.PreviousDayPrice == 0) ? null : Math.Round((x.MidPrice.Value / x.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
                        {
                        }).ToArray());
                
        }

        #endregion
    }
}
