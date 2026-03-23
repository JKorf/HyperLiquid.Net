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
        public async Task<WebCallResult<HyperLiquidBalance[]>> GetBalancesAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "spotClearinghouseState" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<HyperLiquidBalances>(request, parameters, ct).ConfigureAwait(false);
            return result.As<HyperLiquidBalance[]>(result.Data?.Balances);
        }

        #endregion

        #region Spot Transfer

        /// <inheritdoc />
        public async Task<WebCallResult> TransferSpotAsync(
            string destinationAddress,
            string asset,
            decimal quantity,
            CancellationToken ct = default)
        {
            var assetId = await HyperLiquidUtils.GetAssetNameAndIdAsync(_baseClient.BaseClient, asset).ConfigureAwait(false);
            if (!assetId)
                return new WebCallResult(assetId.Error!);

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "spotSend" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress },
                { "token", assetId.Data }
            };
            actionParameters.AddString("amount", quantity);
            actionParameters.AddMilliseconds("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

    }
}
