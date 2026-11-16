---
name: hyperliquid-net
description: Generate and troubleshoot C#/.NET code consuming HyperLiquid.Net for spot and perpetual trading, wallet/account operations, HIP-3 DEX and HIP-4 outcome data, REST, WebSocket, and Shared API V2 workflows.
---

# HyperLiquid.Net

Install the consumer package:

```sh
dotnet add package HyperLiquid.Net
```

Use `HyperLiquidRestClient` and `HyperLiquidSocketClient` with `SpotApi` or `FuturesApi`; each exposes `ExchangeData`, `Account`, and `Trading`. Reuse and dispose clients, await calls, and retain successful subscriptions for unsubscribe cleanup. Socket APIs support both requests and subscriptions.

Credentials are `HyperLiquidCredentials(new HyperLiquidCredential(wallet, signer))`, using `CryptoExchange.Net.Web3.EthKey`. A wallet private key can sign directly; an optional approved signer signs for the wallet. The old public-address/private-key string constructor is unavailable. Public market data needs no credentials; account reads and streams can accept an explicit wallet address.

Native spot symbols use `HYPE/USDC`; perpetuals use `ETH`, with DEX-qualified symbols where supplied by metadata. Use metadata or `HyperLiquidExchange.FormatSymbol` rather than constructing unrelated exchange formats.

Check `Success` before `Data`: REST returns `HttpResult`, socket requests `QueryResult`, subscriptions `WebSocketResult<UpdateSubscription>`, and some Shared helpers `ExchangeCallResult`, with generic variants where applicable. Batch operations also require checking each item.

Use `.SharedApi` and narrow V2 capabilities for portable operations. `IHyperLiquidSharedApiClient` groups the four supported surfaces and supports dynamic lookup. Do not build new examples on legacy `.SharedClient` topic interfaces.

## Read when needed

- [API surfaces](references/api-surfaces.md): choose roots, endpoint groups, HIP workflows, Shared capabilities, symbols, and quantities.
- [Usage](references/usage.md): client setup, wallet credentials, account/trading examples, subscriptions, Shared API V2, and DI.
- [Operational considerations](references/safety.md): before signing account changes; builder fees, wallet/signer roles, execution, margin, and uncertain submissions.

For work on the library itself, use [repository instructions](../../../AGENTS.md) (checkout-only).
