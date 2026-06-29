// 03-websocket.cs
//
// Demonstrates: WebSocket subscriptions - public spot/futures ticker streams,
// order book stream, authenticated order updates, and proper teardown.
//
// Setup: dotnet add package HyperLiquid.Net

using HyperLiquid.Net;
using HyperLiquid.Net.Clients;

// ---- 1. PUBLIC SOCKET CLIENT ----
// Reuse a single socket client instance across subscriptions.
// Subscription methods return WebSocketResult<UpdateSubscription>.
var publicSocket = new HyperLiquidSocketClient();

var spotTickerSub = await publicSocket.SpotApi.ExchangeData.SubscribeToSymbolUpdatesAsync(
    "HYPE/USDC",
    update =>
    {
        Console.WriteLine($"HYPE/USDC mid: {update.Data.MidPrice}");
    });

if (!spotTickerSub.Success)
{
    Console.WriteLine($"Failed to subscribe spot ticker: {spotTickerSub.Error}");
    return;
}

var futuresTickerSub = await publicSocket.FuturesApi.ExchangeData.SubscribeToSymbolUpdatesAsync(
    "ETH",
    update =>
    {
        Console.WriteLine($"ETH perp mid: {update.Data.MidPrice}");
    });

if (!futuresTickerSub.Success)
{
    Console.WriteLine($"Failed to subscribe futures ticker: {futuresTickerSub.Error}");
    await publicSocket.UnsubscribeAsync(spotTickerSub.Data);
    return;
}

var orderBookSub = await publicSocket.FuturesApi.ExchangeData.SubscribeToOrderBookUpdatesAsync(
    "ETH",
    update =>
    {
        var bestBid = update.Data.Levels.Bids.FirstOrDefault();
        var bestAsk = update.Data.Levels.Asks.FirstOrDefault();
        Console.WriteLine($"ETH book: bid={bestBid?.Price}, ask={bestAsk?.Price}");
    });

if (!orderBookSub.Success)
{
    Console.WriteLine($"Failed to subscribe order book: {orderBookSub.Error}");
    await publicSocket.UnsubscribeAsync(spotTickerSub.Data);
    await publicSocket.UnsubscribeAsync(futuresTickerSub.Data);
    return;
}

// ---- 2. AUTHENTICATED SOCKET CLIENT ----
// Authenticated streams use the address from credentials when address is null.
var authSocket = new HyperLiquidSocketClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials("PUBLIC_ADDRESS", "PRIVATE_KEY");
});

var orderSub = await authSocket.FuturesApi.Trading.SubscribeToOrderUpdatesAsync(
    address: null,
    onMessage: update =>
    {
        foreach (var order in update.Data)
            Console.WriteLine($"Order {order.Order.OrderId}: {order.Status}");
    });

if (!orderSub.Success)
{
    Console.WriteLine($"Failed to subscribe order updates: {orderSub.Error}");
    await publicSocket.UnsubscribeAsync(spotTickerSub.Data);
    await publicSocket.UnsubscribeAsync(futuresTickerSub.Data);
    await publicSocket.UnsubscribeAsync(orderBookSub.Data);
    return;
}

Console.WriteLine("All subscriptions active. Press Enter to teardown...");
Console.ReadLine();

// ---- 3. TEARDOWN ----
await publicSocket.UnsubscribeAsync(spotTickerSub.Data);
await publicSocket.UnsubscribeAsync(futuresTickerSub.Data);
await publicSocket.UnsubscribeAsync(orderBookSub.Data);
await authSocket.UnsubscribeAsync(orderSub.Data);

Console.WriteLine("Clean shutdown complete.");

// Common variations:
//   Spot balances:       SpotApi.Account.SubscribeToBalanceUpdatesAsync(address, handler)
//   User trades:         Trading.SubscribeToUserTradeUpdatesAsync(address, handler)
//   Futures positions:   FuturesApi.Trading.SubscribeToBalanceAndPositionUpdatesAsync(address, dex, handler)
//   All mids:            FuturesApi.ExchangeData.SubscribeToPriceUpdatesAsync(dex, handler)
//   Klines:              ExchangeData.SubscribeToKlineUpdatesAsync(symbol, interval, handler)
