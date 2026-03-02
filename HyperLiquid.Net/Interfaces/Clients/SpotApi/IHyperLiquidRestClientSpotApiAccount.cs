using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Enums;
using System;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot account endpoints. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    /// <see cref="IHyperLiquidRestClientAccount"/>
    public interface IHyperLiquidRestClientSpotApiAccount : IHyperLiquidRestClientAccount
    {
        /// <summary>
        /// Get user asset balances
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/spot#retrieve-a-users-token-balances" /><br />
        /// Endpoint:<br />
        /// POST /info (type: spotClearinghouseState)
        /// </para>
        /// </summary>
        /// <param name="address">Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidBalance[]>> GetBalancesAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user account ledger
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-a-users-funding-history-or-non-funding-ledger-updates" /><br />
        /// Endpoint:<br />
        /// POST /info (type: userNonFundingLedgerUpdates)
        /// </para>
        /// </summary>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="address">Address to request ledger for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidAccountLedger>> GetAccountLedgerAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user rate limits
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-user-rate-limits" /><br />
        /// Endpoint:<br />
        /// POST /info (type: userRateLimit)
        /// </para>
        /// </summary>
        /// <param name="address">Address to request rate limits for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidRateLimit>> GetRateLimitsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get the approved builder fee
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#check-builder-fee-approval" /><br />
        /// Endpoint:<br />
        /// POST /info (type: maxBuilderFee)
        /// </para>
        /// </summary>
        /// <param name="builderAddress">The address of the builder. If not provided will use the builder address for this library</param>
        /// <param name="address">Address to request approved builder fee for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<int>> GetApprovedBuilderFeeAsync(string? builderAddress = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Send usd to another address. This transfer does not touch the EVM bridge.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#l1-usdc-transfer" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: usdSend)
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="quantity">Quantity of USD to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> TransferUsdAsync(string destinationAddress, decimal quantity, CancellationToken ct = default);

        /// <summary>
        /// Send spot assets to another address. This transfer does not touch the EVM bridge.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#l1-spot-transfer" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: spotSend)
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="asset">Asset name, for example "HYPE"</param>
        /// <param name="quantity">Quantity to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> TransferSpotAsync(
            string destinationAddress,
            string asset,
            decimal quantity,
            CancellationToken ct = default);

        /// <summary>
        /// Initiate the withdrawal flow. After making this request, the L1 validators will sign and send the withdrawal request to the bridge contract. There is a $1 fee for withdrawing at the time of this writing and withdrawals take approximately 5 minutes to finalize.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#initiate-a-withdrawal-request" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: withdraw3)
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="quantity">Quantity of USD to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> WithdrawAsync(
            string destinationAddress,
            decimal quantity,
            CancellationToken ct = default);

        /// <summary>
        /// Transfer USD between Spot and Futures account
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#transfer-from-spot-account-to-perp-account-and-vice-versa" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: usdClassTransfer)
        /// </para>
        /// </summary>
        /// <param name="direction">Transfer direction</param>
        /// <param name="quantity">Quantity of USD to send</param>
        /// <param name="subAccount">Subaccount address</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> TransferInternalAsync(
            TransferDirection direction,
            decimal quantity,
            string? subAccount = null,
            CancellationToken ct = default);

        /// <summary>
        /// Deposit into staking
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#deposit-into-staking" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: cDeposit)
        /// </para>
        /// </summary>
        /// <param name="wei">Quantity</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> DepositIntoStakingAsync(long wei, CancellationToken ct = default);

        /// <summary>
        /// Withdraw from staking into the user's spot account. Note that transfers from staking to spot account go through a 7 day unstaking queue.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#withdraw-from-staking" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: cWithdraw)
        /// </para>
        /// </summary>
        /// <param name="wei">Quantity</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> WithdrawFromStakingAsync(long wei, CancellationToken ct = default);

        /// <summary>
        /// Delegate or undelegate native tokens to or from a validator. Note that delegations to a particular validator have a lockup duration of 1 day.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#delegate-or-undelegate-stake-from-validator" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: tokenDelegate)
        /// </para>
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="validator">Validator address in hex format, for example 0x0000000000000000000000000000000000000000</param>
        /// <param name="wei">Quantity</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> DelegateOrUndelegateStakeFromValidatorAsync(DelegateDirection direction, string validator, long wei, CancellationToken ct = default);

        /// <summary>
        /// Deposit or withdraw from vault
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#deposit-or-withdraw-from-a-vault" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: vaultTransfer)
        /// </para>
        /// </summary>
        /// <param name="direction">Direction</param>
        /// <param name="vaultAddress">Vault address</param>
        /// <param name="usd">USD to withdraw or deposit</param>
        /// <param name="expireAfter">Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> DepositOrWithdrawFromVaultAsync(DepositWithdrawDirection direction, string vaultAddress, long usd, DateTime? expireAfter = null, CancellationToken ct = default);

        /// <summary>
        /// Approve a builder address of the library to charge the fee percentage as defined in the BuilderFeePercentage client options field
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#approve-a-builder-fee" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: approveBuilderFee)
        /// </para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> ApproveBuilderFeeAsync(CancellationToken ct = default);

        /// <summary>
        /// Approve a builder address to charge a certain fee
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#approve-a-builder-fee" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: approveBuilderFee)
        /// </para>
        /// </summary>
        /// <param name="builderAddress">The address of the builder in hex format, for example 0x0000000000000000000000000000000000000000</param>
        /// <param name="maxFeePercentage">Max fee percentage the builder can charge</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult> ApproveBuilderFeeAsync(string builderAddress, decimal maxFeePercentage, CancellationToken ct = default);

        /// <summary>
        /// Get sub account list
        /// </summary>
        /// <param name="address">Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidSubAccount[]>> GetSubAccountsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user role
        /// </summary>
        /// <param name="address">Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidUserRole>> GetUserRoleAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get extra agents associated with a user
        /// </summary>
        /// <param name="address">Address to request agents for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidUserAgent[]>> GetExtraAgentsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get staking delegations
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-delegations" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegations)
        /// </para>
        /// </summary>
        /// <param name="address">Address to request delegations for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidStakingDelegation[]>> GetStakingDelegationsAsync(string? address = null, CancellationToken ct = default);
        /// <summary>
        /// Get staking summary
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-summary" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegatorSummary)
        /// </para>
        /// </summary>
        /// <param name="address">Address to request summary for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidStakingSummary>> GetStakingSummaryAsync(string? address = null, CancellationToken ct = default);
        /// <summary>
        /// Get staking history
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-history" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegatorHistory)
        /// </para>
        /// </summary>
        /// <param name="address">Address to request history for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidStakingHistory[]>> GetStakingHistoryAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get staking rewards history
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-rewards" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegatorRewards)
        /// </para>
        /// </summary>
        /// <param name="address">Address to request rewards for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<HyperLiquidStakingReward[]>> GetStakingRewardsAsync(string? address = null, CancellationToken ct = default);
    }
}
