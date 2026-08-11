# ![HyperLiquid.Net](https://raw.githubusercontent.com/JKorf/HyperLiquid.Net/main/HyperLiquid.Net/Icon/icon.png) HyperLiquid.Net  

[![.NET](https://img.shields.io/github/actions/workflow/status/JKorf/HyperLiquid.Net/dotnet.yml?style=for-the-badge)](https://github.com/JKorf/HyperLiquid.Net/actions/workflows/dotnet.yml) ![License](https://img.shields.io/github/license/JKorf/HyperLiquid.Net?style=for-the-badge)
![Since](https://img.shields.io/badge/since-2025-brightgreen?style=for-the-badge)

[![Docs](https://img.shields.io/badge/Docs-HyperLiquid.Net-1b7f50?style=for-the-badge)](https://cryptoexchange.jkorf.dev/docs/exchange-clients?library=HyperLiquid.Net)

HyperLiquid.Net is a client library for accessing the [HyperLiquid DEX REST and Websocket API](https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api). 

## Features
* Response data is mapped to descriptive models
* Input parameters and response values are mapped to discriptive enum values where possible
* High performance
* Automatic websocket (re)connection management 
* Client side rate limiting 
* Client side order book implementation
* Support for managing different accounts
* Extensive logging
* Support for different environments
* Easy integration with other exchange client based on the CryptoExchange.Net base library
* Native AOT support

## Documentation

The [HyperLiquid.Net documentation](https://cryptoexchange.jkorf.dev/docs/exchange-clients?library=HyperLiquid.Net) is the main resource for installing, configuring, and using the library.

| Resource | Description |
|--|--|
| [Client guide](https://cryptoexchange.jkorf.dev/docs/exchange-clients?library=HyperLiquid.Net) | Installation, REST and WebSocket clients, authentication, dependency injection, error handling, and advanced features |
| [Examples](https://cryptoexchange.jkorf.dev/docs/exchange-clients/examples?library=HyperLiquid.Net) | Common REST and WebSocket operations |
| [API reference](https://cryptoexchange.jkorf.dev/docs/exchange-clients/reference?library=HyperLiquid.Net) | Client interfaces, methods, and properties |
| [Shared API guide](https://cryptoexchange.jkorf.dev/docs/shared-api) | Common interfaces and models for working with multiple exchanges |

## Supported Frameworks
The library is targeting both `.NET Standard 2.0` and `.NET Standard 2.1` for optimal compatibility, as well as the latest dotnet versions to use the latest framework features.

|.NET implementation|Version Support|
|--|--|
|.NET Core|`2.0` and higher|
|.NET Framework|`4.6.1` and higher|
|Mono|`5.4` and higher|
|Xamarin.iOS|`10.14` and higher|
|Xamarin.Android|`8.0` and higher|
|UWP|`10.0.16299` and higher|
|Unity|`2018.1` and higher|

## Install the library

### NuGet 
[![NuGet version](https://img.shields.io/nuget/v/HyperLiquid.net.svg?style=for-the-badge)](https://www.nuget.org/packages/HyperLiquid.Net)  [![Nuget downloads](https://img.shields.io/nuget/dt/HyperLiquid.Net.svg?style=for-the-badge)](https://www.nuget.org/packages/HyperLiquid.Net)

	dotnet add package HyperLiquid.Net
	
### GitHub packages
HyperLiquid.Net is available on [GitHub packages](https://github.com/JKorf/HyperLiquid.Net/pkgs/nuget/HyperLiquid.Net). You'll need to add `https://nuget.pkg.github.com/JKorf/index.json` as a NuGet package source.

### Download release
[![GitHub Release](https://img.shields.io/github/v/release/JKorf/HyperLiquid.Net?style=for-the-badge&label=GitHub)](https://github.com/JKorf/HyperLiquid.Net/releases)

The NuGet package files are added along side the source with the latest GitHub release which can found [here](https://github.com/JKorf/HyperLiquid.Net/releases).

## How to use
The library uses `[BaseAsset]/[QuoteAsset]` notation for Spot symbols and `[BaseAsset]` for futures symbols. Futures symbols inherently have `USDC` as quote symbol.  
**Spot symbol**: `HYPE/USDC`  
**Futures symbol**: `HYPE` 

*Basic request:*  
```csharp	
var restClient = new HyperLiquidRestClient();

// Spot HYPE/USDC info
var spotTickerResult = await restClient.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync();
var hypeInfo = spotTickerResult.Data.Tickers.Single(x => x.Symbol == "HYPE/USDC");
var currentHypePrice = hypeInfo.MidPrice;

// Futures ETH perpetual contract info
var futuresTickerResult = await restClient.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync();
var ethInfo = futuresTickerResult.Data.Tickers.Single(x => x.Symbol == "ETH");
var currentEthPrice = ethInfo.MidPrice;
```

*Place order:*
```csharp
var restClient = new HyperLiquidRestClient(opts => {
	opts.ApiCredentials = new HyperLiquidCredentials("PUBLICKEY", "PRIVATEKEY");
});

// Place Limit order to go long for 0.1 ETH at 2000
var orderResult = await restClient.FuturesApi.Trading.PlaceOrderAsync(
    "ETH",
    OrderSide.Buy,
    OrderType.Limit,
    0.1m,
    2000
    );
```

*WebSocket subscription:*
```csharp
// Subscribe to HYPE/USDC Spot ticker updates via the websocket API
var socketClient = new HyperLiquidSocketClient();
var tickerSubscriptionResult = await hyperLiquidSocketClient.SpotApi.SubscribeToSymbolUpdatesAsync("HYPE/USDC", (update) =>
{
	var lastPrice = update.Data.MidPrice;
});
```

For more examples and explanations, continue with the [HyperLiquid.Net documentation](https://cryptoexchange.jkorf.dev/docs/exchange-clients?library=HyperLiquid.Net) or browse the [compilable repository examples](https://github.com/JKorf/HyperLiquid.Net/tree/main/Examples).

**NOTE**  
HyperLiquid.Net uses the Builder Code mechanism for HyperLiquid, which means that an additional 1bps / 0.01% fee is charged on top of orders placed with the library to fund development. This is entirely optional and can be disabled in the client options by setting `BuilderFeePercentage` to `0` or `null` in the client options.

## AI / LLM documentation

HyperLiquid.Net includes AI-oriented documentation and examples for code generation tools:

|File|Purpose|
|--|--|
|[`AGENTS.md`](AGENTS.md)|Assistant skill with core HyperLiquid.Net patterns, pitfalls, and examples|
|[`llms.txt`](llms.txt)|Short LLM index with links to docs, examples, and critical usage rules|
|[`llms-full.txt`](llms-full.txt)|Detailed LLM context with endpoint routing, code patterns, and anti-hallucination checks|
|[`docs/ai-api-map.md`](docs/ai-api-map.md)|Table-style intent-to-method map for Spot, Futures, WebSocket, and SharedApis|
|[`Examples/ai-friendly`](Examples/ai-friendly)|Compilable single-file examples for common REST, WebSocket, shared API, and error handling workflows|

See [cryptoexchange-skills-hub](https://github.com/JKorf/cryptoexchange-skills-hub) for installable skills.

## Shared / unified API

The CryptoExchange.Net [Shared APIs](https://cryptoexchange.jkorf.dev/docs/shared-api) provide exchange-agnostic, unified interfaces for common operations such as retrieving tickers, order books and balances, placing orders, and subscribing to market updates.

This allows the same application code to work with different exchange libraries. The supported HyperLiquid API surfaces expose their shared functionality through a `SharedClient` property. Because support differs between exchanges and API surfaces, call `Discover()` to inspect the available trading modes, environments, endpoints, and subscriptions at runtime.

### Supported shared interfaces

| API | Type | Supported interfaces |
|--|--|--|
| `SpotApi` | REST | `IAssetsRestClient`, `IBalanceRestClient`, `IBookTickerRestClient`, `IFeeRestClient`, `IKlineRestClient`, `IOrderBookRestClient`, `ISpotOrderClientIdRestClient`, `ISpotOrderRestClient`, `ISpotSymbolRestClient`, `ISpotTickerRestClient`, `ITransferRestClient`, `IWithdrawRestClient` |
| `SpotApi` | WebSocket | `IBalanceSocketClient`, `IBookTickerSocketClient`, `IKlineSocketClient`, `IOrderBookSocketClient`, `ISpotOrderSocketClient`, `ITickerSocketClient`, `ITradeSocketClient`, `IUserTradeSocketClient` |
| `FuturesApi` | REST | `IBalanceRestClient`, `IBookTickerRestClient`, `IFeeRestClient`, `IFundingRateRestClient`, `IFuturesOrderClientIdRestClient`, `IFuturesOrderRestClient`, `IFuturesSymbolRestClient`, `IFuturesTickerRestClient`, `IFuturesTpSlRestClient`, `IKlineRestClient`, `ILeverageRestClient`, `IOpenInterestRestClient`, `IOrderBookRestClient` |
| `FuturesApi` | WebSocket | `IBalanceSocketClient`, `IBookTickerSocketClient`, `IFuturesOrderSocketClient`, `IKlineSocketClient`, `IOrderBookSocketClient`, `IPositionSocketClient`, `ITickerSocketClient`, `ITradeSocketClient`, `IUserTradeSocketClient` |

### Discover supported functionality

```csharp
var sharedClient = new HyperLiquidRestClient().SpotApi.SharedClient;
var clientInfo = sharedClient.Discover();

Console.WriteLine(clientInfo);
```

### Example

```csharp
using HyperLiquid.Net.Clients;
using CryptoExchange.Net.SharedApis;

var sharedClient = new HyperLiquidRestClient().SpotApi.SharedClient;
ISpotTickerRestClient tickerClient = sharedClient;

var symbol = new SharedSymbol(TradingMode.Spot, "HYPE", "USDC");
var result = await tickerClient.GetSpotTickerAsync(
    new GetTickerRequest(symbol));

if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}

Console.WriteLine(result.Data.LastPrice);
```

The request and response models belong to `CryptoExchange.Net.SharedApis`, so the same pattern can be used with another exchange's `SharedClient`.

## CryptoExchange.Net
HyperLiquid.Net is based on the [CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) base library. Other exchange API implementations based on the CryptoExchange.Net base library are available and follow the same logic.

CryptoExchange.Net also provides [shared access to different exchange APIs](https://cryptoexchange.jkorf.dev/docs/shared-api).

|Exchange|Repository|Nuget|
|--|--|--|
|Aster|[JKorf/Aster.Net](https://github.com/JKorf/Aster.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Aster.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Aster.Net)|
|Binance|[JKorf/Binance.Net](https://github.com/JKorf/Binance.Net)|[![Nuget version](https://img.shields.io/nuget/v/Binance.net.svg?style=flat-square)](https://www.nuget.org/packages/Binance.Net)|
|BingX|[JKorf/BingX.Net](https://github.com/JKorf/BingX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.BingX.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.BingX.Net)|
|Bitfinex|[JKorf/Bitfinex.Net](https://github.com/JKorf/Bitfinex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bitfinex.net.svg?style=flat-square)](https://www.nuget.org/packages/Bitfinex.Net)|
|Bitget|[JKorf/Bitget.Net](https://github.com/JKorf/Bitget.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.Bitget.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.Bitget.Net)|
|BitMart|[JKorf/BitMart.Net](https://github.com/JKorf/BitMart.Net)|[![Nuget version](https://img.shields.io/nuget/v/BitMart.net.svg?style=flat-square)](https://www.nuget.org/packages/BitMart.Net)|
|BitMEX|[JKorf/BitMEX.Net](https://github.com/JKorf/BitMEX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.BitMEX.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.BitMEX.Net)|
|Bitstamp|[JKorf/Bitstamp.Net](https://github.com/JKorf/Bitstamp.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bitstamp.Net.svg?style=flat-square)](https://www.nuget.org/packages/Bitstamp.Net)|
|BloFin|[JKorf/BloFin.Net](https://github.com/JKorf/BloFin.Net)|[![Nuget version](https://img.shields.io/nuget/v/BloFin.net.svg?style=flat-square)](https://www.nuget.org/packages/BloFin.Net)|
|Bybit|[JKorf/Bybit.Net](https://github.com/JKorf/Bybit.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bybit.net.svg?style=flat-square)](https://www.nuget.org/packages/Bybit.Net)|
|Coinbase|[JKorf/Coinbase.Net](https://github.com/JKorf/Coinbase.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Coinbase.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Coinbase.Net)|
|CoinEx|[JKorf/CoinEx.Net](https://github.com/JKorf/CoinEx.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinEx.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinEx.Net)|
|CoinGecko|[JKorf/CoinGecko.Net](https://github.com/JKorf/CoinGecko.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinGecko.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinGecko.Net)|
|CoinW|[JKorf/CoinW.Net](https://github.com/JKorf/CoinW.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinW.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinW.Net)|
|Crypto.com|[JKorf/CryptoCom.Net](https://github.com/JKorf/CryptoCom.Net)|[![Nuget version](https://img.shields.io/nuget/v/CryptoCom.net.svg?style=flat-square)](https://www.nuget.org/packages/CryptoCom.Net)|
|DeepCoin|[JKorf/DeepCoin.Net](https://github.com/JKorf/DeepCoin.Net)|[![Nuget version](https://img.shields.io/nuget/v/DeepCoin.net.svg?style=flat-square)](https://www.nuget.org/packages/DeepCoin.Net)|
|Gate.io|[JKorf/GateIo.Net](https://github.com/JKorf/GateIo.Net)|[![Nuget version](https://img.shields.io/nuget/v/GateIo.net.svg?style=flat-square)](https://www.nuget.org/packages/GateIo.Net)|
|HTX|[JKorf/HTX.Net](https://github.com/JKorf/HTX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.HTX.net.svg?style=flat-square)](https://www.nuget.org/packages/Jkorf.HTX.Net)|
|Kraken|[JKorf/Kraken.Net](https://github.com/JKorf/Kraken.Net)|[![Nuget version](https://img.shields.io/nuget/v/KrakenExchange.net.svg?style=flat-square)](https://www.nuget.org/packages/KrakenExchange.Net)|
|Kucoin|[JKorf/Kucoin.Net](https://github.com/JKorf/Kucoin.Net)|[![Nuget version](https://img.shields.io/nuget/v/Kucoin.net.svg?style=flat-square)](https://www.nuget.org/packages/Kucoin.Net)|
|LBank|[JKorf/LBank.Net](https://github.com/JKorf/LBank.Net)|[![Nuget version](https://img.shields.io/nuget/v/LBank.net.svg?style=flat-square)](https://www.nuget.org/packages/LBank.Net)|
|Lighter|[JKorf/Lighter.Net](https://github.com/JKorf/Lighter.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Lighter.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Lighter.Net)|
|Mexc|[JKorf/Mexc.Net](https://github.com/JKorf/Mexc.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.Mexc.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.Mexc.Net)|
|OKX|[JKorf/OKX.Net](https://github.com/JKorf/OKX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.OKX.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.OKX.Net)|
|Pionex|[JKorf/Pionex.Net](https://github.com/JKorf/Pionex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Pionex.net.svg?style=flat-square)](https://www.nuget.org/packages/Pionex.Net)|
|Polymarket|[JKorf/Polymarket.Net](https://github.com/JKorf/Polymarket.Net)|[![Nuget version](https://img.shields.io/nuget/v/Polymarket.net.svg?style=flat-square)](https://www.nuget.org/packages/Polymarket.Net)|
|Toobit|[JKorf/Toobit.Net](https://github.com/JKorf/Toobit.Net)|[![Nuget version](https://img.shields.io/nuget/v/Toobit.net.svg?style=flat-square)](https://www.nuget.org/packages/Toobit.Net)|
|Upbit|[JKorf/Upbit.Net](https://github.com/JKorf/Upbit.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Upbit.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Upbit.Net)|
|Weex|[JKorf/Weex.Net](https://github.com/JKorf/Weex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Weex.net.svg?style=flat-square)](https://www.nuget.org/packages/Weex.Net)|
|WhiteBit|[JKorf/WhiteBit.Net](https://github.com/JKorf/WhiteBit.Net)|[![Nuget version](https://img.shields.io/nuget/v/WhiteBit.net.svg?style=flat-square)](https://www.nuget.org/packages/WhiteBit.Net)|
|XT|[JKorf/XT.Net](https://github.com/JKorf/XT.Net)|[![Nuget version](https://img.shields.io/nuget/v/XT.net.svg?style=flat-square)](https://www.nuget.org/packages/XT.Net)|

When using multiple of these API's the [CryptoClients.Net](https://github.com/JKorf/CryptoClients.Net) package can be used which combines this and the other packages and allows easy access to all exchange API's.

## Discord
[![Nuget version](https://img.shields.io/discord/847020490588422145?style=for-the-badge)](https://discord.gg/MSpeEtSY8t)  
A Discord server is available [here](https://discord.gg/MSpeEtSY8t). For discussion and/or questions around the CryptoExchange.Net and implementation libraries, feel free to join.

## OSX Support
The signing method used in the library is not natively supported on OSX. Because of this a custom signing method has to be provided or a `PlatformNotSupported` exception will be thrown while trying to sign a request.

A custom signing method can be provided using `HyperLiquidExchange.SignRequestDelegate = CustomSigningMethod;`.  
To run on OSX the `Nethereum.Signer.EIP712` package can be installed with the following custom signing method:
```csharp
Dictionary<string, object> Sign(string request, string secret)
{
    var messageBytes = Convert.FromHexString(request);
    var sign = new MessageSigner().SignAndCalculateV(messageBytes, new EthECKey(secret));
    return new Dictionary<string, object>()
            {
                { "r", "0x" + Convert.ToHexString(sign.R).ToLowerInvariant() },
                { "s", "0x" + Convert.ToHexString(sign.S).ToLowerInvariant() },
                { "v", (int)sign.V[0] }
            };
}
```

## Supported functionality

### Rest & WebSocket
*Requests are available on both the WebSocket and REST client*  

|API|Supported|Location|
|--|--:|--|
|Info|✓|`restClient.SpotApi.Account` / `restClient.SpotApi.ExchangeData` / `restClient.SpotApi.Trading` `restClient.FuturesApi.Account` / `restClient.FuturesApi.ExchangeData` / `restClient.FuturesApi.Trading`|
|Info Perpetuals|✓|`restClient.FuturesApi.Account` / `restClient.FuturesApi.ExchangeData`|
|Info Spot|✓|`restClient.SpotApi.Account` / `restClient.SpotApi.ExchangeData`|
|Exchange|✓|`restClient.SpotApi.Account` / `restClient.SpotApi.Trading` `restClient.FuturesApi.Account` / `restClient.FuturesApi.Trading`|

### Websocket streams
|API|Supported|Location|
|--|--:|--|
|*|✓|`socketClient.SpotApi` / `socketClient.FuturesApi`|

## Support the project
Any support is greatly appreciated.

### Referral
If you do not yet have an account please consider using this referal link to sign up:
[Link](https://app.hyperliquid.xyz/join/JKORF)  
Not only will you support development at no cost, you also get a 4% discount in fees.

### Donate
Make a one time donation in a crypto currency of your choice. If you prefer to donate in a different currency or network send me a message.
   
**USDT (TRX)**  TKigKeJPXZYyMVDgMyXxMf17MWYia92Rjd 

### Sponsor
Alternatively, sponsor me on Github using [Github Sponsors](https://github.com/sponsors/JKorf). 

## Release notes
* Version 5.4.0 - 29 Jul 2026
    * Updated CryptoExchange.Net to version 12.4.0
    * Added calculation of AveragePrice on Shared order models if data is available and AveragePrice is not set
    * Added DebuggerDisplay attributes to Result models
    * Added AveragePrice property to SharedQuantity model
    * Updated SharedFuturesTicker, SharedSpotTicker, SharedTrade and SharedKline to use SharedOrderQuantity for volumes/quantities

* Version 5.3.0 - 21 Jul 2026
    * Updated CryptoExchange.Net to v12.2.0 
    * Added SpotSymbolCatalog to Shared ISpotSymbolRestClient interface
    * Added FuturesSymbolCatalog to Shared IFuturesSymbolRestClient interface
    * Added BaseAssetType, BaseAssetSubType, QuoteAssetType and QuoteAssetSubType to GetSymbolsRequest model
    * Added DisplayName to SharedSpotSymbol and SharedFuturesSymbol models
    * Added BaseAssetType, BaseAssetSubType, QuoteAssetType and QuoteAssetSubType to SharedSpotSymbol and SharedFuturesSymbol models
    * Added DebuggerDisplay attributes to Shared models
    * Added Account.GetUserPortfolioAsync endpoint
    * Added restClient.FuturesApi.ExchangeData.GetPerpAnnotationAsync endpoint
    * Added restClient.FuturesApi.ExchangeData.GetPerpCategoriesAsync endpoint
    * Added restClient.FuturesApi.ExchangeData.GetPerpConciseAnnotationsAsync endpoint
    * Updated client.SpotApi.Account.GetSubAccounts2Async response model
    * Fixed deserialization issue websocket responses using HyperLiquidDefault type

* Version 5.2.1 - 13 Jul 2026
    * Fixed deserialization for EditOrderAsync, ToggleHip3DexAbstractionAsync, SetLeverageAsync and UpdateIsolatedMarginAsync websocket requests when response is an error

* Version 5.2.0 - 09 Jul 2026
    * Updated CryptoExchange.Net to v12.1.0

* Version 5.1.0 - 07 Jul 2026
    * Added restClient.SpotApi.Account.GetSubAccounts2Async endpoint
    * Added CollateralTokenIndex and CollateralToken to HyperLiquidFuturesDexInfo model
    * Added ClientOrderId to HyperLiquidUserTrade model
    * Added DeployerTradingFeeShare to HyperLiquidAsset model
    * Added RequestSurplus to HyperLiquidRateLimit model
    * Updated futures GetExchangeInfoAsync response type to HyperLiquidFuturesDexInfo
    * Fixed EditOrdersAsync using ClientOrderId instead of NewClientOrderId for the replacement order

* Version 5.0.0 - 29 Jun 2026
    * Result types:
      * (Web)CallResult types are replaced by HttpResult, WebSocketResult and QueryResult with the same logic
      * WebSocketResult and QueryResult now return additional info for websocket operations
      * Updated result types to record type
      * Removed implicit result type conversion to bool, `if (result)` no longer works, instead use `if (result.Success)`
      * Fixed result object nullability hinting, for example Data might be null if Success isn't checked for true
    * Clients:
      * Added ToString overrides on base API types
      * Added Exchange property on BaseApiClient
      * Added ApiCredentials property on Api clients
      * Updated ILogger source from client name to topic specific client name
      * Removed logging from client creation
      * Fixed issue in SocketApiClient.GetSocketConnection causing requests to always wait the full max 10 seconds when there was a reconnecting socket
    * Shared APIs:
      * Added missing dedicated option types
      * Added Discover method on ISharedClient interface, returning info on supported capabilities and operations
      * Added ResetStaticExchangeParameters method on ExchangeParameters
      * Added Status property to SharedWithdrawal model
      * Added TradingModes property to SharedBalance model
      * Updated Shared ExchangeParameters parameter names to be case insensitive
      * Updated code comments
      * Replaced ExchangeResult with ExchangeCallResult type
      * Removed TradingMode from the response model, only maintained on models where it makes sense
    * Added alwaysPlace parameter to EditOrderAsync endpoints
    * Added async streaming on UserDataTracker items with StreamUpdatesAsync
    * Added cancellation token support to UserDataTracker starting
    * Added SupportedEnvironments property to PlatformInfo
    * Added setter to HyperLiquidExchange.RateLimiter to allow custom rate limit settings
    * Added Clear() method on UserClientProvider to clear all cached clients
    * Updated HyperLiquidPositionUpdate internal Data type
    * Various small performance improvements
    * Marked SubscribToUserUpdatesAsync as obsolete
    * Fixed websocket connection attempts counting towards rate limit even when server could not be reached
    * Fixed websocket user data update filtering based on address
