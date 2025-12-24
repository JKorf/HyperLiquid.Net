using CryptoExchange.Net.Objects;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using HyperLiquid.Net.Objects.Models;
using System;
using HyperLiquid.Net.Utils;
using HyperLiquid.Net.Enums;
using System.Globalization;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Clients.BaseApi;
using CryptoExchange.Net.Converters.SystemTextJson;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientSpotApiAccount : HyperLiquidRestClientAccount, IHyperLiquidRestClientSpotApiAccount
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly HyperLiquidRestClientSpotApi _baseClient;

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

            var parameters = new ParameterCollection()
            {
                { "type", "spotClearinghouseState" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<HyperLiquidBalances>(request, parameters, ct).ConfigureAwait(false);
            return result.As<HyperLiquidBalance[]>(result.Data?.Balances);
        }

        #endregion

        #region Get Account Ledger

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidAccountLedger>> GetAccountLedgerAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userNonFundingLedgerUpdates" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidAccountLedger> (request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Rate Limits

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidRateLimit>> GetRateLimitsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userRateLimit" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidRateLimit>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Approved Builder Fee

        /// <inheritdoc />
        public async Task<WebCallResult<int>> GetApprovedBuilderFeeAsync(string? builderAddress = null, string? address = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "maxBuilderFee" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey },
                { "builder", builderAddress ?? _baseClient.ClientOptions.BuilderAddress }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<int>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Transfer USD

        /// <inheritdoc />
        public async Task<WebCallResult> TransferUsdAsync(string destinationAddress, decimal quantity, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "usdSend" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress }
            };
            actionParameters.AddString("amount", quantity);
            actionParameters.AddMilliseconds("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
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

        #region Withdraw

        /// <inheritdoc />
        public async Task<WebCallResult> WithdrawAsync(
            string destinationAddress,
            decimal quantity,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "withdraw3" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress },
            };
            actionParameters.AddString("amount", quantity);
            actionParameters.AddMilliseconds("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Transfer Internal

        /// <inheritdoc />
        public async Task<WebCallResult> TransferInternalAsync(
            TransferDirection direction,
            decimal quantity,
            string? subAccount = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "usdClassTransfer" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
            };
            var quantityField = quantity.ToString(CultureInfo.InvariantCulture);
            if (subAccount != null)
                quantityField += " subaccount:" + subAccount;
            actionParameters.Add("amount", quantityField);
            actionParameters.Add("toPerp", direction == TransferDirection.SpotToFutures);
            actionParameters.AddMilliseconds("nonce", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Deposit Into Staking

        /// <inheritdoc />
        public async Task<WebCallResult> DepositIntoStakingAsync(long wei, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "cDeposit" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "wei", wei }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Withdraw From Staking

        /// <inheritdoc />
        public async Task<WebCallResult> WithdrawFromStakingAsync(long wei, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "cWithdraw" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "wei", wei }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Delegate Or Undelegate Stake

        /// <inheritdoc />
        public async Task<WebCallResult> DelegateOrUndelegateStakeFromValidatorAsync(DelegateDirection direction, string validator, long wei, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "tokenDelegate" },
                { "validator", validator },
                { "isUndelegate", direction == DelegateDirection.Undelegate },
                { "wei", wei }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Deposit Or Withdraw From Vault

        /// <inheritdoc />
        public async Task<WebCallResult> DepositOrWithdrawFromVaultAsync(
            DepositWithdrawDirection direction,
            string vaultAddress,
            long usd,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "vaultTransfer" },
                { "vaultAddress", vaultAddress },
                { "isDeposit", direction == DepositWithdrawDirection.Deposit },
                { "usd", usd }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Approve Builder Fee

        /// <inheritdoc />
        public Task<WebCallResult> ApproveBuilderFeeAsync(CancellationToken ct = default)
            => ApproveBuilderFeeAsync(_baseClient.ClientOptions.BuilderAddress, _baseClient.ClientOptions.BuilderFeePercentage ?? 0.1m);

        /// <inheritdoc />
        public async Task<WebCallResult> ApproveBuilderFeeAsync(string builderAddress, decimal maxFeePercentage, CancellationToken ct = default)
        {
            // NOTE; order of the parameters matters
            var actionParameters = new ParameterCollection()
            {
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "maxFeeRate", $"{maxFeePercentage.ToString(CultureInfo.InvariantCulture)}%" },
                { "builder", builderAddress },
                { "nonce", DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow).Value },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "type", "approveBuilderFee" },
            };

            var parameters = new ParameterCollection()
            {
                {
                    "action", actionParameters
                }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Get Sub account list

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidSubAccount[]>> GetSubAccountsAsync(string? address = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "subAccounts" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidSubAccount[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            if (result.Data == null)
                return result.As(new HyperLiquidSubAccount[0]);

            return result;
        }

        #endregion

        #region Get User Role

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidUserRole>> GetUserRoleAsync(string? address = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "userRole" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 60, false);
            return await _baseClient.SendAsync<HyperLiquidUserRole>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

    }
}
