# HyperLiquid.Net API surfaces

## Client roots

`HyperLiquidRestClient` / `IHyperLiquidRestClient` and `HyperLiquidSocketClient` / `IHyperLiquidSocketClient` expose `SpotApi` and `FuturesApi`, each with `ExchangeData`, `Account`, and `Trading`. Socket request APIs mirror the corresponding REST groups and also expose streams. `IHyperLiquidTrackerFactory` provides trackers.

Account and trading interfaces inherit shared base operations: either API root can access portfolio history, abstraction state, ledger, staking, vaults, transfers, order management, TWAP, and builder approvals. Choose the root matching the product and symbol. `Account.SendAssetAsync(...)` supports explicit source/destination DEX, token name/id, and optional subaccount. Check the method-specific parameters rather than assuming every account call accepts `dex`.

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
| Get extended subaccounts with spot balances and per-DEX futures state | `client.SpotApi.Account.GetSubAccounts2Async()` |
| Get user portfolio history | `client.SpotApi.Account.GetUserPortfolioAsync()` or `client.FuturesApi.Account.GetUserPortfolioAsync()` |
| Get user role | `client.SpotApi.Account.GetUserRoleAsync()` |
| Approve an agent | `client.SpotApi.Account.ApproveAgentAsync(...)` |
| Remove an agent | `client.SpotApi.Account.RemoveAgentAsync(...)` |
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

### HIP-4 outcomes

Use `client.SpotApi.ExchangeData.GetQuestionsAndOutcomesInfoAsync()` and `GetSettledOutcomeAsync(outcomeId)` for outcome metadata and settlement. `HyperLiquidUtils.GetOutcomeInfoAsync(client, outcomeId)` resolves outcome information; `socketClient.SpotApi.ExchangeData.SubscribeToOutcomeInfoUpdatesAsync(...)` streams updates.

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
| Get perp annotation | `client.FuturesApi.ExchangeData.GetPerpAnnotationAsync(asset)` |
| Get perp categories | `client.FuturesApi.ExchangeData.GetPerpCategoriesAsync()` |
| Get concise perp annotations across DEXes | `client.FuturesApi.ExchangeData.GetPerpConciseAnnotationsAsync()` |
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

## Shared API V2

`IHyperLiquidSharedApiClient` exposes these aggregate properties. Each native surface also exposes the corresponding `.SharedApi`.

- `SpotRest` (`IHyperLiquidRestClientSpotSharedApi`): `IGetBalancesRest`, `IGetKlinesRest`, `IGetOrderBookRest`, `IGetTickerRest`, `IGetAllTickersRest`, `IGetSpotSymbolsRest`, `IPlaceSpotOrderRest`, `IGetSpotOrderRest`, `IGetOpenSpotOrdersRest`, `IGetClosedSpotOrdersRest`, `IGetSpotOrderTradesRest`, `IGetSpotUserTradeHistoryRest`, `ICancelSpotOrderRest`, `IGetAssetRest`, `IGetAllAssetsRest`, `IGetFeesRest`, `IWithdrawRest`, `IGetSpotOrderByClientOrderIdRest`, `ICancelSpotOrderByClientOrderIdRest`, `IGetBookTickerRest`, `ITransferRest`.
- `FuturesRest` (`IHyperLiquidRestClientFuturesSharedApi`): `IGetBalancesRest`, `IGetKlinesRest`, `IGetOrderBookRest`, `IGetTickerRest`, `IGetAllTickersRest`, `IGetFeesRest`, `IGetFundingRateHistoryRest`, `IGetLeverageRest`, `ISetLeverageRest`, `IGetOpenInterestRest`, `IGetFuturesSymbolsRest`, `IPlaceFuturesOrderRest`, `IGetFuturesOrderRest`, `IGetOpenFuturesOrdersRest`, `IGetClosedFuturesOrdersRest`, `IGetFuturesOrderTradesRest`, `IGetFuturesUserTradeHistoryRest`, `ICancelFuturesOrderRest`, `IGetPositionsRest`, `IGetFuturesOrderByClientOrderIdRest`, `ICancelFuturesOrderByClientOrderIdRest`, `ISetFuturesTpSlRest`, `ICancelFuturesTpSlRest`, `IGetBookTickerRest`.
- `SpotSocket` (`IHyperLiquidSocketClientSpotSharedApi`): `ISubscribeTickerSocket`, `ISubscribeTradesSocket`, `ISubscribeKlinesSocket`, `ISubscribeOrderBookSocket`, `ISubscribeSpotOrdersSocket`, `ISubscribeBalancesSocket`, `ISubscribeBookTickerSocket`, `ISubscribeUserTradesSocket`, `IPlaceSpotOrderSocket`, `ICancelSpotOrderSocket`.
- `FuturesSocket` (`IHyperLiquidSocketClientFuturesSharedApi`): `ISubscribeTickerSocket`, `ISubscribeTradesSocket`, `ISubscribeKlinesSocket`, `ISubscribeOrderBookSocket`, `ISubscribeUserTradesSocket`, `ISubscribeFuturesOrdersSocket`, `ISubscribeBalancesSocket`, `ISubscribePositionsSocket`, `ISubscribeBookTickerSocket`, `IPlaceFuturesOrderSocket`, `ICancelFuturesOrderSocket`.

