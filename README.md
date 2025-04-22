# ![HyperLiquid.Net](https://raw.githubusercontent.com/JKorf/HyperLiquid.Net/main/HyperLiquid.Net/Icon/icon.png) HyperLiquid.Net  

[![.NET](https://img.shields.io/github/actions/workflow/status/JKorf/HyperLiquid.Net/dotnet.yml?style=for-the-badge)](https://github.com/JKorf/HyperLiquid.Net/actions/workflows/dotnet.yml) ![License](https://img.shields.io/github/license/JKorf/HyperLiquid.Net?style=for-the-badge)

HyperLiquid.Net is a client library for accessing the [HyperLiquid DEX REST and Websocket API](https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api). 

## Features
* Response data is mapped to descriptive models
* Input parameters and response values are mapped to discriptive enum values where possible
* Automatic websocket (re)connection management 
* Client side rate limiting 
* Client side order book implementation
* Extensive logging
* Support for different environments
* Easy integration with other exchange client based on the CryptoExchange.Net base library
* Native AOT support

## Supported Frameworks
The library is targeting both `.NET Standard 2.0` and `.NET Standard 2.1` for optimal compatibility, as well as dotnet 8.0 and 9.0 to use the latest framework features.

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

* REST Endpoints
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
* Websocket streams
	```csharp
	// Subscribe to HYPE/USDC Spot ticker updates via the websocket API
	var socketClient = new HyperLiquidSocketClient();
	var tickerSubscriptionResult = await hyperLiquidSocketClient.SpotApi.SubscribeToSymbolUpdatesAsync("HYPE/USDC", (update) =>
	{
		var lastPrice = update.Data.MidPrice;
	});
	```

For information on the clients, dependency injection, response processing and more see the [documentation](https://jkorf.github.io/CryptoExchange.Net), or have a look at the examples [here](https://github.com/JKorf/HyperLiquid.Net/tree/main/Examples) or [here](https://github.com/JKorf/CryptoExchange.Net/tree/master/Examples).

## CryptoExchange.Net
HyperLiquid.Net is based on the [CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net) base library. Other exchange API implementations based on the CryptoExchange.Net base library are available and follow the same logic.

CryptoExchange.Net also allows for [easy access to different exchange API's](https://jkorf.github.io/CryptoExchange.Net#idocs_shared).

|Exchange|Repository|Nuget|
|--|--|--|
|Binance|[JKorf/Binance.Net](https://github.com/JKorf/Binance.Net)|[![Nuget version](https://img.shields.io/nuget/v/Binance.net.svg?style=flat-square)](https://www.nuget.org/packages/Binance.Net)|
|BingX|[JKorf/BingX.Net](https://github.com/JKorf/BingX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.BingX.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.BingX.Net)|
|Bitfinex|[JKorf/Bitfinex.Net](https://github.com/JKorf/Bitfinex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bitfinex.net.svg?style=flat-square)](https://www.nuget.org/packages/Bitfinex.Net)|
|Bitget|[JKorf/Bitget.Net](https://github.com/JKorf/Bitget.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.Bitget.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.Bitget.Net)|
|BitMart|[JKorf/BitMart.Net](https://github.com/JKorf/BitMart.Net)|[![Nuget version](https://img.shields.io/nuget/v/BitMart.net.svg?style=flat-square)](https://www.nuget.org/packages/BitMart.Net)|
|BitMEX|[JKorf/BitMEX.Net](https://github.com/JKorf/BitMEX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.BitMEX.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.BitMEX.Net)|
|Bybit|[JKorf/Bybit.Net](https://github.com/JKorf/Bybit.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bybit.net.svg?style=flat-square)](https://www.nuget.org/packages/Bybit.Net)|
|Coinbase|[JKorf/Coinbase.Net](https://github.com/JKorf/Coinbase.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Coinbase.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Coinbase.Net)|
|CoinEx|[JKorf/CoinEx.Net](https://github.com/JKorf/CoinEx.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinEx.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinEx.Net)|
|CoinGecko|[JKorf/CoinGecko.Net](https://github.com/JKorf/CoinGecko.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinGecko.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinGecko.Net)|
|Crypto.com|[JKorf/CryptoCom.Net](https://github.com/JKorf/CryptoCom.Net)|[![Nuget version](https://img.shields.io/nuget/v/CryptoCom.net.svg?style=flat-square)](https://www.nuget.org/packages/CryptoCom.Net)|
|DeepCoin|[JKorf/DeepCoin.Net](https://github.com/JKorf/DeepCoin.Net)|[![Nuget version](https://img.shields.io/nuget/v/DeepCoin.net.svg?style=flat-square)](https://www.nuget.org/packages/DeepCoin.Net)|
|Gate.io|[JKorf/GateIo.Net](https://github.com/JKorf/GateIo.Net)|[![Nuget version](https://img.shields.io/nuget/v/GateIo.net.svg?style=flat-square)](https://www.nuget.org/packages/GateIo.Net)|
|HTX|[JKorf/HTX.Net](https://github.com/JKorf/HTX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.HTX.net.svg?style=flat-square)](https://www.nuget.org/packages/Jkorf.HTX.Net)|
|Kraken|[JKorf/Kraken.Net](https://github.com/JKorf/Kraken.Net)|[![Nuget version](https://img.shields.io/nuget/v/KrakenExchange.net.svg?style=flat-square)](https://www.nuget.org/packages/KrakenExchange.Net)|
|Kucoin|[JKorf/Kucoin.Net](https://github.com/JKorf/Kucoin.Net)|[![Nuget version](https://img.shields.io/nuget/v/Kucoin.net.svg?style=flat-square)](https://www.nuget.org/packages/Kucoin.Net)|
|Mexc|[JKorf/Mexc.Net](https://github.com/JKorf/Mexc.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.Mexc.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.Mexc.Net)|
|OKX|[JKorf/OKX.Net](https://github.com/JKorf/OKX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.OKX.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.OKX.Net)|
|WhiteBit|[JKorf/WhiteBit.Net](https://github.com/JKorf/WhiteBit.Net)|[![Nuget version](https://img.shields.io/nuget/v/WhiteBit.net.svg?style=flat-square)](https://www.nuget.org/packages/WhiteBit.Net)|
|XT|[JKorf/XT.Net](https://github.com/JKorf/XT.Net)|[![Nuget version](https://img.shields.io/nuget/v/XT.net.svg?style=flat-square)](https://www.nuget.org/packages/XT.Net)|

When using multiple of these API's the [CryptoClients.Net](https://github.com/JKorf/CryptoClients.Net) package can be used which combines this and the other packages and allows easy access to all exchange API's.

## Discord
[![Nuget version](https://img.shields.io/discord/847020490588422145?style=for-the-badge)](https://discord.gg/MSpeEtSY8t)  
A Discord server is available [here](https://discord.gg/MSpeEtSY8t). For discussion and/or questions around the CryptoExchange.Net and implementation libraries, feel free to join.

## Supported functionality

### Rest
|API|Supported|Location|
|--|--:|--|
|Info|✓|`restClient.SpotApi.Account` / `restClient.SpotApi.ExchangeData` / `restClient.SpotApi.Trading` `restClient.FuturesApi.Account` / `restClient.FuturesApi.ExchangeData` / `restClient.FuturesApi.Trading`|
|Info Perpetuals|✓|`restClient.FuturesApi.Account` / `restClient.FuturesApi.ExchangeData`|
|Info Spot|✓|`restClient.SpotApi.Account` / `restClient.SpotApi.ExchangeData`|
|Exchange|✓|`restClient.SpotApi.Account` / `restClient.SpotApi.Trading` `restClient.FuturesApi.Account` / `restClient.FuturesApi.Trading`|

### Websocket
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
Make a one time donation in a crypto currency of your choice. If you prefer to donate a currency not listed here please contact me.

**Btc**:  bc1q277a5n54s2l2mzlu778ef7lpkwhjhyvghuv8qf  
**Eth**:  0xcb1b63aCF9fef2755eBf4a0506250074496Ad5b7   
**USDT (TRX)**  TKigKeJPXZYyMVDgMyXxMf17MWYia92Rjd 

### Sponsor
Alternatively, sponsor me on Github using [Github Sponsors](https://github.com/sponsors/JKorf). 

## Release notes
* Version 2.0.0-beta1 - 22 Apr 2025
    * Updated CryptoExchange.Net to version 9.0.0-beta1, see https://github.com/JKorf/CryptoExchange.Net/releases/
    * Added support for Native AOT compilation
    * Added RateLimitUpdated event
    * Added SharedSymbol response property to all Shared interfaces response models returning a symbol name
    * Added GenerateClientOrderId method to FuturesApi and Spot Shared clients
    * Added IBookTickerRestClient implementation to SpotApi and FuturesApi Shared clients
    * Added IFuturesOrderClientIdClient implementation to FuturesApi Shared client
    * Added IFuturesTpSlRestClient implementation to FuturesApi Shared client
    * Added ISpotOrderClientIdClient implementation to FuturesApi Shared client
    * Added TriggerPrice, IsTriggerOrder properties to SharedSpotOrder model
    * Added TriggerPrice, IsTriggerOrder properties to SharedFuturesOrder model
    * Added MaxLongLeverage, MaxShortLeverage to SharedFuturesSymbol model
    * Added OptionalExchangeParameters and Supported properties to EndpointOptions
    * Added better error handling for unknown symbols
    * Added vaultAddress parameter to various endpoints
    * Combined rest PlaceOrderAsync and PlaceTriggerOrderAsync methods for more flexibility
    * Refactored Shared clients quantity parameters and responses to use SharedQuantity
    * Updated all IEnumerable response and model types to array response types
    * Removed Newtonsoft.Json dependency
    * Removed Nethereum dependency
    * Removed legacy AddHyperLiquid(restOptions, socketOptions) DI overload
    * Fixed some typos
    * Fixed symbol name conversion not respecting environment setting
    * Fixed DivideByZero exception in Shared ticker requests if PreviousDayPrice is 0
    * Fixed various signing issues and response parsing issues

* Version 1.1.2 - 28 Mar 2025
    * Fix testnet support

* Version 1.1.1 - 22 Mar 2025
    * Fixed deserialization of spot exchange info

* Version 1.1.0 - 11 Feb 2025
    * Updated CryptoExchange.Net to version 8.8.0, see https://github.com/JKorf/CryptoExchange.Net/releases/
    * Added support for more SharedKlineInterval values
    * Added setting of DataTime value on websocket DataEvent updates
    * Fix Mono runtime exception on rest client construction using DI

* Version 1.0.1 - 22 Jan 2025
    * Added DisplayName and ImageUrl to HyperLiquidExchange class
    * Update HyperLiquidOptions object to make use of LibraryOptions base class

* Version 1.0.0 - 21 Jan 2025
    * Initial release

