using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Enums;

namespace HyperLiquid.Net.Interfaces.Clients.BaseApi
{
    /// <summary>
    /// HyperLiquid WebSocket account endpoints and streams. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    public interface IHyperLiquidSocketClientApiAccount
    {
        /// <summary>
        /// Get user trading fee rates
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request open orders for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidFeeInfo>> GetFeeInfoAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// This generalized method is used to transfer tokens between different perp DEXs, spot balance, users, and/or sub-accounts. Use "" to specify the default USDC perp DEX and "spot" to specify spot. Only the collateral token can be transferred to or from a perp DEX.
        /// </summary>
        /// <param name="destination">["<c>destination</c>"] Destination address</param>
        /// <param name="sourceDex">["<c>sourceDex</c>"] Source Dex, empty string for the default USDC DEX, "spot" for spot</param>
        /// <param name="destinationDex">["<c>destinationDex</c>"] Destination Dex, empty string for the default USDC DEX, "spot" for spot</param>
        /// <param name="tokenName">Token name, for example `PURR`</param>
        /// <param name="tokenId">Token id, for example `0xc4bf3f870c0e9465323c0b6ed28096c2`</param>
        /// <param name="quantity">["<c>amount</c>"] Quantity to send</param>
        /// <param name="fromSubAccount">["<c>fromSubAccount</c>"] Source sub account</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> SendAssetAsync(
            string destination,
            string sourceDex,
            string destinationDex,
            string tokenName,
            string tokenId,
            decimal quantity,
            string? fromSubAccount = null,
            CancellationToken ct = default);

        /// <summary>
        /// Get user account ledger
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint/perpetuals#retrieve-a-users-funding-history-or-non-funding-ledger-updates" /><br />
        /// Endpoint:<br />
        /// POST /info (type: userNonFundingLedgerUpdates)
        /// </para>
        /// </summary>
        /// <param name="startTime">["<c>startTime</c>"] Filter by start time</param>
        /// <param name="endTime">["<c>endTime</c>"] Filter by end time</param>
        /// <param name="address">["<c>user</c>"] Address to request ledger for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidAccountLedger>> GetAccountLedgerAsync(DateTime startTime, DateTime? endTime = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user rate limits
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-user-rate-limits" /><br />
        /// Endpoint:<br />
        /// POST /info (type: userRateLimit)
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request rate limits for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidRateLimit>> GetRateLimitsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get the approved builder fee
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#check-builder-fee-approval" /><br />
        /// Endpoint:<br />
        /// POST /info (type: maxBuilderFee)
        /// </para>
        /// </summary>
        /// <param name="builderAddress">["<c>builder</c>"] The address of the builder. If not provided will use the builder address for this library</param>
        /// <param name="address">["<c>user</c>"] Address to request approved builder fee for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<int>> GetApprovedBuilderFeeAsync(string? builderAddress = null, string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Send usd to another address. This transfer does not touch the EVM bridge.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#l1-usdc-transfer" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: usdSend)
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">["<c>destination</c>"] Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="quantity">["<c>amount</c>"] Quantity of USD to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> TransferUsdAsync(string destinationAddress, decimal quantity, CancellationToken ct = default);

        /// <summary>
        /// Initiate the withdrawal flow. After making this request, the L1 validators will sign and send the withdrawal request to the bridge contract. There is a $1 fee for withdrawing at the time of this writing and withdrawals take approximately 5 minutes to finalize.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#initiate-a-withdrawal-request" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: withdraw3)
        /// </para>
        /// </summary>
        /// <param name="destinationAddress">["<c>destination</c>"] Address in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000</param>
        /// <param name="quantity">["<c>amount</c>"] Quantity of USD to send</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> WithdrawAsync(
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
        /// <param name="quantity">["<c>amount</c>"] Quantity of USD to send</param>
        /// <param name="subAccount">Subaccount address</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> TransferInternalAsync(
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
        /// <param name="wei">["<c>wei</c>"] Quantity</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> DepositIntoStakingAsync(long wei, CancellationToken ct = default);

