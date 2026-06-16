// 04-multi-exchange.cs
//
// Demonstrates: writing exchange-agnostic code using CryptoExchange.Net.SharedApis.
// Same code works against HyperLiquid and other CryptoExchange.Net based libraries
// that implement the same shared interfaces.
//
// Setup:
//   dotnet add package HyperLiquid.Net
//   dotnet add package Binance.Net    // optional, for another exchange
//   dotnet add package JK.OKX.Net      // optional, for another exchange

using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Clients;

// ---- REST SHARED CLIENTS ----
var restClient = new HyperLiquidRestClient();
ISpotTickerRestClient hyperLiquidSpot = restClient.SpotApi.SharedClient;
IFuturesTickerRestClient hyperLiquidFutures = restClient.FuturesApi.SharedClient;

var spotCapabilities = restClient.SpotApi.SharedClient.Discover();
Console.WriteLine($"Shared spot REST features: {spotCapabilities.Features.Count(x => x.Supported)}");

var hypeUsdc = new SharedSymbol(TradingMode.Spot, "HYPE", "USDC");
var ethPerp = new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDC");

await PrintSpotTicker(hyperLiquidSpot, hypeUsdc);
await PrintFuturesTicker(hyperLiquidFutures, ethPerp);

async Task PrintSpotTicker(ISpotTickerRestClient client, SharedSymbol symbol)
{
    var result = await client.GetSpotTickerAsync(new GetTickerRequest(symbol));
    if (!result.Success)
    {
        Console.WriteLine($"[{client.Exchange}] Spot ticker failed: {result.Error}");
        return;
    }

    Console.WriteLine($"[{client.Exchange}] {result.Data.Symbol}: {result.Data.LastPrice}");
}

async Task PrintFuturesTicker(IFuturesTickerRestClient client, SharedSymbol symbol)
{
    var result = await client.GetFuturesTickerAsync(new GetTickerRequest(symbol));
    if (!result.Success)
    {
        Console.WriteLine($"[{client.Exchange}] Futures ticker failed: {result.Error}");
        return;
    }

    Console.WriteLine($"[{client.Exchange}] {result.Data.Symbol}: {result.Data.LastPrice}");
}

// ---- WEBSOCKET SHARED SUBSCRIPTION ----
var socketClient = new HyperLiquidSocketClient();
ITickerSocketClient tickerSocket = socketClient.SpotApi.SharedClient;

var sub = await tickerSocket.SubscribeToTickerUpdatesAsync(
    new SubscribeTickerRequest(hypeUsdc),
    update => Console.WriteLine($"[{tickerSocket.Exchange}] {update.Data.Symbol}: {update.Data.LastPrice}"));

if (!sub.Success)
{
    Console.WriteLine($"Subscribe failed: {sub.Error}");
    return;
}

Console.WriteLine("Press Enter to exit");
Console.ReadLine();

await socketClient.UnsubscribeAsync(sub.Data);

// Common variations:
//   Multi-exchange dashboard: use List<ISpotTickerRestClient> or List<IFuturesTickerRestClient>
//   Cross-exchange orderbook: use IOrderBookSocketClient on each exchange
//   Unified balances:        use IBalanceRestClient
//   Unified futures orders:  use IFuturesOrderRestClient
//   Symbol formatting:       SharedSymbol normalizes spot/perp differences where supported
