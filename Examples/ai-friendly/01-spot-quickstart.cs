// 01-spot-quickstart.cs
//
// Demonstrates: client setup, public spot market data, authenticated balances,
// limit order placement, order status check, cancellation.
//
// Setup:
//   dotnet new console -n HyperLiquidSpotQuickstart && cd HyperLiquidSpotQuickstart
//   dotnet add package HyperLiquid.Net
//   Copy this file content into Program.cs
//   Substitute PUBLIC_ADDRESS / PRIVATE_KEY below
//   dotnet run

using HyperLiquid.Net;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Enums;

// ---- 1. PUBLIC CLIENT (no credentials needed for market data) ----
// Reuse this client across the application; do not create per request.
var publicClient = new HyperLiquidRestClient();

var prices = await publicClient.SpotApi.ExchangeData.GetPricesAsync();
if (!prices.Success)
{
    Console.WriteLine($"Failed to get mid prices: {prices.Error}");
    return;
}

Console.WriteLine($"HYPE/USDC mid price: {prices.Data["HYPE/USDC"]}");

var spotInfo = await publicClient.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync();
if (!spotInfo.Success)
{
    Console.WriteLine($"Failed to get spot tickers: {spotInfo.Error}");
    return;
}

var hypeTicker = spotInfo.Data.Tickers.Single(x => x.Symbol == "HYPE/USDC");
Console.WriteLine($"HYPE/USDC base volume: {hypeTicker.BaseVolume}");
Console.WriteLine($"HYPE/USDC quote volume: {hypeTicker.QuoteVolume}");

// ---- 2. AUTHENTICATED CLIENT (for account / trading) ----
var tradingClient = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials("PUBLIC_ADDRESS", "PRIVATE_KEY");
});

var balances = await tradingClient.SpotApi.Account.GetBalancesAsync();
if (!balances.Success)
{
    Console.WriteLine($"Failed to get balances: {balances.Error}");
    return;
}

foreach (var balance in balances.Data.Where(b => b.Total > 0))
{
    Console.WriteLine($"{balance.Asset}: total={balance.Total}, hold={balance.Hold}");
}

// ---- 3. PLACE A LIMIT BUY ORDER ----
// HyperLiquid spot symbols use Base/Quote notation, for example HYPE/USDC.
var order = await tradingClient.SpotApi.Trading.PlaceOrderAsync(
    symbol: "HYPE/USDC",
    side: OrderSide.Buy,
    orderType: OrderType.Limit,
    quantity: 1m,
    price: 10m,
    timeInForce: TimeInForce.GoodTillCanceled);

if (!order.Success)
{
    Console.WriteLine($"Failed to place order: {order.Error}");
    return;
}

Console.WriteLine($"Placed order {order.Data.OrderId}, status: {order.Data.Status}");

// ---- 4. CHECK ORDER STATUS ----
var status = await tradingClient.SpotApi.Trading.GetOrderAsync(order.Data.OrderId);
if (status.Success)
{
    Console.WriteLine($"Order status: {status.Data.Status}");
}

// ---- 5. CANCEL THE ORDER (cleanup for this example) ----
var cancel = await tradingClient.SpotApi.Trading.CancelOrderAsync("HYPE/USDC", order.Data.OrderId);
if (cancel.Success)
{
    Console.WriteLine($"Cancelled order {order.Data.OrderId}");
}

// Common variations:
//   Market order:  orderType: OrderType.Market, still pass price for slippage calculation
//   Trigger order: add triggerPrice and tpSlType
//   Client order id: pass a 128-bit hex string via clientOrderId when external correlation is needed
//   Shared API:    use new HyperLiquidRestClient().SpotApi.SharedClient for exchange-agnostic code
