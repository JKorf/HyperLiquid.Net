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
        #region Get All Assets

        async Task<IExchangeCallResult<SharedAsset[]>> IGetAllAssets.GetAllAssetsAsync(GetAssetsRequest request, CancellationToken ct)
            => await GetAllAssetsAsync(request, ct).ConfigureAwait(false);

        Task<HttpResult<SharedAsset[]>> IAssetsRestClient.GetAssetsAsync(GetAssetsRequest request, CancellationToken ct)
            => GetAllAssetsAsync(request, ct);
        GetAllAssetsOptions IAssetsRestClient.GetAssetsOptions => GetAllAssetsOptions;

        public GetAllAssetsOptions GetAllAssetsOptions { get; } = new GetAllAssetsOptions(_exchangeName, false);

        public async Task<HttpResult<SharedAsset[]>> GetAllAssetsAsync(GetAssetsRequest request, CancellationToken ct)
        {
            var validationError = GetAllAssetsOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedAsset[]>(Exchange, validationError);

            var assets = await _api.ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
            if (!assets.Success)
                return HttpResult.Fail<SharedAsset[]>(assets);

            return HttpResult.Ok(assets, assets.Data.Assets.Select(x =>
                new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Name))
                {
                    FullName = x.FullName,
                    Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = x.AssetId
                }]
            }).ToArray());
        }

        #endregion
        #region Get Asset

        async Task<IExchangeCallResult<SharedAsset>> IGetAsset.GetAssetAsync(GetAssetRequest request, CancellationToken ct)
            => await GetAssetAsync(request, ct).ConfigureAwait(false);

        public GetAssetOptions GetAssetOptions { get; } = new GetAssetOptions(_exchangeName, false);
        public async Task<HttpResult<SharedAsset>> GetAssetAsync(GetAssetRequest request, CancellationToken ct)
        {
            var validationError = GetAssetOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedAsset>(Exchange, validationError);

                    var assets = await _api.ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!assets.Success)
                        return HttpResult.Fail<SharedAsset>(assets);

                    var asset = assets.Data.Assets.SingleOrDefault(x => x.Name.Equals(HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset), StringComparison.InvariantCultureIgnoreCase));
                    if (asset == null)
                        return HttpResult.Fail<SharedAsset>(assets, new ServerError(new ErrorInfo(ErrorType.UnknownAsset, "Asset not found")));

                    return HttpResult.Ok(assets, new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(asset.Name))
                    {
                        FullName = asset.FullName,
                        Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = asset.AssetId
                }]
                    });
                
        }

        #endregion
    }
}
