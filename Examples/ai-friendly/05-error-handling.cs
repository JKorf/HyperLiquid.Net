// 05-error-handling.cs
//
// Demonstrates: HttpResult patterns, retry logic, symbol formatting,
// builder fee checks, and common error categories.
//
// Setup: dotnet add package HyperLiquid.Net

using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Enums;

var client = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials("PUBLIC_ADDRESS", "PRIVATE_KEY");
});

// ---- 1. THE BASIC PATTERN ----
// REST methods return HttpResult<T> or HttpResult.
// WebSocket methods return WebSocketResult<T> or WebSocketResult.
// .Data is only safe to read when .Success is true.

var result = await client.FuturesApi.ExchangeData.GetPricesAsync();

if (result.Success)
{
    Console.WriteLine($"ETH perp mid: {result.Data["ETH"]}");
}
else
{
    Console.WriteLine($"Code:      {result.Error?.Code}");
    Console.WriteLine($"Message:   {result.Error?.Message}");
    Console.WriteLine($"Type:      {result.Error?.ErrorType}");
    Console.WriteLine($"Transient: {result.Error?.IsTransient}");
}

// ---- 2. SIMPLE RETRY WITH BACKOFF ----
// Retry only on transient errors. Do not retry validation or credential failures blindly.

async Task<HttpResult<T>> WithRetry<T>(
    Func<Task<HttpResult<T>>> call,
    int maxAttempts = 3)
{
    HttpResult<T> last = default!;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        last = await call();
        if (last.Success) return last;
        if (last.Error?.IsTransient != true) return last;

        await Task.Delay(TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt)));
    }

    return last;
}

var prices = await WithRetry(() => client.SpotApi.ExchangeData.GetPricesAsync());
if (prices.Success)
{
    Console.WriteLine($"HYPE/USDC mid: {prices.Data["HYPE/USDC"]}");
}

// ---- 3. SYMBOL FORMATTING ----
// Spot symbols use Base/Quote. Futures symbols use the base asset only.

var spotSymbol = HyperLiquidExchange.FormatSymbol("HYPE", "USDC", TradingMode.Spot);
var futuresSymbol = HyperLiquidExchange.FormatSymbol("ETH", "USDC", TradingMode.PerpetualLinear);

Console.WriteLine($"Spot symbol: {spotSymbol}");
Console.WriteLine($"Futures symbol: {futuresSymbol}");

// ---- 4. BUILDER FEE CHECK ----
// HyperLiquid.Net enables builder code by default. The default fee is 1 bps / 0.01%.
// Trading code can check whether the account approved the configured builder fee.

var approvedFee = await client.SpotApi.Account.GetApprovedBuilderFeeAsync();
if (!approvedFee.Success)
{
    Console.WriteLine($"Could not read approved builder fee: {approvedFee.Error}");
}
else
{
    Console.WriteLine($"Approved builder fee: {approvedFee.Data}");
}

// To disable builder fee for a client:
var noBuilderFeeClient = new HyperLiquidRestClient(options =>
{
    options.ApiCredentials = new HyperLiquidCredentials("PUBLIC_ADDRESS", "PRIVATE_KEY");
    options.BuilderFeePercentage = 0;
});

// ---- 5. ORDER VALIDATION CATEGORIES ----
// HyperLiquid market orders still need a price. Use a recent mid price from GetPricesAsync
// or ExchangeData ticker metadata so slippage calculation has an anchor.

var latestPrices = await noBuilderFeeClient.FuturesApi.ExchangeData.GetPricesAsync();
if (!latestPrices.Success)
{
    Console.WriteLine($"Cannot fetch prices, aborting order: {latestPrices.Error}");
    return;
}

var order = await noBuilderFeeClient.FuturesApi.Trading.PlaceOrderAsync(
    symbol: "ETH",
    side: OrderSide.Buy,
    orderType: OrderType.Market,
    quantity: 0.01m,
    price: latestPrices.Data["ETH"]);

if (!order.Success)
{
    var category = order.Error?.IsTransient == true
        ? "Transient - retry with backoff may be appropriate"
        : "Permanent - fix inputs, credentials, symbol, or account state";

    Console.WriteLine($"{category}: {order.Error}");
}

// Common error scenarios:
//   Unknown symbol:    use HYPE/USDC for spot and ETH for futures
//   Credential error:  verify HyperLiquidCredentials public address/private key
//   Order rejected:    check quantity, price, timeInForce, reduceOnly, trigger parameters
//   Builder approval:  approve builder fee or set BuilderFeePercentage to 0 when intended
//   Rate limit/server: retry only if result.Error?.IsTransient == true
