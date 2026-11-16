# HyperLiquid operational considerations

## Wallet and approved signer

`HyperLiquidCredential` separates the account wallet from an optional approved signing key. `EthKey.FromPrivateKey` supplies a signing key; with an approved signer, use `EthKey.FromPublicAddress` for the account wallet and `EthKey.FromPrivateKey` for the signer. Validation requires a signer private key when supplied, otherwise a wallet private key. Keep private keys in secret storage. Explicit account addresses identify the account to read; an agent address is not a substitute for the wallet whose balances/positions are requested. `Account.ApproveAgentAsync` authorizes a signer; `RemoveAgentAsync` uses the exchange web interface's undocumented zero-address approval convention, so verify removal with `GetExtraAgentsAsync`. Agent permissions do not imply permission for wallet-only transfers, withdrawals, or approvals.

## Builder fees

REST and socket options default `BuilderFeePercentage` to `0.01m` (0.01%, one basis point). Check `Account.GetApprovedBuilderFeeAsync()` and explicitly handle `ApproveBuilderFeeAsync()` for the configured builder. Approval is an account change. Configure the fee explicitly when the application needs another value; `0` disables it. Avoid silently changing this cost policy while generating unrelated code.

## Execution and position state

A limit order can execute immediately. Cancellation can race with fills; inspect order status and trades rather than treating successful submission or cancellation as proof of no exposure. Market orders require a recent reference price for slippage; do not substitute entry prices or arbitrary fallback prices. Closing a position requires its current signed quantity, the opposite side, and `reduceOnly: true`. `SetLeverageAsync` and `UpdateIsolatedMarginAsync` change margin state and liquidation exposure; scope changes to the correct symbol, vault, and HIP-3 DEX. Do not assume hedge-mode flags from other exchanges exist here.

Client order ids, when provided, must be 128-bit hex strings. Retain them for correlation. Place/edit/cancel batches return nested per-item `CallResult` values; an outer success can contain individual failures. Inspect all entries before deciding what remains open. TWAP and scheduled cancellation have their own identifiers and state. `CancelAfterAsync` has exchange eligibility constraints, including trading-volume requirements; it cannot be assumed to protect every account. Its scheduled cancellation covers both spot and futures orders; a null timeout removes the countdown.

## Asset movements and DEX state

`TransferInternalAsync` moves USD between spot and perpetual account classes; `TransferSpotAsync` sends spot assets to another address. `SendAssetAsync` distinguishes source/destination DEX and token identifiers. Confirm destination, asset, subaccount/vault, and direction for the requested operation. Bridge withdrawals can have fees and settlement delays. Staking uses wei-denominated amounts where the method specifies `wei`; vault deposits/withdrawals and validator delegation have distinct account effects. HIP-3 abstraction toggles alter account behavior across DEXes; read the existing state before changing it.

## Uncertain submissions

A timeout or transport error after signing can leave acceptance unknown. `Error.IsTransient` alone does not justify repeating orders, transfers, withdrawals, leverage changes, approvals, or other account changes. Reconcile using order/client ids, open orders, trades, ledger or account state before deciding whether to resubmit; account for indexing delays. Bounded backoff is appropriate for transient public reads. Preserve the request identifiers and per-item outcomes needed for recovery.
