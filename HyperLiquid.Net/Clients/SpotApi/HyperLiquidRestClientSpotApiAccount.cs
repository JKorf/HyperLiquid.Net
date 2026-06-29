using CryptoExchange.Net.Objects;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using HyperLiquid.Net.Objects.Models;
using System;
using HyperLiquid.Net.Utils;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Clients.BaseApi;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientSpotApiAccount : HyperLiquidRestClientApiAccount, IHyperLiquidRestClientSpotApiAccount
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly new HyperLiquidRestClientSpotApi _baseClient;

        internal HyperLiquidRestClientSpotApiAccount(HyperLiquidRestClientSpotApi baseClient): base(baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Spot Balances

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidBalance[]>> GetBalancesAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "spotClearinghouseState" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<HyperLiquidBalances>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<HyperLiquidBalance[]>(result);

            return HttpResult.Ok(result, result.Data.Balances);
        }

        #endregion

        #region Spot Transfer

        /// <inheritdoc />
        public async Task<HttpResult> TransferSpotAsync(
            string destinationAddress,
            string asset,
            decimal quantity,
            CancellationToken ct = default)
        {
            var assetId = await HyperLiquidUtils.GetAssetNameAndIdAsync(_baseClient.BaseClient, asset).ConfigureAwait(false);
            if (!assetId.Success)
                return HttpResult.Fail(_baseClient.Exchange, assetId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "spotSend" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress },
                { "token", assetId.Data }
            };
            actionParameters.Add("amount", quantity);
            actionParameters.Add("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

    }
}
