# HyperLiquid.Net AI API Quick Map

Use this file to route common user intents to the correct HyperLiquid.Net client member. If a method name or parameter is not listed here, inspect `HyperLiquid.Net/Interfaces/Clients/**` before generating code.

## Client Roots

| Intent | Use |
|---|---|
| REST calls | `new HyperLiquidRestClient()` |
| WebSocket streams and socket API requests | `new HyperLiquidSocketClient()` |
| API key authentication | `options.ApiCredentials = new HyperLiquidCredentials("public address", "private key")` |
| Live environment | `HyperLiquidEnvironment.Live` |
| Testnet environment | `HyperLiquidEnvironment.Testnet` |
| Dependency injection | `services.AddHyperLiquid(options => { ... })` |
| Disable builder fee | `options.BuilderFeePercentage = 0` |
| Format spot symbol | `HyperLiquidExchange.FormatSymbol("HYPE", "USDC", TradingMode.Spot)` |
| Format futures symbol | `HyperLiquidExchange.FormatSymbol("ETH", "USDC", TradingMode.PerpetualLinear)` |

## Spot REST

| User intent | HyperLiquid.Net member |
|---|---|
| Get all mid prices | `client.SpotApi.ExchangeData.GetPricesAsync()` |
| Get spot exchange info | `client.SpotApi.ExchangeData.GetExchangeInfoAsync()` |
| Get spot exchange info and tickers | `client.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync()` |
| Get spot asset info | `client.SpotApi.ExchangeData.GetAssetInfoAsync(assetId)` |
| Get spot order book | `client.SpotApi.ExchangeData.GetOrderBookAsync("HYPE/USDC")` |
| Get spot klines | `client.SpotApi.ExchangeData.GetKlinesAsync("HYPE/USDC", KlineInterval.OneMinute, start, end)` |
| Get spot balances | `client.SpotApi.Account.GetBalancesAsync()` |
| Get fee info | `client.SpotApi.Account.GetFeeInfoAsync()` |
| Get account ledger | `client.SpotApi.Account.GetAccountLedgerAsync(start, end)` |
| Get rate limits | `client.SpotApi.Account.GetRateLimitsAsync()` |
| Get approved builder fee | `client.SpotApi.Account.GetApprovedBuilderFeeAsync()` |
| Approve library builder fee | `client.SpotApi.Account.ApproveBuilderFeeAsync()` |
| Approve custom builder fee | `client.SpotApi.Account.ApproveBuilderFeeAsync(builderAddress, maxFeePercentage)` |
| Send spot asset | `client.SpotApi.Account.TransferSpotAsync(destinationAddress, asset, quantity)` |
| Send USD | `client.SpotApi.Account.TransferUsdAsync(destinationAddress, quantity)` |
| Withdraw USD | `client.SpotApi.Account.WithdrawAsync(destinationAddress, quantity)` |
| Transfer between spot and futures | `client.SpotApi.Account.TransferInternalAsync(direction, quantity)` |
| Get subaccounts | `client.SpotApi.Account.GetSubAccountsAsync()` |
| Get user role | `client.SpotApi.Account.GetUserRoleAsync()` |
| Get extra agents | `client.SpotApi.Account.GetExtraAgentsAsync()` |
| Get staking delegations | `client.SpotApi.Account.GetStakingDelegationsAsync()` |
| Get staking summary | `client.SpotApi.Account.GetStakingSummaryAsync()` |
| Get staking history | `client.SpotApi.Account.GetStakingHistoryAsync()` |
| Get staking rewards | `client.SpotApi.Account.GetStakingRewardsAsync()` |
| Deposit into staking | `client.SpotApi.Account.DepositIntoStakingAsync(wei)` |
| Withdraw from staking | `client.SpotApi.Account.WithdrawFromStakingAsync(wei)` |
| Delegate or undelegate stake | `client.SpotApi.Account.DelegateOrUndelegateStakeFromValidatorAsync(direction, validator, wei)` |
| Deposit or withdraw vault funds | `client.SpotApi.Account.DepositOrWithdrawFromVaultAsync(direction, vaultAddress, usd)` |
| Place spot order | `client.SpotApi.Trading.PlaceOrderAsync("HYPE/USDC", side, orderType, quantity, price, ...)` |
| Place multiple spot orders | `client.SpotApi.Trading.PlaceMultipleOrdersAsync(requests)` |
| Get spot/futures open orders | `client.SpotApi.Trading.GetOpenOrdersAsync()` |
| Get extended open orders | `client.SpotApi.Trading.GetOpenOrdersExtendedAsync()` |
| Get user trades | `client.SpotApi.Trading.GetUserTradesAsync()` |
| Get user trades by time | `client.SpotApi.Trading.GetUserTradesByTimeAsync(start, end)` |
| Get order by id or client id | `client.SpotApi.Trading.GetOrderAsync(orderId, clientOrderId)` |
| Get order history | `client.SpotApi.Trading.GetOrderHistoryAsync()` |
| Cancel spot order | `client.SpotApi.Trading.CancelOrderAsync("HYPE/USDC", orderId)` |
| Cancel multiple orders | `client.SpotApi.Trading.CancelOrdersAsync(requests)` |
| Cancel by client order id | `client.SpotApi.Trading.CancelOrderByClientOrderIdAsync("HYPE/USDC", clientOrderId)` |
| Dead man's switch | `client.SpotApi.Trading.CancelAfterAsync(timeout)` |
| Edit order | `client.SpotApi.Trading.EditOrderAsync(...)` |
| Edit multiple orders | `client.SpotApi.Trading.EditOrdersAsync(requests)` |
| Place TWAP order | `client.SpotApi.Trading.PlaceTwapOrderAsync(...)` |
| Cancel TWAP order | `client.SpotApi.Trading.CancelTwapOrderAsync(symbol, twapId)` |

