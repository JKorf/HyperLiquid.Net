using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using HyperLiquid.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal partial class HyperLiquidSocketClientApiAccount : IHyperLiquidSocketClientApiAccount
    {
        protected internal readonly HyperLiquidSocketClientApi _baseClient;
        protected readonly ILogger _logger;

        protected readonly string _chainIdTestnet = "0x66eee";
        protected readonly string _chainIdMainnet = "0xa4b1";

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientApiAccount(ILogger logger, HyperLiquidSocketClientApi baseClient)
        {
            _logger = logger;
            _baseClient = baseClient;
        }
        #endregion

        #region Get Trading Fee

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidFeeInfo>> GetFeeInfoAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userFees" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFeeInfo>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Send Asset

        /// <inheritdoc />
        public async Task<CallResult> SendAssetAsync(
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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Get Account Ledger

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidAccountLedger>> GetAccountLedgerAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userNonFundingLedgerUpdates" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidAccountLedger>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Rate Limits

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidRateLimit>> GetRateLimitsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userRateLimit" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidRateLimit>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Approved Builder Fee

        /// <inheritdoc />
        public async Task<CallResult<int>> GetApprovedBuilderFeeAsync(string? builderAddress = null, string? address = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "maxBuilderFee" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key },
                { "builder", builderAddress ?? _baseClient.ClientOptions.BuilderAddress }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<int>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Transfer USD

        /// <inheritdoc />
        public async Task<CallResult> TransferUsdAsync(string destinationAddress, decimal quantity, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Withdraw

        /// <inheritdoc />
        public async Task<CallResult> WithdrawAsync(
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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Transfer Internal

        /// <inheritdoc />
        public async Task<CallResult> TransferInternalAsync(
            TransferDirection direction,
            decimal quantity,
            string? subAccount = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Deposit Into Staking

        /// <inheritdoc />
        public async Task<CallResult> DepositIntoStakingAsync(long wei, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "cDeposit" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "wei", wei }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Withdraw From Staking

        /// <inheritdoc />
        public async Task<CallResult> WithdrawFromStakingAsync(long wei, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "cWithdraw" },
                { "hyperliquidChain", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "Testnet" : "Mainnet" },
                { "signatureChainId", _baseClient.ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? _chainIdTestnet : _chainIdMainnet },
                { "wei", wei }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Delegate Or Undelegate Stake

        /// <inheritdoc />
        public async Task<CallResult> DelegateOrUndelegateStakeFromValidatorAsync(DelegateDirection direction, string validator, long wei, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "tokenDelegate" },
                { "validator", validator },
                { "isUndelegate", direction == DelegateDirection.Undelegate },
                { "wei", wei }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Deposit Or Withdraw From Vault

        /// <inheritdoc />
        public async Task<CallResult> DepositOrWithdrawFromVaultAsync(
            DepositWithdrawDirection direction,
            string vaultAddress,
            long usd,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "vaultTransfer" },
                { "vaultAddress", vaultAddress },
                { "isDeposit", direction == DepositWithdrawDirection.Deposit },
                { "usd", usd }
            };
            parameters.AddMilliseconds("nonce", DateTime.UtcNow);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Approve Builder Fee

        /// <inheritdoc />
        public Task<CallResult> ApproveBuilderFeeAsync(CancellationToken ct = default)
            => ApproveBuilderFeeAsync(_baseClient.ClientOptions.BuilderAddress, _baseClient.ClientOptions.BuilderFeePercentage ?? 0.1m);

        /// <inheritdoc />
        public async Task<CallResult> ApproveBuilderFeeAsync(string builderAddress, decimal maxFeePercentage, CancellationToken ct = default)
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

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidDefault>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Get Sub account list

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidSubAccount[]>> GetSubAccountsAsync(string? address = null, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "subAccounts" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidSubAccount[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            if (!result)
                return result;

            if (result.Data == null)
                return result.As(new HyperLiquidSubAccount[0]);

            return result;
        }

        #endregion

        #region Get User Role

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidUserRole>> GetUserRoleAsync(string? address = null, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userRole" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidUserRole>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Extra Agents

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidUserAgent[]>> GetExtraAgentsAsync(string? address = null, CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "extraAgents" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidUserAgent[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Get Staking Delegations
        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidStakingDelegation[]>> GetStakingDelegationsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "delegations" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidStakingDelegation[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region Get Staking Summary
        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidStakingSummary>> GetStakingSummaryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "delegatorSummary" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidStakingSummary>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region Get Staking History
        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidStakingHistory[]>> GetStakingHistoryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "delegatorHistory" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidStakingHistory[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region Get Staking History
        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidStakingReward[]>> GetStakingRewardsAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");
            
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "delegatorRewards" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidStakingReward[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            return result;
        }
        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserLedgerUpdatesAsync(string? address, Action<DataEvent<HyperLiquidAccountLedger>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidLedgerUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                onMessage(
                    new DataEvent<HyperLiquidAccountLedger>(HyperLiquidExchange.ExchangeName, data.Data.Ledger, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidLedgerUpdate>(_logger, _baseClient, "userNonFundingLedgerUpdates", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                _baseClient.UpdateTimeOffset(data.Data.ServerTime);

                onMessage(
                    new DataEvent<HyperLiquidUserUpdate>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.ServerTime, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserUpdate>(_logger, _baseClient, "webData2", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToWebData3UpdatesAsync(string? address, Action<DataEvent<HyperLiquidWebDataV3Update>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidWebDataV3Update>>((receiveTime, originalData, invocation, data) =>
            {
                _baseClient.UpdateTimeOffset(data.Data.UserState.ServerTime);

                onMessage(
                    new DataEvent<HyperLiquidWebDataV3Update>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(data.Data.UserState.ServerTime, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidWebDataV3Update>(_logger, _baseClient, "webData3", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserEventUpdatesAsync(
            string? address,
            Action<DataEvent<HyperLiquidUserTrade[]>>? onTradeUpdate = null,
            Action<DataEvent<HyperLiquidUserFunding>>? onFundingUpdate = null,
            Action<DataEvent<HyperLiquidLiquidationUpdate>>? onLiquidationUpdate = null,
            Action<DataEvent<HyperLiquidNonUserCancelation[]>>? onNonUserCancelation = null,
            CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserEventUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                if (data.Data.Trades?.Any() == true)
                {
                    onTradeUpdate?.Invoke(
                        new DataEvent<HyperLiquidUserTrade[]>(HyperLiquidExchange.ExchangeName, data.Data.Trades!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }

                if (data.Data.Funding != null)
                {
                    onFundingUpdate?.Invoke(
                        new DataEvent<HyperLiquidUserFunding>(HyperLiquidExchange.ExchangeName, data.Data.Funding!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }

                if (data.Data.Liquidation != null)
                {
                    onLiquidationUpdate?.Invoke(
                        new DataEvent<HyperLiquidLiquidationUpdate>(HyperLiquidExchange.ExchangeName, data.Data.Liquidation!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }

                if (data.Data.NonUserCancelations?.Any() == true)
                {
                    onNonUserCancelation?.Invoke(
                        new DataEvent<HyperLiquidNonUserCancelation[]>(HyperLiquidExchange.ExchangeName, data.Data.NonUserCancelations!, receiveTime, originalData)
                            .WithUpdateType(SocketUpdateType.Update)
                            .WithStreamId(data.Channel));
                }
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserEventUpdate>(_logger, _baseClient, "userEvents", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false, "user");
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
