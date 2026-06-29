using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Utils;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal class HyperLiquidRestClientApiAccount: IHyperLiquidRestClientAccount
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        protected internal readonly HyperLiquidRestClientApi _baseClient;

        protected readonly string _chainIdTestnet = "0x66eee";
        protected readonly string _chainIdMainnet = "0xa4b1";

        internal HyperLiquidRestClientApiAccount(HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
        }

        #region Get Trading Fee

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidFeeInfo>> GetFeeInfoAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userFees" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidFeeInfo>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Send Asset

        /// <inheritdoc />
        public async Task<HttpResult> SendAssetAsync(
            string destination,
            string sourceDex,
            string destinationDex,
            string tokenName,
            string tokenId,
            decimal quantity,
            string? fromSubAccount = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "sendAsset" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
            };

            actionParameters.Add("destination", destination);
            actionParameters.Add("sourceDex", sourceDex);
            actionParameters.Add("destinationDex", destinationDex);
            actionParameters.Add("token", $"{tokenName}:{tokenId}");
            actionParameters.Add("amount", quantity);
            actionParameters.Add("fromSubAccount", fromSubAccount ?? string.Empty);
            actionParameters.Add("nonce", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Account Ledger

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidAccountLedger>> GetAccountLedgerAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userNonFundingLedgerUpdates" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.Add("startTime", startTime);
            parameters.Add("endTime", endTime);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidAccountLedger>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Rate Limits

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidRateLimit>> GetRateLimitsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userRateLimit" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<HyperLiquidRateLimit>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Approved Builder Fee

        /// <inheritdoc />
        public async Task<HttpResult<int>> GetApprovedBuilderFeeAsync(string? builderAddress = null, string? address = null, CancellationToken ct = default)
        {
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "maxBuilderFee" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key },
                { "builder", builderAddress ?? _baseClient.ClientOptions.BuilderAddress }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            return await _baseClient.SendAsync<int>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Transfer USD

        /// <inheritdoc />
        public async Task<HttpResult> TransferUsdAsync(string destinationAddress, decimal quantity, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "usdSend" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress }
            };
            actionParameters.Add("amount", quantity);
            actionParameters.Add("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Withdraw

        /// <inheritdoc />
        public async Task<HttpResult> WithdrawAsync(
            string destinationAddress,
            decimal quantity,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "withdraw3" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "destination", destinationAddress },
            };
            actionParameters.Add("amount", quantity);
            actionParameters.Add("time", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Transfer Internal

        /// <inheritdoc />
        public async Task<HttpResult> TransferInternalAsync(
            TransferDirection direction,
            decimal quantity,
            string? subAccount = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
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
            actionParameters.Add("nonce", DateTime.UtcNow);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Deposit Into Staking

        /// <inheritdoc />
        public async Task<HttpResult> DepositIntoStakingAsync(long wei, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "cDeposit" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "wei", wei }
            };
            parameters.Add("nonce", DateTime.UtcNow);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Withdraw From Staking

        /// <inheritdoc />
        public async Task<HttpResult> WithdrawFromStakingAsync(long wei, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "cWithdraw" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "wei", wei }
            };
            parameters.Add("nonce", DateTime.UtcNow);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Delegate Or Undelegate Stake

        /// <inheritdoc />
        public async Task<HttpResult> DelegateOrUndelegateStakeFromValidatorAsync(DelegateDirection direction, string validator, long wei, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "tokenDelegate" },
                { "validator", validator },
                { "isUndelegate", direction == DelegateDirection.Undelegate },
                { "wei", wei }
            };
            parameters.Add("nonce", DateTime.UtcNow);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Deposit Or Withdraw From Vault

        /// <inheritdoc />
        public async Task<HttpResult> DepositOrWithdrawFromVaultAsync(
            DepositWithdrawDirection direction,
            string vaultAddress,
            long usd,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "vaultTransfer" },
                { "vaultAddress", vaultAddress },
                { "isDeposit", direction == DepositWithdrawDirection.Deposit },
                { "usd", usd }
            };
            parameters.Add("nonce", DateTime.UtcNow);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Approve Builder Fee

        /// <inheritdoc />
        public Task<HttpResult> ApproveBuilderFeeAsync(CancellationToken ct = default)
            => ApproveBuilderFeeAsync(_baseClient.ClientOptions.BuilderAddress, _baseClient.ClientOptions.BuilderFeePercentage ?? 0.1m);

        /// <inheritdoc />
        public async Task<HttpResult> ApproveBuilderFeeAsync(string builderAddress, decimal maxFeePercentage, CancellationToken ct = default)
        {
            // NOTE; order of the parameters matters
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "maxFeeRate", $"{maxFeePercentage.ToString(CultureInfo.InvariantCulture)}%" },
                { "builder", builderAddress },
                { "nonce", DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow).Value },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "type", "approveBuilderFee" },
            };

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                {
                    "action", actionParameters
                }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAuthAsync<HyperLiquidDefault>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Sub account list

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidSubAccount[]>> GetSubAccountsAsync(string? address = null, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "subAccounts" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidSubAccount[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            if (result.Data == null)
                return HttpResult.Ok(result, new HyperLiquidSubAccount[0]);

            return result;
        }

        #endregion

        #region Get User Role

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidUserRole>> GetUserRoleAsync(string? address = null, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userRole" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 60, false);
            return await _baseClient.SendAsync<HyperLiquidUserRole>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Extra Agents

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidUserAgent[]>> GetExtraAgentsAsync(string? address = null, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "extraAgents" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, false);
            return await _baseClient.SendAsync<HyperLiquidUserAgent[]>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Staking Delegations
        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidStakingDelegation[]>> GetStakingDelegationsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "delegations" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidStakingDelegation[]>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion

        #region Get Staking Summary
        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidStakingSummary>> GetStakingSummaryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "delegatorSummary" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidStakingSummary>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion

        #region Get Staking History
        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidStakingHistory[]>> GetStakingHistoryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "delegatorHistory" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidStakingHistory[]>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion

        #region Get Staking Rewards
        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidStakingReward[]>> GetStakingRewardsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "delegatorRewards" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<HyperLiquidStakingReward[]>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion

        #region Get User Abstraction State
        /// <inheritdoc />
        public async Task<HttpResult<UserAbstractionState>> GetUserAbstractionStateAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userAbstraction" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            return await _baseClient.SendAsync<UserAbstractionState>(request, parameters, ct).ConfigureAwait(false);
        }
        #endregion
    }
}