## Futures REST

| User intent | HyperLiquid.Net member |
|---|---|
| Get all mid prices | `client.FuturesApi.ExchangeData.GetPricesAsync()` |
| Get perp DEXes | `client.FuturesApi.ExchangeData.GetPerpDexesAsync()` |
| Get futures exchange info | `client.FuturesApi.ExchangeData.GetExchangeInfoAsync()` |
| Get futures exchange info for HIP-3 DEX | `client.FuturesApi.ExchangeData.GetExchangeInfoAsync(dex)` |
| Get exchange info for all perp DEXes | `client.FuturesApi.ExchangeData.GetExchangeInfoAllDexesAsync()` |
| Get futures exchange info and tickers | `client.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync()` |
| Get funding rate history | `client.FuturesApi.ExchangeData.GetFundingRateHistoryAsync("ETH", start, end)` |
| Get symbols at max open interest | `client.FuturesApi.ExchangeData.GetSymbolsAtMaxOpenInterestAsync(dex)` |
| Get Perp DEX market limits | `client.FuturesApi.ExchangeData.GetPerpDexMarketLimitsAsync(dex)` |
| Get Perp DEX market status | `client.FuturesApi.ExchangeData.GetPerpDexMarketStatusAsync(dex)` |
| Get futures order book | `client.FuturesApi.ExchangeData.GetOrderBookAsync("ETH")` |
| Get futures klines | `client.FuturesApi.ExchangeData.GetKlinesAsync("ETH", KlineInterval.OneMinute, start, end)` |
| Get futures account and positions | `client.FuturesApi.Account.GetAccountInfoAsync()` |
| Get futures account for DEX | `client.FuturesApi.Account.GetAccountInfoAsync(dex: dex)` |
| Get funding history | `client.FuturesApi.Account.GetFundingHistoryAsync(start, end)` |
| Get user symbol state | `client.FuturesApi.Account.GetUserSymbolAsync("ETH")` |
| Get HIP-3 DEX abstraction state | `client.FuturesApi.Account.GetHip3DexAbstractionAsync()` |
| Toggle HIP-3 DEX abstraction | `client.FuturesApi.Account.ToggleHip3DexAbstractionAsync(enabled)` |
| Set leverage | `client.FuturesApi.Trading.SetLeverageAsync("ETH", leverage, MarginType.Cross)` |
| Update isolated margin | `client.FuturesApi.Trading.UpdateIsolatedMarginAsync("ETH", updateValue)` |
| Place futures order | `client.FuturesApi.Trading.PlaceOrderAsync("ETH", side, orderType, quantity, price, ...)` |
| Place reduce-only close order | `client.FuturesApi.Trading.PlaceOrderAsync("ETH", side, OrderType.Market, quantity, price, reduceOnly: true)` |
| Place futures trigger order | `client.FuturesApi.Trading.PlaceOrderAsync("ETH", side, orderType, quantity, price, triggerPrice: trigger, tpSlType: type)` |
| Place futures TWAP order | `client.FuturesApi.Trading.PlaceTwapOrderAsync("ETH", side, quantity, reduceOnly, minutes, randomize)` |

## Spot WebSocket