Use aggregate properties or inject a narrow capability for known operations. For runtime selection, call `aggregate.GetCapability(SharedCapabilities.Tickers.GetTicker.Rest, tradingMode: TradingMode.Spot)` and check for null before using the match's `Capability` and `Options`. Descriptors identify operations; they do not guarantee support. `GetCapabilities` returns all matches; `Discover` summarizes support. With several exchanges registered, inject `IEnumerable<TCapability>` and select by exchange and supported trading mode.

## Symbols, catalogs, and quantities

Native spot symbols use `Base/Quote`, e.g. `HYPE/USDC`. Native perpetual symbols use the base name, e.g. `ETH`; preserve HIP-3 DEX prefixes from metadata. `HyperLiquidExchange.FormatSymbol` and `SharedSymbol` handle reusable spot/perpetual formatting. Shared adapters normalize aliases such as UBTC/UETH. Shared perpetual catalog names can use `ETH/USDC`, while native perpetual endpoints expect `ETH`; do not pass a Shared catalog name directly to native trading. Preserve DEX prefixes when mapping back to native symbols.

Use `IGetSpotSymbolsRest.GetSpotSymbolsAsync(new GetSymbolsRequest())` or `IGetFuturesSymbolsRest.GetFuturesSymbolsAsync(new GetSymbolsRequest())` before reading `SpotSymbolCatalog` or `FuturesSymbolCatalog`. The futures symbols request accepts the optional `dex` exchange parameter; retrieve the unfiltered catalog when exploring all DEXes. Inspect request filters and catalog display names, base/quote asset types and subtypes: crypto, stablecoin, TradFi equity/commodity, and fiat classifications where populated. Resolve HIP-3 metadata and account/DEX abstraction for the selected market; do not assume every DEX uses the same collateral.

Native order quantities are decimal base-asset amounts, including perpetual orders. Shared order support and order books use `SharedQuantityType.BaseAsset`; pass base quantity through `SharedOrderQuantity`, rather than contract counts or quote notional. Respect metadata quantity/price precision and minimum order constraints. Market orders still require a current reference `price` for slippage calculation.

## Checkout-only sources

- [Public interfaces](../../../../HyperLiquid.Net/Interfaces/Clients/): exact signatures, optional addresses/DEX parameters, inheritance, and cancellation tokens.
- [Shared adapters](../../../../HyperLiquid.Net/Clients/): supported options, quantity conversions, and symbol classification.
- [Complete examples](../../../../Examples/ai-friendly/): independently compilable programs.
