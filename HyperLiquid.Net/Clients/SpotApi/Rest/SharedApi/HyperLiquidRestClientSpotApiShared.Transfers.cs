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
        #region Transfer

        async Task<IExchangeCallResult<SharedId>> ITransfer.TransferAsync(TransferRequest request, CancellationToken ct)
            => await TransferAsync(request, ct).ConfigureAwait(false);

        public TransferOptions TransferOptions { get; } = new TransferOptions(_exchangeName, [
            SharedAccountType.PerpetualLinearFutures,
            SharedAccountType.PerpetualInverseFutures,
            SharedAccountType.DeliveryLinearFutures,
            SharedAccountType.DeliveryInverseFutures,
            SharedAccountType.Spot
            ])
        {
            ParameterRuleOverrides = [
                RequestParameterRuleOverride<TransferRequest>.NotSupported(x => x.FromSymbol),
                RequestParameterRuleOverride<TransferRequest>.NotSupported(x => x.ToSymbol),
                ]
        };
        public async Task<HttpResult<SharedId>> TransferAsync(TransferRequest request, CancellationToken ct)
        {
            var validationError = TransferOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

            if (!request.Asset.Equals("USDC", StringComparison.InvariantCultureIgnoreCase)
                && !request.Asset.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
            {
                return HttpResult.Fail<SharedId>(Exchange, ArgumentError.Invalid("Asset", "invalid asset, only USD is supported"));
            }

            var type = GetTransferType(request);
            if (type == null)
                return HttpResult.Fail<SharedId>(Exchange, ArgumentError.Invalid("To/From AccountType", "invalid to/from account combination"));

            // Get data
            var transfer = await _api.Account.TransferInternalAsync(
                type.Value,
                request.Quantity,
                ct: ct).ConfigureAwait(false);
            if (!transfer.Success)
                return HttpResult.Fail<SharedId>(transfer);

            return HttpResult.Ok(transfer, new SharedId(""));
                
        }

        #endregion

        private TransferDirection? GetTransferType(TransferRequest request)
        {
            if (request.FromAccountType == SharedAccountType.Spot && request.ToAccountType.IsFuturesAccount()) return TransferDirection.SpotToFutures;
            if (request.FromAccountType.IsFuturesAccount() && request.ToAccountType == SharedAccountType.Spot) return TransferDirection.FuturesToSpot;
            return null;
        }
    }
}
