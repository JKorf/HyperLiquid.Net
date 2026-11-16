# HyperLiquid.Net examples

Each file is an independent console program. Install `HyperLiquid.Net` in a console project and copy one file to `Program.cs`.

| Program | Workflow |
|---|---|
| [01-spot-quickstart.cs](01-spot-quickstart.cs) | Public prices, wallet balances, spot order/status/cancellation |
| [02-futures.cs](02-futures.cs) | Perpetual metadata, leverage, positions, market orders and reduce-only close |
| [03-websocket.cs](03-websocket.cs) | Public and account subscriptions with cleanup |
| [04-multi-exchange.cs](04-multi-exchange.cs) | Shared API V2 ticker capabilities, symbol catalog, and subscription |
| [05-error-handling.cs](05-error-handling.cs) | Results, bounded public-read retries, formatting and builder fee reads |

Replace `<private key>` only for the intended account operations. Several examples submit orders or change leverage when executed; compilation does not execute them. Read [operational considerations](../../.claude/skills/hyperliquid-net/references/safety.md) before running account-changing examples. Limit orders can fill before cancellation.

See the [consumer skill](../../.claude/skills/hyperliquid-net/SKILL.md) and [usage snippets](../../.claude/skills/hyperliquid-net/references/usage.md) for wallet/signer setup. The repository's `AiExampleCompileTests` compiles these programs without running them.
