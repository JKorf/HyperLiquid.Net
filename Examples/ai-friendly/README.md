# HyperLiquid.Net AI-Friendly Examples

These examples are compact single-file console programs for AI assistants, documentation snippets, and quick onboarding. They are written to compile independently when copied into a new console project with `HyperLiquid.Net` installed.

## Files

|File|Demonstrates|
|--|--|
|`01-spot-quickstart.cs`|Public prices, spot ticker metadata, authenticated balances, spot limit order pattern, order status, cancellation|
|`02-futures.cs`|Perpetual futures ticker data, leverage, account positions, market order pattern, reduce-only close pattern|
|`03-websocket.cs`|Spot/futures ticker subscriptions, order book subscription, authenticated order updates, unsubscribe cleanup|
|`04-multi-exchange.cs`|CryptoExchange.Net SharedApis for exchange-agnostic REST and WebSocket code|
|`05-error-handling.cs`|`WebCallResult<T>` handling, retry decisions, symbol formatting, builder fee checks, validation categories|

## Setup

```bash
dotnet new console -n HyperLiquidExample
cd HyperLiquidExample
dotnet add package HyperLiquid.Net
```

Copy one example into `Program.cs`, replace `PUBLIC_ADDRESS` and `PRIVATE_KEY` where authenticated actions are used, and run:

```bash
dotnet run
```

## Rules These Examples Follow

- Use `HyperLiquidRestClient` and `HyperLiquidSocketClient`; no raw HTTP.
- Use `HyperLiquidCredentials`, not generic `ApiCredentials`.
- Check `.Success` before reading `.Data`.
- Use `HYPE/USDC` style spot symbols and `ETH` style futures symbols.
- Pass `price` to market orders because HyperLiquid uses it for max slippage calculation.
- Store WebSocket subscriptions and unsubscribe on shutdown.
- Use SharedApis for exchange-agnostic code.
