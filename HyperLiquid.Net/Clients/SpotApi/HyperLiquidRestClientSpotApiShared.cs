using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Enums;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects.Errors;

namespace HyperLiquid.Net.Clients.SpotApi
{
    internal partial class HyperLiquidRestClientSpotApi : IHyperLiquidRestClientSpotApiShared
    {
        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidSpot";
        public TradingMode[] SupportedTradingModes => new[] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();
        public SharedClientInfo Discover() => SharedUtils.GetClientInfo(HyperLiquidExchange.Metadata, this);


        #region Balance Client
        GetBalancesOptions IBalanceRestClient.GetBalancesOptions { get; } = new GetBalancesOptions(_exchangeName, AccountTypeFilter.Spot);

        async Task<HttpResult<SharedBalance[]>> IBalanceRestClient.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetBalancesOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedBalance[]>(Exchange, validationError);

            var result = await Account.GetBalancesAsync(ct: ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<SharedBalance[]>(result);

            return HttpResult.Ok(result, result.Data.Select(x => new SharedBalance(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Asset), x.Total - x.Hold, x.Total)).ToArray());
        }

        #endregion

        #region Klines Client

        GetKlinesOptions IKlineRestClient.GetKlinesOptions { get; } = new GetKlinesOptions(_exchangeName, false, true, true, 1000, false,
            SharedKlineInterval.OneMinute,
            SharedKlineInterval.FiveMinutes,
            SharedKlineInterval.FifteenMinutes,
            SharedKlineInterval.ThirtyMinutes,
            SharedKlineInterval.OneHour,
            SharedKlineInterval.TwoHours,
            SharedKlineInterval.FourHours,
            SharedKlineInterval.EightHours,
            SharedKlineInterval.TwelveHours,
            SharedKlineInterval.OneDay,
            SharedKlineInterval.OneWeek,
            SharedKlineInterval.OneMonth)
        {
            MaxTotalDataPoints = 5000
        };

        async Task<HttpResult<SharedKline[]>> IKlineRestClient.GetKlinesAsync(GetKlinesRequest request, PageRequest? pageRequest, CancellationToken ct)
        {
            var validationError = SharedClient.GetKlinesOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedKline[]>(Exchange, validationError);
                    var interval = (Enums.KlineInterval)request.Interval;

                    var symbol = request.Symbol!.GetSymbol(FormatSymbol);
                    int limit = request.Limit ?? 1000;
                    var direction = DataDirection.Descending;
                    var pageParams = Pagination.GetPaginationParameters(direction, limit, request.StartTime, request.EndTime ?? DateTime.UtcNow, pageRequest);

                    // Get data
                    var result = await ExchangeData.GetKlinesAsync(
                        symbol,
                        interval,
                        startTime: pageParams.StartTime ?? DateTime.UtcNow.Add(TimeSpan.FromSeconds(-((int)interval * 99))),
                        endTime: pageParams.EndTime ?? DateTime.UtcNow,
                        ct: ct
                        ).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedKline[]>(result);

                    var nextPageRequest = Pagination.GetNextPageRequest(
                             () => Pagination.NextPageFromTime(pageParams, result.Data.Min(x => x.OpenTime)),
                             result.Data.Length,
                             result.Data.Select(x => x.OpenTime),
                             request.StartTime,
                             request.EndTime ?? DateTime.UtcNow,
                             pageParams);

                    return HttpResult.Ok(result, ExchangeHelpers.ApplyFilter(result.Data, x => x.OpenTime, request.StartTime, request.EndTime, direction)
                            .Select(x =>
                                new SharedKline(
                                    request.Symbol,
                                    symbol,
                                    x.OpenTime,
                                    x.ClosePrice,
                                    x.HighPrice,
                                    x.LowPrice,
                                    x.OpenPrice,
                                    x.Volume))
                            .ToArray(), nextPageRequest);
                
        }

        #endregion

