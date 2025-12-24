using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal class HyperLiquidRestClientAccount: IHyperLiquidRestClientAccount
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly HyperLiquidRestClientApi _baseClient;

        protected readonly string _chainIdTestnet = "0x66eee";
        protected readonly string _chainIdMainnet = "0xa4b1";

        internal HyperLiquidRestClientAccount(HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Trading Fee

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFeeInfo>> GetFeeInfoAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userFees" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidFeeInfo>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Send Asset

        /// <inheritdoc />
        public async Task<WebCallResult> SendAssetAsync(
            string destination,
            string sourceDex,
            string destinationDex,
            string tokenName,
            string tokenId,
            decimal quantity,
            string? fromSubAccount = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "sendAsset" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
            };

            actionParameters.Add("destination", destination);
            actionParameters.Add("sourceDex", sourceDex);
            actionParameters.Add("destinationDex", destinationDex);
            actionParameters.Add("token", $"{tokenName}:{tokenId}");
            actionParameters.AddString("amount", quantity);
            actionParameters.Add("fromSubAccount", fromSubAccount ?? string.Empty);
            actionParameters.AddMilliseconds("nonce", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion
    }
}