        /// <summary>
        /// Withdraw from staking into the user's spot account. Note that transfers from staking to spot account go through a 7 day unstaking queue.
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#withdraw-from-staking" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: cWithdraw)
        /// </para>
        /// </summary>
        /// <param name="wei">["<c>wei</c>"] Quantity</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> WithdrawFromStakingAsync(long wei, CancellationToken ct = default);

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
        /// <param name="validator">["<c>validator</c>"] Validator address in hex format, for example 0x0000000000000000000000000000000000000000</param>
        /// <param name="wei">["<c>wei</c>"] Quantity</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> DelegateOrUndelegateStakeFromValidatorAsync(DelegateDirection direction, string validator, long wei, CancellationToken ct = default);

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
        /// <param name="vaultAddress">["<c>vaultAddress</c>"] Vault address</param>
        /// <param name="usd">["<c>usd</c>"] USD to withdraw or deposit</param>
        /// <param name="expireAfter">["<c>expiresAfter</c>"] Timestamp after which the request expires and is rejected by the server</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> DepositOrWithdrawFromVaultAsync(DepositWithdrawDirection direction, string vaultAddress, long usd, DateTime? expireAfter = null, CancellationToken ct = default);

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
        Task<CallResult> ApproveBuilderFeeAsync(CancellationToken ct = default);

        /// <summary>
        /// Approve a builder address to charge a certain fee
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/exchange-endpoint#approve-a-builder-fee" /><br />
        /// Endpoint:<br />
        /// POST /exchange (type: approveBuilderFee)
        /// </para>
        /// </summary>
        /// <param name="builderAddress">["<c>builder</c>"] The address of the builder in hex format, for example 0x0000000000000000000000000000000000000000</param>
        /// <param name="maxFeePercentage">["<c>maxFeeRate</c>"] Max fee percentage the builder can charge</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult> ApproveBuilderFeeAsync(string builderAddress, decimal maxFeePercentage, CancellationToken ct = default);

        /// <summary>
        /// Get sub account list
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidSubAccount[]>> GetSubAccountsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get user role
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request balances for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidUserRole>> GetUserRoleAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get extra agents associated with a user
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request agents for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidUserAgent[]>> GetExtraAgentsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get staking delegations
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-delegations" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegations)
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request delegations for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidStakingDelegation[]>> GetStakingDelegationsAsync(string? address = null, CancellationToken ct = default);
        /// <summary>
        /// Get staking summary
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-summary" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegatorSummary)
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request summary for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidStakingSummary>> GetStakingSummaryAsync(string? address = null, CancellationToken ct = default);
        /// <summary>
        /// Get staking history
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-history" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegatorHistory)
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request history for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidStakingHistory[]>> GetStakingHistoryAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Get staking rewards history
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/info-endpoint#query-a-users-staking-rewards" /><br />
        /// Endpoint:<br />
        /// POST /info (type: delegatorRewards)
        /// </para>
        /// </summary>
        /// <param name="address">["<c>user</c>"] Address to request rewards for. If not provided will use the address provided in the API credentials</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<HyperLiquidStakingReward[]>> GetStakingRewardsAsync(string? address = null, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user non-order updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: userEvents)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onTradeUpdate">User trade data handler</param>
        /// <param name="onFundingUpdate">User funding update handler</param>
        /// <param name="onLiquidationUpdate">User liquidation update handler</param>
        /// <param name="onNonUserCancelation">Non-user order cancelation update</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserEventUpdatesAsync(
            string? address,
            Action<DataEvent<HyperLiquidUserTrade[]>>? onTradeUpdate = null,
            Action<DataEvent<HyperLiquidUserFunding>>? onFundingUpdate = null,
            Action<DataEvent<HyperLiquidLiquidationUpdate>>? onLiquidationUpdate = null,
            Action<DataEvent<HyperLiquidNonUserCancelation[]>>? onNonUserCancelation = null,
            CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user ledger updates (excluding funding updates)will provided updates for both Spot and Futures changes
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: userNonFundingLedgerUpdates)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserLedgerUpdatesAsync(string? address, Action<DataEvent<HyperLiquidAccountLedger>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to user updates, including Spot and Futures balances
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: webData2)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToUserUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserUpdate>> onMessage, CancellationToken ct = default);

        /// <summary>
        /// Subscribe to webData3 updates
        /// <para>
        /// Docs:<br />
        /// <a href="https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api/websocket/subscriptions" /><br />
        /// Endpoint:<br />
        /// WS /ws (type: webData3)
        /// </para>
        /// </summary>
        /// <param name="address">Address to subscribe for. If not provided will use the address provided in the API credentials</param>
        /// <param name="onMessage">The data handler</param>
        /// <param name="ct">Cancellation token for closing this subscription</param>
        /// <returns>A stream subscription. This stream subscription can be used to be notified when the socket is disconnected/reconnected and to unsubscribe</returns>
        Task<CallResult<UpdateSubscription>> SubscribeToWebData3UpdatesAsync(string? address, Action<DataEvent<HyperLiquidWebDataV3Update>> onMessage, CancellationToken ct = default);
    }
}
