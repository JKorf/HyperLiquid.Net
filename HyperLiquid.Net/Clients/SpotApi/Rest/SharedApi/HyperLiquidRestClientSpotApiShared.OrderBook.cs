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
        #region Get Order Book

        async Task<ICallResult<SharedOrderBook>> IGetOrderBook.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
            => await GetOrderBookAsync(request, ct).ConfigureAwait(false);

        public GetOrderBookOptions GetOrderBookOptions { get; } = new GetOrderBookOptions(_exchangeName, [20], false);
        public async Task<HttpResult<SharedOrderBook>> GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        {
            var validationError = GetOrderBookOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedOrderBook>(Exchange, validationError);

                    var result = await _api.ExchangeData.GetOrderBookAsync(
                        request.Symbol!.GetSymbol(FormatSymbol),
                        ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedOrderBook>(result);

                    if (result.Data == null)
                        return HttpResult.Fail<SharedOrderBook>(result, new ServerError(ErrorInfo.Unknown with { Message = "No response" }));

                    return HttpResult.Ok(result, new SharedOrderBook(SharedQuantityType.BaseAsset, null, result.Data.Levels.Asks, result.Data.Levels.Bids));
                
        }

        #endregion
    }
}
