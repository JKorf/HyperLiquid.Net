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
        #region Get Funding Rate History

        async Task<ICallResult<SharedFundingRate[]>> IGetFundingRateHistory.GetFundingRateHistoryAsync(GetFundingRateHistoryRequest request, PageRequest? pageToken, CancellationToken ct)
            => await GetFundingRateHistoryAsync(request, pageToken, ct).ConfigureAwait(false);

        public GetFundingRateHistoryOptions GetFundingRateHistoryOptions { get; } = new GetFundingRateHistoryOptions(_exchangeName, true, false, true, 500, false);

        public async Task<HttpResult<SharedFundingRate[]>> GetFundingRateHistoryAsync(GetFundingRateHistoryRequest request, PageRequest? pageToken, CancellationToken ct)
        {
            var validationError = GetFundingRateHistoryOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedFundingRate[]>(Exchange, validationError);

            //DateTime? fromTime = null;
            //if (pageToken is DateTimeToken token)
            //    fromTime = token.LastTime;

            // Get data
            var result = await _api.ExchangeData.GetFundingRateHistoryAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.StartTime ?? DateTime.UtcNow.AddDays(-30),
                //endTime: request.EndTime,
                ct: ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<SharedFundingRate[]>(result);

            //DateTimeToken? nextToken = null;
            //if (result.Data.Count() == (request.Limit ?? 500))
            //    nextToken = new DateTimeToken(result.Data.Max(x => x.Timestamp).AddSeconds(1));

            // Return
            return HttpResult.Ok(result, result.Data.Select(x => new SharedFundingRate(x.FundingRate, x.Timestamp)).ToArray()/*, nextToken*/);
                                
        }

        #endregion
    }
}
