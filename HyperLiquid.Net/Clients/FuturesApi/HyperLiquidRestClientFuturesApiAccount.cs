using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Drawing;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientFuturesApiAccount : HyperLiquidRestClientAccount, IHyperLiquidRestClientFuturesApiAccount
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly HyperLiquidRestClientFuturesApi _baseClient;

        internal HyperLiquidRestClientFuturesApiAccount(HyperLiquidRestClientFuturesApi baseClient) : base(baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Futures Account

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFuturesAccount>> GetAccountInfoAsync(string? address = null, string? dex = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "clearinghouseState" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            parameters.AddOptional("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidFuturesAccount>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Funding History

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidUserLedger<HyperLiquidUserFunding>[]>> GetFundingHistoryAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userFunding" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidUserLedger<HyperLiquidUserFunding>[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get User Symbol

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidFuturesUserSymbolUpdate>> GetUserSymbolAsync(string symbol, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "activeAssetData" },
                { "coin", symbol },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidFuturesUserSymbolUpdate>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get HIP-3 DEX Abstraction

        /// <inheritdoc />
        public async Task<WebCallResult<bool>> GetHip3DexAbstractionAsync(string? user = null, CancellationToken ct = default)
        {
            if (user == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(user), "User needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userDexAbstraction" },
                { "user", user ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<bool>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Toggle HIP-3 DEX Abstraction

        /// <inheritdoc />
        public async Task<WebCallResult> ToggleHip3DexAbstractionAsync(bool enabled, string? user = null, CancellationToken ct = default)
        {
            if (user == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(user), "User needs to be provided if API credentials not set");

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "userDexAbstraction" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "user", user ?? _baseClient.AuthenticationProvider!.ApiKey }
            };

            actionParameters.Add("enabled", enabled);
            actionParameters.AddMilliseconds("nonce", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, true);
            var result = await _baseClient.SendAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion
    }
}