| User intent | HyperLiquid.Net member |
|---|---|
| Get prices over socket request API | `socketClient.SpotApi.ExchangeData.GetPricesAsync()` |
| Get spot exchange info over socket request API | `socketClient.SpotApi.ExchangeData.GetExchangeInfoAsync()` |
| Get spot exchange info and tickers over socket request API | `socketClient.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync()` |
| Get spot balances over socket request API | `socketClient.SpotApi.Account.GetBalancesAsync()` |
| Place spot order over socket request API | `socketClient.SpotApi.Trading.PlaceOrderAsync(...)` |
| Subscribe spot symbol updates | `socketClient.SpotApi.ExchangeData.SubscribeToSymbolUpdatesAsync("HYPE/USDC", handler)` |
| Subscribe spot balance updates | `socketClient.SpotApi.Account.SubscribeToBalanceUpdatesAsync(address, handler)` |
| Subscribe spot order book | `socketClient.SpotApi.ExchangeData.SubscribeToOrderBookUpdatesAsync("HYPE/USDC", handler)` |
| Subscribe spot klines | `socketClient.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync("HYPE/USDC", interval, handler)` |
| Subscribe spot trades | `socketClient.SpotApi.ExchangeData.SubscribeToTradeUpdatesAsync("HYPE/USDC", handler)` |
| Subscribe spot book ticker | `socketClient.SpotApi.ExchangeData.SubscribeToBookTickerUpdatesAsync("HYPE/USDC", handler)` |
| Subscribe order updates | `socketClient.SpotApi.Trading.SubscribeToOrderUpdatesAsync(address, handler)` |
| Subscribe open order updates | `socketClient.SpotApi.Trading.SubscribeToOpenOrderUpdatesAsync(address, dex, handler)` |
| Subscribe user trade updates | `socketClient.SpotApi.Trading.SubscribeToUserTradeUpdatesAsync(address, handler)` |
| Subscribe TWAP trade updates | `socketClient.SpotApi.Trading.SubscribeToTwapTradeUpdatesAsync(address, handler)` |
| Subscribe TWAP order updates | `socketClient.SpotApi.Trading.SubscribeToTwapOrderUpdatesAsync(address, handler)` |
| Subscribe user ledger updates | `socketClient.SpotApi.Account.SubscribeToUserLedgerUpdatesAsync(address, handler)` |
| Subscribe user event updates | `socketClient.SpotApi.Account.SubscribeToUserEventUpdatesAsync(...)` |
| Subscribe web data v3 updates | `socketClient.SpotApi.Account.SubscribeToWebData3UpdatesAsync(address, handler)` |

## Futures WebSocket

| User intent | HyperLiquid.Net member |
|---|---|
| Get futures exchange info over socket request API | `socketClient.FuturesApi.ExchangeData.GetExchangeInfoAsync()` |
| Get futures account over socket request API | `socketClient.FuturesApi.Account.GetAccountInfoAsync()` |
| Set leverage over socket request API | `socketClient.FuturesApi.Trading.SetLeverageAsync("ETH", leverage, MarginType.Cross)` |
| Place futures order over socket request API | `socketClient.FuturesApi.Trading.PlaceOrderAsync(...)` |
| Subscribe futures symbol updates | `socketClient.FuturesApi.ExchangeData.SubscribeToSymbolUpdatesAsync("ETH", handler)` |
| Subscribe all mid prices | `socketClient.FuturesApi.ExchangeData.SubscribeToPriceUpdatesAsync(dex, handler)` |
| Subscribe futures account and position updates | `socketClient.FuturesApi.Trading.SubscribeToBalanceAndPositionUpdatesAsync(address, dex, handler)` |
| Subscribe account and position updates all DEXes | `socketClient.FuturesApi.Trading.SubscribeToBalanceAndPositionUpdatesAllDexesAsync(address, handler)` |
| Subscribe user symbol updates | `socketClient.FuturesApi.Account.SubscribeToUserSymbolUpdatesAsync(address, "ETH", handler)` |
| Subscribe user funding updates | `socketClient.FuturesApi.Account.SubscribeToUserFundingUpdatesAsync(address, handler)` |
| Subscribe futures order book | `socketClient.FuturesApi.ExchangeData.SubscribeToOrderBookUpdatesAsync("ETH", handler)` |
| Subscribe futures klines | `socketClient.FuturesApi.ExchangeData.SubscribeToKlineUpdatesAsync("ETH", interval, handler)` |
| Subscribe futures trades | `socketClient.FuturesApi.ExchangeData.SubscribeToTradeUpdatesAsync("ETH", handler)` |
| Subscribe futures book ticker | `socketClient.FuturesApi.ExchangeData.SubscribeToBookTickerUpdatesAsync("ETH", handler)` |
| Subscribe order updates | `socketClient.FuturesApi.Trading.SubscribeToOrderUpdatesAsync(address, handler)` |
| Subscribe user trade updates | `socketClient.FuturesApi.Trading.SubscribeToUserTradeUpdatesAsync(address, handler)` |

