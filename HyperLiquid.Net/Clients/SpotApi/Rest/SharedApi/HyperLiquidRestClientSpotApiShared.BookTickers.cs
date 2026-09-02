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
        #region Book Ticker client

        public GetBookTickerOptions GetBookTickerOptions { get; } = new GetBookTickerOptions(_exchangeName, false);
        public async Task<HttpResult<SharedBookTicker>> GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct)
        {
            var validationError = GetBookTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedBookTicker>(Exchange, validationError);

                    var symbol = request.Symbol!.GetSymbol(FormatSymbol);
                    var resultTicker = await _api.ExchangeData.GetOrderBookAsync(symbol, ct: ct).ConfigureAwait(false);
                    if (!resultTicker.Success)
                        return HttpResult.Fail<SharedBookTicker>(resultTicker);

                    if (resultTicker.Data == null)
                        return HttpResult.Fail<SharedBookTicker>(resultTicker, new ServerError(new ErrorInfo(ErrorType.Unknown, "No response")));

                    return HttpResult.Ok(resultTicker, new SharedBookTicker(
                        ExchangeSymbolCache.ParseSymbol(_topicId, _api.EnvironmentName, null, symbol),
                        symbol,
                        resultTicker.Data.Levels.Asks[0].Price,
                        new SharedOrderQuantity(resultTicker.Data.Levels.Asks[0].Quantity),
                        resultTicker.Data.Levels.Bids[0].Price,
                        new SharedOrderQuantity(resultTicker.Data.Levels.Bids[0].Quantity)));
                
        }

        #endregion
    }
}
