# HyperLiquid.Net usage

## Imports

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Web3;
using HyperLiquid.Net;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients;
using Microsoft.Extensions.DependencyInjection;
```

## Public client and results

```csharp
using var client = new HyperLiquidRestClient();
var result = await client.SpotApi.ExchangeData.GetPricesAsync();
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}
if (result.Data.TryGetValue("HYPE/USDC", out var price))
    Console.WriteLine(price);
```

## Wallet authentication

Read secrets from application configuration. Direct wallet signing:

```csharp
var key = Environment.GetEnvironmentVariable("HYPERLIQUID_PRIVATE_KEY")
    ?? throw new InvalidOperationException("Wallet private key is required");
using var client = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials(
        new HyperLiquidCredential(EthKey.FromPrivateKey(key)));
    options.Environment = HyperLiquidEnvironment.Testnet;
});
var balances = await client.SpotApi.Account.GetBalancesAsync();
if (!balances.Success)
{
    Console.WriteLine(balances.Error);
    return;
}
foreach (var balance in balances.Data.Balances)
    Console.WriteLine($"{balance.Asset}: {balance.Total}");
```

For an already approved agent, keep the wallet account address separate from the signing key:

```csharp
var address = Environment.GetEnvironmentVariable("HYPERLIQUID_WALLET_ADDRESS")
    ?? throw new InvalidOperationException("Wallet address is required");
var key = Environment.GetEnvironmentVariable("HYPERLIQUID_SIGNER_PRIVATE_KEY")
    ?? throw new InvalidOperationException("Signer private key is required");
using var client = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials(new HyperLiquidCredential(
        EthKey.FromPublicAddress(address), EthKey.FromPrivateKey(key)));
});
var account = await client.FuturesApi.Account.GetAccountInfoAsync();
if (!account.Success)
{
    Console.WriteLine(account.Error);
    return;
}
Console.WriteLine(account.Data.Positions.Length);
```

## Representative trading operation

This example submits an executable order when run. It assumes the authenticated client above is configured, along with the intended environment and builder approval. See [operational considerations](safety.md) before executing.

```csharp
var order = await client.SpotApi.Trading.PlaceOrderAsync(
    symbol: "HYPE/USDC", side: OrderSide.Buy, orderType: OrderType.Limit,
    quantity: 1m, price: 10m, timeInForce: TimeInForce.GoodTillCanceled);
if (!order.Success)
{
    Console.WriteLine(order.Error);
    return;
}
Console.WriteLine($"{order.Data.OrderId}: {order.Data.Status}");
var cancel = await client.SpotApi.Trading.CancelOrderAsync("HYPE/USDC", order.Data.OrderId);
if (!cancel.Success)
    Console.WriteLine(cancel.Error);
```

## Socket requests and subscriptions

Socket requests have a result distinct from subscription handles:

```csharp
using var socketClient = new HyperLiquidSocketClient();
var prices = await socketClient.FuturesApi.ExchangeData.GetPricesAsync();
if (!prices.Success)
{
    Console.WriteLine(prices.Error);
    return;
}
Console.WriteLine(prices.Data.Count);
```

Keep subscriptions active for the required application lifetime, then unsubscribe:

```csharp
using var socketClient = new HyperLiquidSocketClient();
var sub = await socketClient.FuturesApi.ExchangeData.SubscribeToSymbolUpdatesAsync(
    "ETH", update => Console.WriteLine(update.Data.MidPrice));
if (!sub.Success)
{
    Console.WriteLine(sub.Error);
    return;
}
try
{
    await Task.Delay(TimeSpan.FromSeconds(30));
}
finally
{
    await socketClient.UnsubscribeAsync(sub.Data);
}
```

Account streams accept an explicit wallet address; null uses the configured credential wallet. Retain and clean up those successful subscription handles in the same way.

## Closing a perpetual position

A perpetual reduce-only close using current position quantity and a freshly fetched reference price:

```csharp
const string symbol = "ETH";
var account = await client.FuturesApi.Account.GetAccountInfoAsync();
if (!account.Success)
{
    Console.WriteLine(account.Error);
    return;
}
var position = account.Data.Positions.Select(x => x.Position)
    .FirstOrDefault(x => x.Symbol == symbol && x.PositionQuantity != 0);
if (position?.PositionQuantity is not decimal quantity || quantity == 0)
    return;
var prices = await client.FuturesApi.ExchangeData.GetPricesAsync();
if (!prices.Success)
{
    Console.WriteLine(prices.Error);
    return;
}
if (!prices.Data.TryGetValue(symbol, out var price) || price <= 0)
    return;
var close = await client.FuturesApi.Trading.PlaceOrderAsync(
    symbol: symbol, side: quantity > 0 ? OrderSide.Sell : OrderSide.Buy,
    orderType: OrderType.Market, quantity: Math.Abs(quantity), price: price, reduceOnly: true);
if (!close.Success)
    Console.WriteLine(close.Error);
```

## Shared API V2

```csharp
using var client = new HyperLiquidRestClient();
IGetTickerRest ticker = client.FuturesApi.SharedApi;
var symbol = new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDC");
var result = await ticker.GetTickerAsync(new GetTickerRequest(symbol));
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}
Console.WriteLine(result.Data.LastPrice);
```

Populate the symbol catalog through a successful symbol request:

```csharp
IGetSpotSymbolsRest symbolsClient = client.SpotApi.SharedApi;
var result = await symbolsClient.GetSpotSymbolsAsync(new GetSymbolsRequest());
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}
foreach (var symbol in result.Data.Take(5))
    Console.WriteLine($"{symbol.Name}: {symbol.BaseAsset}/{symbol.QuoteAsset}");
var catalog = symbolsClient.SpotSymbolCatalog;
```

## Dependency injection and dynamic lookup

```csharp
var services = new ServiceCollection();
services.AddHyperLiquid(options =>
{
    options.Environment = HyperLiquidEnvironment.Testnet;
});
using var provider = services.BuildServiceProvider();
var aggregate = provider.GetRequiredService<IHyperLiquidSharedApiClient>();
var match = aggregate.GetCapability(
    SharedCapabilities.Tickers.GetTicker.Rest, tradingMode: TradingMode.Spot);
if (match is null)
    return;
var result = await match.Capability.GetTickerAsync(new GetTickerRequest(
    new SharedSymbol(TradingMode.Spot, "HYPE", "USDC")));
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}
Console.WriteLine(result.Data.LastPrice);
```

Set `options.ApiCredentials` for authenticated DI consumers, or configure `options.Rest.ApiCredentials` and `options.Socket.ApiCredentials` separately. Inject `IHyperLiquidRestClient`, `IHyperLiquidSocketClient`, the aggregate, or a registered narrow capability as needed. [API routing](api-surfaces.md#shared-api-v2) describes implementation selection when multiple exchanges are registered.

## Complete programs (checkout-only)

[Examples](../../../../Examples/ai-friendly/README.md) includes spot, futures, socket lifecycle, Shared API V2, and public-read error handling programs. These links require a repository checkout; the snippets above work with the installed package.