## SharedApis

Use SharedApis for exchange-agnostic code across HyperLiquid, Binance, OKX, Bybit, Kraken, and other CryptoExchange.Net libraries.

| User intent | HyperLiquid.Net member or interface |
|---|---|
| Shared spot REST client | `new HyperLiquidRestClient().SpotApi.SharedClient` |
| Shared futures REST client | `new HyperLiquidRestClient().FuturesApi.SharedClient` |
| Shared spot socket client | `new HyperLiquidSocketClient().SpotApi.SharedClient` |
| Shared futures socket client | `new HyperLiquidSocketClient().FuturesApi.SharedClient` |
| Discover shared capabilities | `client.SpotApi.SharedClient.Discover()` |
| Shared spot ticker REST | `ISpotTickerRestClient.GetSpotTickerAsync(new GetTickerRequest(symbol))` |
| Shared futures ticker REST | `IFuturesTickerRestClient.GetFuturesTickerAsync(new GetTickerRequest(symbol))` |
| Shared spot order REST | `ISpotOrderRestClient.PlaceSpotOrderAsync(...)` |
| Shared futures order REST | `IFuturesOrderRestClient.PlaceFuturesOrderAsync(...)` |
| Shared balances REST | `IBalanceRestClient.GetBalancesAsync(new GetBalancesRequest(...))` |
| Shared position REST | `IPositionRestClient.GetPositionsAsync(...)` |
| Shared ticker socket | `ITickerSocketClient.SubscribeToTickerUpdatesAsync(...)` |
| Shared order book socket | `IOrderBookSocketClient.SubscribeToOrderBookUpdatesAsync(...)` |
| Shared trade socket | `ITradeSocketClient.SubscribeToTradeUpdatesAsync(...)` |

Shared REST methods return `HttpResult<T>` / `HttpResult`; shared socket subscriptions return `WebSocketResult<UpdateSubscription>`; shared symbol/cache helpers such as `SupportsSpotSymbolAsync` and `SupportsFuturesSymbolAsync` can return `ExchangeCallResult<T>`.

For shared socket subscriptions, keep the concrete socket client and unsubscribe with `await socketClient.UnsubscribeAsync(subscription.Data)`.

## Result Handling

| Situation | Pattern |
|---|---|
| REST success check | `if (!result.Success) { Console.WriteLine(result.Error); return; }` |
| Socket subscription success check | `if (!sub.Success) { Console.WriteLine(sub.Error); return; }` where `sub` is `WebSocketResult<UpdateSubscription>` |
| Socket request success check | `if (!query.Success) { Console.WriteLine(query.Error); return; }` where `query` is `QueryResult<T>` or `QueryResult` |
| Read REST data | Read `result.Data` only after `result.Success` |
| Read shared helper data | Read `ExchangeCallResult<T>.Data` only after `.Success` |
| Retry decision | Retry only when `result.Error?.IsTransient == true` |
| Cancellation | Pass `ct: cancellationToken` |

## Common Routing Pitfalls

| Do not use | Use instead |
|---|---|
| Raw `HttpClient` to HyperLiquid endpoints | `HyperLiquidRestClient` / `HyperLiquidSocketClient` |
| `ApiCredentials` | `HyperLiquidCredentials` |
| `HYPEUSDC` spot symbol | `HYPE/USDC` |
| `ETH/USDC` futures symbol | `ETH` |
| `.Data` without `.Success` check | Check `.Success` first |
| Market order without price | Pass `price` to `PlaceOrderAsync` |
| Separate margin client | `FuturesApi.Trading.SetLeverageAsync` / `UpdateIsolatedMarginAsync` |
| Separate staking client | `SpotApi.Account.GetStaking*` and staking action methods |
| `ITickerSocketClient.UnsubscribeAsync(...)` | Keep the concrete socket client and call `socketClient.UnsubscribeAsync(subscription.Data)` |
| Invented `GetServerTimeAsync()` | Inspect interfaces; HyperLiquid.Net does not expose that method |
