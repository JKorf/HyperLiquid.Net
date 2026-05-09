// 02-futures.cs
//
// Demonstrates: perpetual futures - metadata, leverage, account positions,
// market order pattern, reduce-only close pattern.
//
// Setup: dotnet add package HyperLiquid.Net
// Substitute PUBLIC_ADDRESS / PRIVATE_KEY. The key must be able to sign HyperLiquid actions.

using HyperLiquid.Net;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Enums;

var client = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials("PUBLIC_ADDRESS", "PRIVATE_KEY");
});

const string symbol = "ETH"; // Perp symbols are base asset only. Do not use ETH/USDC here.

// ---- 1. GET FUTURES METADATA AND TICKER DATA ----
var tickers = await client.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync();
if (!tickers.Success)
{
    Console.WriteLine($"Failed to get futures tickers: {tickers.Error}");
    return;
}

var ethTicker = tickers.Data.Tickers.Single(x => x.Symbol == symbol);
Console.WriteLine($"{symbol} mid price: {ethTicker.MidPrice}");

// ---- 2. SET LEVERAGE ----
var leverage = await client.FuturesApi.Trading.SetLeverageAsync(
    symbol: symbol,
    leverage: 5,
    marginType: MarginType.Cross);

if (!leverage.Success)
{
    Console.WriteLine($"Failed to set leverage: {leverage.Error}");
    return;
}

Console.WriteLine($"Leverage updated for {symbol}");

// ---- 3. PLACE MARKET ORDER (open long position) ----
// Market orders still require a price. HyperLiquid uses it for max slippage calculation.
var openOrder = await client.FuturesApi.Trading.PlaceOrderAsync(
    symbol: symbol,
    side: OrderSide.Buy,
    orderType: OrderType.Market,
    quantity: 0.01m,
    price: ethTicker.MidPrice ?? 3000m);

if (!openOrder.Success)
{
    Console.WriteLine($"Failed to open position: {openOrder.Error}");
    return;
}

Console.WriteLine($"Opened position via order {openOrder.Data.OrderId}");

// ---- 4. GET CURRENT POSITION ----
var account = await client.FuturesApi.Account.GetAccountInfoAsync();
if (!account.Success)
{
    Console.WriteLine($"Failed to get account: {account.Error}");
    return;
}

var position = account.Data.Positions
    .Select(x => x.Position)
    .FirstOrDefault(x => x.Symbol == symbol && x.PositionQuantity != 0);

if (position == null)
{
    Console.WriteLine("No open position found (order may not have filled yet).");
    return;
}

Console.WriteLine($"Position: {position.PositionQuantity} {symbol} at avg {position.AverageEntryPrice}");
Console.WriteLine($"Unrealized PnL: {position.UnrealizedPnl}");
Console.WriteLine($"Liquidation price: {position.LiquidationPrice}");

// ---- 5. CLOSE THE POSITION ----
var closeSide = position.PositionQuantity > 0 ? OrderSide.Sell : OrderSide.Buy;
var closeOrder = await client.FuturesApi.Trading.PlaceOrderAsync(
    symbol: symbol,
    side: closeSide,
    orderType: OrderType.Market,
    quantity: Math.Abs(position.PositionQuantity ?? 0),
    price: ethTicker.MidPrice ?? position.AverageEntryPrice ?? 3000m,
    reduceOnly: true);

if (closeOrder.Success)
{
    Console.WriteLine($"Closed position via order {closeOrder.Data.OrderId}");
}

// Common variations:
//   Limit order:       orderType: OrderType.Limit, add timeInForce
//   Stop market:       orderType: OrderType.StopMarket, add triggerPrice + tpSlType
//   Isolated margin:   SetLeverageAsync(symbol, leverage, MarginType.Isolated)
//   Add/remove margin: FuturesApi.Trading.UpdateIsolatedMarginAsync(symbol, updateValue)
//   HIP-3 DEX:         pass dex to supported FuturesApi ExchangeData/Account methods