        #region Order Book client
        GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; } = new GetOrderBookOptions(_exchangeName, [20], false);
        async Task<HttpResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetOrderBookOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedOrderBook>(Exchange, validationError);

                    var result = await ExchangeData.GetOrderBookAsync(
                        request.Symbol!.GetSymbol(FormatSymbol),
                        ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedOrderBook>(result);

                    if (result.Data == null)
                        return HttpResult.Fail<SharedOrderBook>(result, new ServerError(ErrorInfo.Unknown with { Message = "No response" }));

                    return HttpResult.Ok(result, new SharedOrderBook(result.Data.Levels.Asks, result.Data.Levels.Bids));
                
        }

        #endregion

        #region Ticker client

        GetSpotTickerOptions ISpotTickerRestClient.GetSpotTickerOptions { get; } = new GetSpotTickerOptions(_exchangeName);
        async Task<HttpResult<SharedSpotTicker>> ISpotTickerRestClient.GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotTicker>(Exchange, validationError);

                    var symbolName = request.Symbol!.GetSymbol(FormatSymbol);
                    var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedSpotTicker>(result);

                    var symbol = result.Data.Tickers.SingleOrDefault(x => x.Symbol == symbolName);
                    if (symbol == null)
                        return HttpResult.Fail<SharedSpotTicker>(result, new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                    return HttpResult.Ok(result, new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, symbol.Symbol!), symbol.Symbol!, symbol.MidPrice, null, null, symbol.BaseVolume, (symbol.MidPrice == null || symbol.PreviousDayPrice == 0) ? null : Math.Round((symbol.MidPrice.Value / symbol.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
                    {
                        QuoteVolume = symbol.QuoteVolume
                    });
                
        }

        GetSpotTickersOptions ISpotTickerRestClient.GetSpotTickersOptions { get; } = new GetSpotTickersOptions(_exchangeName);
        async Task<HttpResult<SharedSpotTicker[]>> ISpotTickerRestClient.GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotTickersOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotTicker[]>(Exchange, validationError);

                    var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedSpotTicker[]>(result);

                    return HttpResult.Ok(result, result.Data.Tickers.Select(x => new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Symbol!), x.Symbol!, x.MidPrice, null, null, x.BaseVolume, (x.MidPrice == null || x.PreviousDayPrice == 0) ? null : Math.Round((x.MidPrice.Value / x.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
                    {
                        QuoteVolume = x.QuoteVolume
                    }).ToArray());
                
        }

        #endregion

        #region Book Ticker client

        GetBookTickerOptions IBookTickerRestClient.GetBookTickerOptions { get; } = new GetBookTickerOptions(_exchangeName, false);
        async Task<HttpResult<SharedBookTicker>> IBookTickerRestClient.GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetBookTickerOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedBookTicker>(Exchange, validationError);

                    var symbol = request.Symbol!.GetSymbol(FormatSymbol);
                    var resultTicker = await ExchangeData.GetOrderBookAsync(symbol, ct: ct).ConfigureAwait(false);
                    if (!resultTicker.Success)
                        return HttpResult.Fail<SharedBookTicker>(resultTicker);

                    if (resultTicker.Data == null)
                        return HttpResult.Fail<SharedBookTicker>(resultTicker, new ServerError(new ErrorInfo(ErrorType.Unknown, "No response")));

                    return HttpResult.Ok(resultTicker, new SharedBookTicker(
                        ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, symbol),
                        symbol,
                        resultTicker.Data.Levels.Asks[0].Price,
                        resultTicker.Data.Levels.Asks[0].Quantity,
                        resultTicker.Data.Levels.Bids[0].Price,
                        resultTicker.Data.Levels.Bids[0].Quantity));
                
        }

        #endregion

        #region Spot Symbol client
        GetSpotSymbolsOptions ISpotSymbolRestClient.GetSpotSymbolsOptions { get; } = new GetSpotSymbolsOptions(_exchangeName, false);

        async Task<HttpResult<SharedSpotSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotSymbolsOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotSymbol[]>(Exchange, validationError);

                    var result = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedSpotSymbol[]>(result);

                    var resultData = result.Data.Symbols.Select(s => new SharedSpotSymbol(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(s.BaseAsset.Name), HyperLiquidExchange.AssetAliases.ExchangeToCommonName(s.QuoteAsset.Name), s.Name, true)
                    {
                        MinTradeQuantity = 1m / (decimal)(Math.Pow(10, s.BaseAsset.QuantityDecimals)),
                        MinNotionalValue = 10, // Order API returns error mentioning at least 10$ order value, but value isn't returned by symbol API
                        QuantityDecimals = s.BaseAsset.QuantityDecimals,
                        PriceSignificantFigures = 5,
                        PriceDecimals = 8 - s.BaseAsset.QuantityDecimals
                    }).ToArray();

                    ExchangeSymbolCache.UpdateSymbolInfo(_topicId, EnvironmentName, null, resultData);
                    return HttpResult.Ok(result, resultData);
                
        }

        async Task<ExchangeCallResult<SharedSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsForBaseAssetAsync(string baseAsset)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId, EnvironmentName, null))
            {
                var symbols = await ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols.Success)
                    return ExchangeCallResult<SharedSymbol[]>.Fail(Exchange, symbols.Error!);
            }

            return ExchangeCallResult<SharedSymbol[]>.Ok(Exchange, ExchangeSymbolCache.GetSymbolsForBaseAsset(_topicId, EnvironmentName, null, baseAsset));
        }

        async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(SharedSymbol symbol)
        {
            if (symbol.TradingMode != TradingMode.Spot)
                throw new ArgumentException(nameof(symbol), "Only Spot symbols allowed");

            if (!ExchangeSymbolCache.HasCached(_topicId, EnvironmentName, null))
            {
                var symbols = await ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols.Success)
                    return ExchangeCallResult<bool>.Fail(Exchange, symbols.Error!);
            }

            return ExchangeCallResult<bool>.Ok(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, EnvironmentName, null, symbol));
        }

        async Task<ExchangeCallResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(string symbolName)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId, EnvironmentName, null))
            {
                var symbols = await ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols.Success)
                    return ExchangeCallResult<bool>.Fail(Exchange, symbols.Error!);
            }

            return ExchangeCallResult<bool>.Ok(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, EnvironmentName, null, symbolName));
        }
        #endregion

        #region Spot Order Client

        SharedFeeDeductionType ISpotOrderRestClient.SpotFeeDeductionType => SharedFeeDeductionType.DeductFromOutput;
        SharedFeeAssetType ISpotOrderRestClient.SpotFeeAssetType => SharedFeeAssetType.OutputAsset;
        SharedOrderType[] ISpotOrderRestClient.SpotSupportedOrderTypes { get; } = new[] { SharedOrderType.Limit, SharedOrderType.Market, SharedOrderType.LimitMaker };
        SharedTimeInForce[] ISpotOrderRestClient.SpotSupportedTimeInForce { get; } = new[] { SharedTimeInForce.GoodTillCanceled, SharedTimeInForce.ImmediateOrCancel };
        SharedQuantitySupport ISpotOrderRestClient.SpotSupportedOrderQuantity { get; } = new SharedQuantitySupport(
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset);

        string ISpotOrderRestClient.GenerateClientOrderId() => ExchangeHelpers.RandomHexString(16)!.ToLowerInvariant();

        PlaceSpotOrderOptions ISpotOrderRestClient.PlaceSpotOrderOptions { get; } = new PlaceSpotOrderOptions(_exchangeName)
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(PlaceSpotOrderRequest.Price), typeof(decimal), "Price for the order. For market orders this should be the current symbol price", 21.5m)
            },
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("vaultAddress", typeof(string), "Vault address to use for the order", "0x123...")
            }
        };

        async Task<HttpResult<SharedId>> ISpotOrderRestClient.PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.PlaceSpotOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

            var result = await Trading.PlaceOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                request.OrderType == SharedOrderType.Limit || request.OrderType == SharedOrderType.LimitMaker ? Enums.OrderType.Limit : Enums.OrderType.Market,
                quantity: request.Quantity?.QuantityInBaseAsset ?? 0,
                price: request.Price!.Value,
                timeInForce: GetTimeInForce(request.TimeInForce, request.OrderType),
                clientOrderId: request.ClientOrderId,
                vaultAddress: request.GetParamValue<string?>(Exchange, "vaultAddress"),
                ct: ct).ConfigureAwait(false);

            if (!result.Success)
                return HttpResult.Fail<SharedId>(result);

            return HttpResult.Ok(result, new SharedId(result.Data.OrderId.ToString()));
                
        }

        GetSpotOrderOptions ISpotOrderRestClient.GetSpotOrderOptions { get; } = new GetSpotOrderOptions(_exchangeName, true);
        async Task<HttpResult<SharedSpotOrder>> ISpotOrderRestClient.GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotOrder>(Exchange, validationError);

                    if (!long.TryParse(request.OrderId, out var orderId))
                        return HttpResult.Fail<SharedSpotOrder>(Exchange, ArgumentError.Invalid(nameof(GetOrderRequest.OrderId), "Invalid order id"));

                    var order = await Trading.GetOrderAsync(orderId, ct: ct).ConfigureAwait(false);
                    if (!order.Success)
                        return HttpResult.Fail<SharedSpotOrder>(order);

                    return HttpResult.Ok(order, new SharedSpotOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, order.Data.Order.Symbol!),
                        order.Data.Order.Symbol!,
                        order.Data.Order.OrderId.ToString(),
                        ParseOrderType(order.Data.Order.OrderType),
                        order.Data.Order.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                        ParseOrderStatus(order.Data.Status),
                        order.Data.Order.Timestamp)
                    {
                        TimeInForce = ParseTimeInForce(order.Data.Order.TimeInForce),
                        ClientOrderId = order.Data.Order.ClientOrderId,
                        OrderPrice = order.Data.Order.Price,
                        OrderQuantity = new SharedOrderQuantity(order.Data.Order.Quantity),
                        QuantityFilled = new SharedOrderQuantity(order.Data.Order.Quantity - order.Data.Order.QuantityRemaining),
                        UpdateTime = order.Data.Timestamp,
                        TriggerPrice = order.Data.Order.TriggerPrice,
                        IsTriggerOrder = order.Data.Order.TriggerPrice > 0
                    });
                
        }

        GetOpenSpotOrdersOptions ISpotOrderRestClient.GetOpenSpotOrdersOptions { get; } = new GetOpenSpotOrdersOptions(_exchangeName, true);
        async Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetOpenSpotOrdersOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotOrder[]>(Exchange, validationError);

                    var symbol = request.Symbol?.GetSymbol(FormatSymbol);
                    var orders = await Trading.GetOpenOrdersExtendedAsync(ct: ct).ConfigureAwait(false);
                    if (!orders.Success)
                        return HttpResult.Fail<SharedSpotOrder[]>(orders);

                    var data = orders.Data.Where(x => x.SymbolType == Enums.SymbolType.Spot);
                    if (symbol != null)
                        data = data.Where(x => x.Symbol == symbol);

                    return HttpResult.Ok(orders, data.Select(x => new SharedSpotOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Symbol),
                        x.Symbol!,
                        x.OrderId.ToString(),
                        ParseOrderType(x.OrderType),
                        x.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                        SharedOrderStatus.Open,
                        x.Timestamp)
                    {
                        TimeInForce = ParseTimeInForce(x.TimeInForce),
                        ClientOrderId = x.ClientOrderId,
                        OrderPrice = x.Price,
                        OrderQuantity = new SharedOrderQuantity(x.Quantity),
                        QuantityFilled = new SharedOrderQuantity(x.Quantity - x.QuantityRemaining),
                        UpdateTime = x.Timestamp,
                        TriggerPrice = x.TriggerPrice,
                        IsTriggerOrder = x.TriggerPrice > 0
                    }).ToArray());
                
        }

        GetSpotClosedOrdersOptions ISpotOrderRestClient.GetClosedSpotOrdersOptions { get; } = new GetSpotClosedOrdersOptions(_exchangeName, true, true, false, 2000)
        {
            RequestNotes = "API request doesn't allow filtering, so filtering is done client side. This might result in missing historical data as only up to 2000 results are returned from the API"
        };
        async Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, PageRequest? pageToken, CancellationToken ct)
        {
            var validationError = SharedClient.GetClosedSpotOrdersOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotOrder[]>(Exchange, validationError);

                    // Get data
                    var orders = await Trading.GetOrderHistoryAsync(ct: ct).ConfigureAwait(false);
                    if (!orders.Success)
                        return HttpResult.Fail<SharedSpotOrder[]>(orders);

                    var direction = request.Direction ?? DataDirection.Ascending;
                    var symbol = request.Symbol!.GetSymbol(FormatSymbol);
                    var data = orders.Data.Where(x =>
                        x.Order.SymbolType == Enums.SymbolType.Spot
                        && x.Order.Symbol == symbol
                        && x.Status != Enums.OrderStatus.Open);

                    if (direction == DataDirection.Ascending)
                        data.OrderBy(x => x.Timestamp);
                    if (direction == DataDirection.Descending)
                        data.OrderByDescending(x => x.Timestamp);
                    if (request.Limit != null)
                        data = data.Take(request.Limit.Value);

                    return HttpResult.Ok(orders, ExchangeHelpers.ApplyFilter(data, x => x.Timestamp, request.StartTime, request.EndTime, direction)
                                .Select(x =>
                                    new SharedSpotOrder(
                                        ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Order.Symbol),
                                        x.Order.Symbol!,
                                        x.Order.OrderId.ToString(),
                                        ParseOrderType(x.Order.OrderType),
                                        x.Order.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                                        ParseOrderStatus(x.Status),
                                        x.Order.Timestamp)
                                    {
                                        TimeInForce = ParseTimeInForce(x.Order.TimeInForce),
                                        ClientOrderId = x.Order.ClientOrderId,
                                        OrderPrice = x.Order.Price,
                                        OrderQuantity = new SharedOrderQuantity(x.Order.Quantity),
                                        QuantityFilled = new SharedOrderQuantity(x.Order.Quantity - x.Order.QuantityRemaining),
                                        UpdateTime = x.Timestamp,
                                        TriggerPrice = x.Order.TriggerPrice,
                                        IsTriggerOrder = x.Order.TriggerPrice > 0
                                    })
                                .ToArray());
                
        }

        GetSpotOrderTradesOptions ISpotOrderRestClient.GetSpotOrderTradesOptions { get; } = new GetSpotOrderTradesOptions(_exchangeName, true);
        async Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotOrderTradesOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedUserTrade[]>(Exchange, validationError);

                    if (!long.TryParse(request.OrderId, out var orderId))
                        return HttpResult.Fail<SharedUserTrade[]>(Exchange, ArgumentError.Invalid(nameof(GetOrderTradesRequest.OrderId), "Invalid order id"));

                    var orders = await Trading.GetUserTradesAsync(ct: ct).ConfigureAwait(false);
                    if (!orders.Success)
                        return HttpResult.Fail<SharedUserTrade[]>(orders);

                    var data = orders.Data.Where(x => x.OrderId == orderId);
                    return HttpResult.Ok(orders, data.Select(x => new SharedUserTrade(
                        ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Symbol),
                        x.Symbol,
                        x.OrderId.ToString(),
                        x.TradeId.ToString(),
                        x.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                        x.Quantity,
                        x.Price,
                        x.Timestamp)
                    {
                        Fee = x.Fee,
                        FeeAsset = HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.FeeToken),
                        Role = x.Crossed ? SharedRole.Taker : SharedRole.Maker
                    }).ToArray());
                
        }

        GetSpotUserTradesOptions ISpotOrderRestClient.GetSpotUserTradesOptions { get; } = new GetSpotUserTradesOptions(_exchangeName, true, false, true, 2000)
        {
            RequestNotes = "API request doesn't allow filtering, so filtering is done client side. This might result in missing historical data as only up to 2000 per request / 10000 results in total are returned from the API"
        };
        async Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotUserTradesAsync(GetUserTradesRequest request, PageRequest? pageRequest, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotUserTradesOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedUserTrade[]>(Exchange, validationError);

                    int limit = request.Limit ?? 2000;
                    var direction = DataDirection.Ascending;
                    var pageParams = Pagination.GetPaginationParameters(direction, limit, request.StartTime, request.EndTime ?? DateTime.UtcNow, pageRequest, false);

                    // Get data
                    var result = await Trading.GetUserTradesByTimeAsync(
                        startTime: pageParams.StartTime ?? DateTime.UtcNow.AddDays(-7),
                        ct: ct
                        ).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedUserTrade[]>(result);

                    var data = result.Data.Where(x => x.Symbol == request.Symbol!.GetSymbol(FormatSymbol));
                    var nextPageRequest = Pagination.GetNextPageRequest(
                             () => Pagination.NextPageFromTime(pageParams, result.Data.Max(x => x.Timestamp)),
                             result.Data.Length,
                             result.Data.Select(x => x.Timestamp),
                             request.StartTime,
                             request.EndTime ?? DateTime.UtcNow,
                             pageParams);

                    return HttpResult.Ok(result, ExchangeHelpers.ApplyFilter(data, x => x.Timestamp, request.StartTime, request.EndTime, direction)
                            .Select(x =>
                                new SharedUserTrade(
                                ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, x.Symbol),
                                x.Symbol,
                                x.OrderId.ToString(),
                                x.TradeId.ToString(),
                                x.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                                x.Quantity,
                                x.Price,
                                x.Timestamp)
                                {
                                    Fee = x.Fee,
                                    FeeAsset = HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.FeeToken),
                                    Role = x.Crossed ? SharedRole.Taker : SharedRole.Maker
                                }).ToArray(), nextPageRequest);
                
        }

        CancelSpotOrderOptions ISpotOrderRestClient.CancelSpotOrderOptions { get; } = new CancelSpotOrderOptions(_exchangeName, true)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("vaultAddress", typeof(string), "Vault address to use for the order", "0x123...")
            }
        };
        async Task<HttpResult<SharedId>> ISpotOrderRestClient.CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.CancelSpotOrderOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId))
                return HttpResult.Fail<SharedId>(Exchange, ArgumentError.Invalid(nameof(CancelOrderRequest.OrderId), "Invalid order id"));

            var order = await Trading.CancelOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                orderId, 
                vaultAddress: request.GetParamValue<string?>(Exchange, "vaultAddress"),
                ct: ct).ConfigureAwait(false);
            if (!order.Success)
                return HttpResult.Fail<SharedId>(order);

            return HttpResult.Ok(order, new SharedId(request.OrderId));
                
        }

        private SharedTimeInForce? ParseTimeInForce(TimeInForce? timeInForce)
        {
            if (timeInForce == TimeInForce.ImmediateOrCancel) return SharedTimeInForce.ImmediateOrCancel;
            if (timeInForce == TimeInForce.GoodTillCanceled) return SharedTimeInForce.GoodTillCanceled;

            return null;
        }

        private Enums.TimeInForce? GetTimeInForce(SharedTimeInForce? tif, SharedOrderType type)
        {
            if (tif == SharedTimeInForce.ImmediateOrCancel) return Enums.TimeInForce.ImmediateOrCancel;
            if (tif == SharedTimeInForce.GoodTillCanceled) return Enums.TimeInForce.GoodTillCanceled;
            if (type == SharedOrderType.LimitMaker) return Enums.TimeInForce.PostOnly;

            return null;
        }

        private SharedOrderStatus ParseOrderStatus(Enums.OrderStatus status)
        {
            if (status == Enums.OrderStatus.Open) return SharedOrderStatus.Open;
            if (status == Enums.OrderStatus.Filled) return SharedOrderStatus.Filled;
            if (status == Enums.OrderStatus.Canceled
                || status == Enums.OrderStatus.Rejected
                || status == Enums.OrderStatus.MarginCanceled
                || status == Enums.OrderStatus.RejectedInsufficientBalance
                || status == Enums.OrderStatus.RejectedIOC
                || status == Enums.OrderStatus.RejectedBadPrice
                || status == Enums.OrderStatus.RejectedInsufficientMargin
                || status == Enums.OrderStatus.RejectedSiblingFilledCanceled
                || status == Enums.OrderStatus.ReduceOnlyRejected
                || status == Enums.OrderStatus.RejectedMinValue
                || status == Enums.OrderStatus.PositionIncreaseAtOpenInterestCapRejected
                || status == Enums.OrderStatus.PositionFlipAtOpenInterestCapRejected
                || status == Enums.OrderStatus.TooAggressiveAtOpenInterestCapRejected
                || status == Enums.OrderStatus.OpenInterestIncreaseRejected
                || status == Enums.OrderStatus.OpenInterestCapCanceled
                || status == Enums.OrderStatus.VaultWithdrawalCanceled
                || status == Enums.OrderStatus.SelfTradeCanceled
                || status == Enums.OrderStatus.DelistedCanceled
                || status == Enums.OrderStatus.LiquidatedCanceled
                || status == Enums.OrderStatus.ScheduledCancel
                || status == Enums.OrderStatus.RejectedTick
                || status == Enums.OrderStatus.RejectedBadTriggerPrice
                || status == Enums.OrderStatus.RejectedNoLiquidity
                || status == Enums.OrderStatus.RejectedOracle
                || status == Enums.OrderStatus.RejectedPerpMaxPosition)
            {
                return SharedOrderStatus.Canceled;
            }

            return SharedOrderStatus.Unknown;
        }

        private SharedOrderType ParseOrderType(Enums.OrderType type)
        {
            if (type == Enums.OrderType.Market) return SharedOrderType.Market;
            if (type == Enums.OrderType.Limit) return SharedOrderType.Limit;

            return SharedOrderType.Other;
        }

        #endregion

        #region Spot Client Id Order Client

        GetSpotOrderByClientOrderIdOptions ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdOptions { get; } = new GetSpotOrderByClientOrderIdOptions(_exchangeName, true);
        async Task<HttpResult<SharedSpotOrder>> ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetSpotOrderByClientOrderIdOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedSpotOrder>(Exchange, validationError);

                    var order = await Trading.GetOrderAsync(clientOrderId: request.OrderId, ct: ct).ConfigureAwait(false);
                    if (!order.Success)
                        return HttpResult.Fail<SharedSpotOrder>(order);

                    return HttpResult.Ok(order, new SharedSpotOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, EnvironmentName, null, order.Data.Order.Symbol!),
                        order.Data.Order.Symbol!,
                        order.Data.Order.OrderId.ToString(),
                        ParseOrderType(order.Data.Order.OrderType),
                        order.Data.Order.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                        ParseOrderStatus(order.Data.Status),
                        order.Data.Order.Timestamp)
                    {
                        TimeInForce = ParseTimeInForce(order.Data.Order.TimeInForce),
                        ClientOrderId = order.Data.Order.ClientOrderId,
                        OrderPrice = order.Data.Order.Price,
                        OrderQuantity = new SharedOrderQuantity(order.Data.Order.Quantity),
                        QuantityFilled = new SharedOrderQuantity(order.Data.Order.Quantity - order.Data.Order.QuantityRemaining),
                        UpdateTime = order.Data.Timestamp,
                        TriggerPrice = order.Data.Order.TriggerPrice,
                        IsTriggerOrder = order.Data.Order.TriggerPrice > 0
                    });
                
        }

        CancelSpotOrderByClientOrderIdOptions ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdOptions { get; } = new CancelSpotOrderByClientOrderIdOptions(_exchangeName, true)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("vaultAddress", typeof(string), "Vault address to use for the order", "0x123...")
            }
        };
        async Task<HttpResult<SharedId>> ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.CancelSpotOrderByClientOrderIdOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

            var order = await Trading.CancelOrderByClientOrderIdAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                clientOrderId: request.OrderId,
                vaultAddress: request.GetParamValue<string?>(Exchange, "vaultAddress"), 
                ct: ct).ConfigureAwait(false);
            if (!order.Success)
                return HttpResult.Fail<SharedId>(order);

            return HttpResult.Ok(order, new SharedId(request.OrderId));
                
        }
        #endregion

        #region Asset client
        GetAssetsOptions IAssetsRestClient.GetAssetsOptions { get; } = new GetAssetsOptions(_exchangeName, true);

        async Task<HttpResult<SharedAsset[]>> IAssetsRestClient.GetAssetsAsync(GetAssetsRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetAssetsOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedAsset[]>(Exchange, validationError);

                    var assets = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!assets.Success)
                        return HttpResult.Fail<SharedAsset[]>(assets);

                    return HttpResult.Ok(assets, assets.Data.Assets.Select(x => new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Name))
                    {
                        FullName = x.FullName,
                        Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = x.AssetId
                }]
                    }).ToArray());
                
        }

        GetAssetOptions IAssetsRestClient.GetAssetOptions { get; } = new GetAssetOptions(_exchangeName, false);
        async Task<HttpResult<SharedAsset>> IAssetsRestClient.GetAssetAsync(GetAssetRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetAssetOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedAsset>(Exchange, validationError);

                    var assets = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!assets.Success)
                        return HttpResult.Fail<SharedAsset>(assets);

                    var asset = assets.Data.Assets.SingleOrDefault(x => x.Name.Equals(HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset), StringComparison.InvariantCultureIgnoreCase));
                    if (asset == null)
                        return HttpResult.Fail<SharedAsset>(assets, new ServerError(new ErrorInfo(ErrorType.UnknownAsset, "Asset not found")));

                    return HttpResult.Ok(assets, new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(asset.Name))
                    {
                        FullName = asset.FullName,
                        Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = asset.AssetId
                }]
                    });
                
        }

        #endregion

        #region Fee Client
        GetFeeOptions IFeeRestClient.GetFeeOptions { get; } = new GetFeeOptions(_exchangeName, true);

        async Task<HttpResult<SharedFee>> IFeeRestClient.GetFeesAsync(GetFeeRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.GetFeeOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedFee>(Exchange, validationError);

                    // Get data
                    var result = await Account.GetFeeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!result.Success)
                        return HttpResult.Fail<SharedFee>(result);

                    // Return
                    return HttpResult.Ok(result, new SharedFee(result.Data.MakerFeeRateSpot * 100, result.Data.TakerFeeRateSpot * 100));
                
        }
        #endregion

        #region Withdraw client

        WithdrawOptions IWithdrawRestClient.WithdrawOptions { get; } = new WithdrawOptions(_exchangeName);
        async Task<HttpResult<SharedId>> IWithdrawRestClient.WithdrawAsync(WithdrawRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.WithdrawOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

                    // Get data
                    var withdrawal = await Account.TransferSpotAsync(
                        request.Address,
                        HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset),
                        request.Quantity,
                        ct: ct).ConfigureAwait(false);
                    if (!withdrawal.Success)
                        return HttpResult.Fail<SharedId>(withdrawal);

                    return HttpResult.Ok(withdrawal, new SharedId(string.Empty));
                
        }

        #endregion

        #region Transfer client

        TransferOptions ITransferRestClient.TransferOptions { get; } = new TransferOptions(_exchangeName, [
            SharedAccountType.PerpetualLinearFutures,
            SharedAccountType.PerpetualInverseFutures,
            SharedAccountType.DeliveryLinearFutures,
            SharedAccountType.DeliveryInverseFutures,
            SharedAccountType.Spot
            ]);
        async Task<HttpResult<SharedId>> ITransferRestClient.TransferAsync(TransferRequest request, CancellationToken ct)
        {
            var validationError = SharedClient.TransferOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedId>(Exchange, validationError);

                    if (!request.Asset.Equals("USDC", StringComparison.InvariantCultureIgnoreCase)
                        && !request.Asset.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return HttpResult.Fail<SharedId>(Exchange, ArgumentError.Invalid("Asset", "invalid asset, only USD is supported"));
                    }

                    var type = GetTransferType(request);
                    if (type == null)
                        return HttpResult.Fail<SharedId>(Exchange, ArgumentError.Invalid("To/From AccountType", "invalid to/from account combination"));

                    // Get data
                    var transfer = await Account.TransferInternalAsync(
                        type.Value,
                        request.Quantity,
                        ct: ct).ConfigureAwait(false);
                    if (!transfer.Success)
                        return HttpResult.Fail<SharedId>(transfer);

                    return HttpResult.Ok(transfer, new SharedId(""));
                
        }

        private TransferDirection? GetTransferType(TransferRequest request)
        {
            if (request.FromAccountType == SharedAccountType.Spot && request.ToAccountType.IsFuturesAccount()) return TransferDirection.SpotToFutures;
            if (request.FromAccountType.IsFuturesAccount() && request.ToAccountType == SharedAccountType.Spot) return TransferDirection.FuturesToSpot;
            return null;
        }

        #endregion
    }
}