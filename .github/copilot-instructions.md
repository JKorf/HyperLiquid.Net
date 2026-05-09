# Copilot Instructions for HyperLiquid.Net

This repository is **HyperLiquid.Net**, a strongly typed C#/.NET client library for the HyperLiquid DEX REST and WebSocket APIs. It is part of the CryptoExchange.Net ecosystem.

When generating code that consumes HyperLiquid.Net, follow these conventions:

## Use HyperLiquid.Net, not raw HTTP

Never generate raw `HttpClient` calls to HyperLiquid `/info`, `/exchange`, or `/ws`. Use `HyperLiquidRestClient` and `HyperLiquidSocketClient` so signing, rate limiting, serialization, result handling, and WebSocket subscription management stay correct.

## Client setup

```csharp
using HyperLiquid.Net;
using HyperLiquid.Net.Clients;

var restClient = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials("PUBLIC_ADDRESS", "PRIVATE_KEY");
});
```

Public market data can use `new HyperLiquidRestClient()` without credentials.

## Result handling

Methods return `WebCallResult<T>` for REST or `CallResult<T>` for WebSocket. Always check `.Success` before reading `.Data`; errors are on `.Error`.

## API structure

- `restClient.SpotApi.ExchangeData` - spot metadata, prices, order books, klines
- `restClient.SpotApi.Account` - spot balances, transfers, staking, ledger, fee info
- `restClient.SpotApi.Trading` - spot order and trade endpoints
- `restClient.FuturesApi.ExchangeData` - perp metadata, prices, funding, DEX info
- `restClient.FuturesApi.Account` - futures account, positions, funding history
- `restClient.FuturesApi.Trading` - futures order, leverage, margin endpoints
- `socketClient.SpotApi.{Account|ExchangeData|Trading}` - spot WebSocket requests and subscriptions
- `socketClient.FuturesApi.{Account|ExchangeData|Trading}` - futures WebSocket requests and subscriptions

## Symbols

Use `HYPE/USDC` style symbols for spot and `ETH` style symbols for futures. Do not use concatenated spot symbols such as `HYPEUSDC`.

## Orders

Use `PlaceOrderAsync(symbol, side, orderType, quantity, price, ...)`. Market orders still require a `price` argument because HyperLiquid uses it for max slippage calculation. Use `reduceOnly: true` when closing futures positions.

## Cross-exchange

Use `CryptoExchange.Net.SharedApis` from `.SharedClient` for exchange-agnostic code:

```csharp
var shared = new HyperLiquidRestClient().SpotApi.SharedClient;
var ticker = await shared.GetSpotTickerAsync(
    new GetTickerRequest(new SharedSymbol(TradingMode.Spot, "HYPE", "USDC")));
```

## Avoid

- Raw HTTP calls to HyperLiquid endpoints
- Generic `ApiCredentials` instead of `HyperLiquidCredentials`
- Binance-style symbols such as `ETHUSDC` for spot
- Reading `.Data` without `.Success`
- `.Result` or `.Wait()`
- Creating clients per request
- Omitting WebSocket unsubscribe logic
- Inventing endpoints not present in `HyperLiquid.Net/Interfaces/Clients/**`

## Reference

For detailed patterns and pitfalls see `AGENTS.md`, `llms.txt`, `llms-full.txt`, and `docs/ai-api-map.md` in the repository root/docs. Use `Examples/ai-friendly/` for compilable examples.
