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
        public string Exchange => "HyperLiquid";

        public TradingMode[] SupportedTradingModes => new[] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();


        #region Balance Client
        GetBalancesOptions IBalanceRestClient.GetBalancesOptions { get; } = new GetBalancesOptions(_exchangeName, AccountTypeFilter.Spot);

        Task<HttpResult<SharedBalance[]>> IBalanceRestClient.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IBalanceRestClient, GetBalancesRequest, SharedBalance[]>(
                this,
                client => client.GetBalancesOptions,
                request,
                async () =>
                {

                    var result = await Account.GetBalancesAsync(ct: ct).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedBalance[]>.Error(result);

                    return SharedExecutionResult<SharedBalance[]>.Ok(result, result.Data.Select(x => new SharedBalance(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Asset), x.Total - x.Hold, x.Total)).ToArray());
                });
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

        Task<HttpResult<SharedKline[]>> IKlineRestClient.GetKlinesAsync(GetKlinesRequest request, PageRequest? pageRequest, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IKlineRestClient, GetKlinesRequest, SharedKline[]>(
                this,
                client => client.GetKlinesOptions,
                request,
                async () =>
                {
                    var interval = (Enums.KlineInterval)request.Interval;
                    if (!Enum.IsDefined(typeof(Enums.KlineInterval), interval))
                        return SharedExecutionResult<SharedKline[]>.Error(ArgumentError.Invalid(nameof(GetKlinesRequest.Interval), "Interval not supported"));

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
                    if (!result)
                        return SharedExecutionResult<SharedKline[]>.Error(result);

                    var nextPageRequest = Pagination.GetNextPageRequest(
                             () => Pagination.NextPageFromTime(pageParams, result.Data.Min(x => x.OpenTime)),
                             result.Data.Length,
                             result.Data.Select(x => x.OpenTime),
                             request.StartTime,
                             request.EndTime ?? DateTime.UtcNow,
                             pageParams);

                    return SharedExecutionResult<SharedKline[]>.Ok(result, ExchangeHelpers.ApplyFilter(result.Data, x => x.OpenTime, request.StartTime, request.EndTime, direction)
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
                });
        }

        #endregion

        #region Order Book client
        GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; } = new GetOrderBookOptions(_exchangeName, [20], false);
        Task<HttpResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IOrderBookRestClient, GetOrderBookRequest, SharedOrderBook>(
                this,
                client => client.GetOrderBookOptions,
                request,
                async () =>
                {

                    var result = await ExchangeData.GetOrderBookAsync(
                        request.Symbol!.GetSymbol(FormatSymbol),
                        ct: ct).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedOrderBook>.Error(result);

                    if (result.Data == null)
                        return SharedExecutionResult<SharedOrderBook>.Error(result.AsError<SharedOrderBook>(new ServerError(ErrorInfo.Unknown with { Message = "No response" })));

                    return SharedExecutionResult<SharedOrderBook>.Ok(result, new SharedOrderBook(result.Data.Levels.Asks, result.Data.Levels.Bids));
                });
        }

        #endregion

        #region Ticker client

        GetSpotTickerOptions ISpotTickerRestClient.GetSpotTickerOptions { get; } = new GetSpotTickerOptions(_exchangeName);
        Task<HttpResult<SharedSpotTicker>> ISpotTickerRestClient.GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotTickerRestClient, GetTickerRequest, SharedSpotTicker>(
                this,
                client => client.GetSpotTickerOptions,
                request,
                async () =>
                {

                    var symbolName = request.Symbol!.GetSymbol(FormatSymbol);
                    var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedSpotTicker>.Error(result);

                    var symbol = result.Data.Tickers.SingleOrDefault(x => x.Symbol == symbolName);
                    if (symbol == null)
                        return SharedExecutionResult<SharedSpotTicker>.Error(result.AsError<SharedSpotTicker>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found"))));

                    return SharedExecutionResult<SharedSpotTicker>.Ok(result, new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, symbol.Symbol!), symbol.Symbol!, symbol.MidPrice, null, null, symbol.BaseVolume, (symbol.MidPrice == null || symbol.PreviousDayPrice == 0) ? null : Math.Round((symbol.MidPrice.Value / symbol.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
                    {
                        QuoteVolume = symbol.QuoteVolume
                    });
                });
        }

        GetSpotTickersOptions ISpotTickerRestClient.GetSpotTickersOptions { get; } = new GetSpotTickersOptions(_exchangeName);
        Task<HttpResult<SharedSpotTicker[]>> ISpotTickerRestClient.GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotTickerRestClient, GetTickersRequest, SharedSpotTicker[]>(
                this,
                client => client.GetSpotTickersOptions,
                request,
                async () =>
                {

                    var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedSpotTicker[]>.Error(result);

                    return SharedExecutionResult<SharedSpotTicker[]>.Ok(result, result.Data.Tickers.Select(x => new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol!), x.Symbol!, x.MidPrice, null, null, x.BaseVolume, (x.MidPrice == null || x.PreviousDayPrice == 0) ? null : Math.Round((x.MidPrice.Value / x.PreviousDayPrice * 100 - 100) / 10, 3) * 10)
                    {
                        QuoteVolume = x.QuoteVolume
                    }).ToArray());
                });
        }

        #endregion

        #region Book Ticker client

        EndpointOptions<GetBookTickerRequest, IBookTickerRestClient> IBookTickerRestClient.GetBookTickerOptions { get; } = new EndpointOptions<GetBookTickerRequest, IBookTickerRestClient>(_exchangeName, false);
        Task<HttpResult<SharedBookTicker>> IBookTickerRestClient.GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IBookTickerRestClient, GetBookTickerRequest, SharedBookTicker>(
                this,
                client => client.GetBookTickerOptions,
                request,
                async () =>
                {

                    var symbol = request.Symbol!.GetSymbol(FormatSymbol);
                    var resultTicker = await ExchangeData.GetOrderBookAsync(symbol, ct: ct).ConfigureAwait(false);
                    if (!resultTicker)
                        return SharedExecutionResult<SharedBookTicker>.Error(resultTicker);

                    if (resultTicker.Data == null)
                        return SharedExecutionResult<SharedBookTicker>.Error(resultTicker.AsError<SharedBookTicker>(new ServerError(new ErrorInfo(ErrorType.Unknown, "No response"))));

                    return SharedExecutionResult<SharedBookTicker>.Ok(resultTicker, new SharedBookTicker(
                        ExchangeSymbolCache.ParseSymbol(_topicId, symbol),
                        symbol,
                        resultTicker.Data.Levels.Asks[0].Price,
                        resultTicker.Data.Levels.Asks[0].Quantity,
                        resultTicker.Data.Levels.Bids[0].Price,
                        resultTicker.Data.Levels.Bids[0].Quantity));
                });
        }

        #endregion

        #region Spot Symbol client
        EndpointOptions<GetSymbolsRequest, ISpotSymbolRestClient> ISpotSymbolRestClient.GetSpotSymbolsOptions { get; } = new EndpointOptions<GetSymbolsRequest, ISpotSymbolRestClient>(_exchangeName, false);

        Task<HttpResult<SharedSpotSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotSymbolRestClient, GetSymbolsRequest, SharedSpotSymbol[]>(
                this,
                client => client.GetSpotSymbolsOptions,
                request,
                async () =>
                {

                    var result = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedSpotSymbol[]>.Error(result);

                    var resultData = result.Data.Symbols.Select(s => new SharedSpotSymbol(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(s.BaseAsset.Name), HyperLiquidExchange.AssetAliases.ExchangeToCommonName(s.QuoteAsset.Name), s.Name, true)
                    {
                        MinTradeQuantity = 1m / (decimal)(Math.Pow(10, s.BaseAsset.QuantityDecimals)),
                        MinNotionalValue = 10, // Order API returns error mentioning at least 10$ order value, but value isn't returned by symbol API
                        QuantityDecimals = s.BaseAsset.QuantityDecimals,
                        PriceSignificantFigures = 5,
                        PriceDecimals = 8 - s.BaseAsset.QuantityDecimals
                    }).ToArray();

                    ExchangeSymbolCache.UpdateSymbolInfo(_topicId, resultData);
                    return SharedExecutionResult<SharedSpotSymbol[]>.Ok(result, resultData);
                });
        }

        async Task<WebSocketResult<SharedSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsForBaseAssetAsync(string baseAsset)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId))
            {
                var symbols = await ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols)
                    return new WebSocketResult<SharedSymbol[]>(Exchange, symbols.Error!);
            }

            return new WebSocketResult<SharedSymbol[]>(Exchange, ExchangeSymbolCache.GetSymbolsForBaseAsset(_topicId, baseAsset));
        }

        async Task<WebSocketResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(SharedSymbol symbol)
        {
            if (symbol.TradingMode != TradingMode.Spot)
                throw new ArgumentException(nameof(symbol), "Only Spot symbols allowed");

            if (!ExchangeSymbolCache.HasCached(_topicId))
            {
                var symbols = await ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols)
                    return new WebSocketResult<bool>(Exchange, symbols.Error!);
            }

            return new WebSocketResult<bool>(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, symbol));
        }

        async Task<WebSocketResult<bool>> ISpotSymbolRestClient.SupportsSpotSymbolAsync(string symbolName)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId))
            {
                var symbols = await ((ISpotSymbolRestClient)this).GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols)
                    return new WebSocketResult<bool>(Exchange, symbols.Error!);
            }

            return new WebSocketResult<bool>(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, symbolName));
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
            }
        };

        Task<HttpResult<SharedId>> ISpotOrderRestClient.PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, PlaceSpotOrderRequest, SharedId>(
                this,
                client => client.PlaceSpotOrderOptions,
                request,
                async () =>
                {

                    var result = await Trading.PlaceOrderAsync(
                        request.Symbol!.GetSymbol(FormatSymbol),
                        request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                        request.OrderType == SharedOrderType.Limit || request.OrderType == SharedOrderType.LimitMaker ? Enums.OrderType.Limit : Enums.OrderType.Market,
                        quantity: request.Quantity?.QuantityInBaseAsset ?? 0,
                        price: request.Price!.Value,
                        timeInForce: GetTimeInForce(request.TimeInForce, request.OrderType),
                        clientOrderId: request.ClientOrderId,
                        rawParameter: request.ExchangeParameters?.GetRawParameters(Exchange),
                        ct: ct).ConfigureAwait(false);

                    if (!result)
                        return SharedExecutionResult<SharedId>.Error(result);

                    return SharedExecutionResult<SharedId>.Ok(result, new SharedId(result.Data.OrderId.ToString()));
                });
        }

        EndpointOptions<GetOrderRequest, ISpotOrderRestClient> ISpotOrderRestClient.GetSpotOrderOptions { get; } = new EndpointOptions<GetOrderRequest, ISpotOrderRestClient>(_exchangeName, true);
        Task<HttpResult<SharedSpotOrder>> ISpotOrderRestClient.GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, GetOrderRequest, SharedSpotOrder>(
                this,
                client => client.GetSpotOrderOptions,
                request,
                async () =>
                {

                    if (!long.TryParse(request.OrderId, out var orderId))
                        return SharedExecutionResult<SharedSpotOrder>.Error(ArgumentError.Invalid(nameof(GetOrderRequest.OrderId), "Invalid order id"));

                    var order = await Trading.GetOrderAsync(orderId, ct: ct).ConfigureAwait(false);
                    if (!order)
                        return SharedExecutionResult<SharedSpotOrder>.Error(order);

                    return SharedExecutionResult<SharedSpotOrder>.Ok(order, new SharedSpotOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, order.Data.Order.Symbol!),
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
                });
        }

        EndpointOptions<GetOpenOrdersRequest, ISpotOrderRestClient> ISpotOrderRestClient.GetOpenSpotOrdersOptions { get; } = new EndpointOptions<GetOpenOrdersRequest, ISpotOrderRestClient>(_exchangeName, true);
        Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, GetOpenOrdersRequest, SharedSpotOrder[]>(
                this,
                client => client.GetOpenSpotOrdersOptions,
                request,
                async () =>
                {

                    var symbol = request.Symbol?.GetSymbol(FormatSymbol);
                    var orders = await Trading.GetOpenOrdersExtendedAsync(ct: ct).ConfigureAwait(false);
                    if (!orders)
                        return SharedExecutionResult<SharedSpotOrder[]>.Error(orders);

                    var data = orders.Data.Where(x => x.SymbolType == Enums.SymbolType.Spot);
                    if (symbol != null)
                        data = data.Where(x => x.Symbol == symbol);

                    return SharedExecutionResult<SharedSpotOrder[]>.Ok(orders, data.Select(x => new SharedSpotOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol),
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
                });
        }

        GetSpotClosedOrdersOptions ISpotOrderRestClient.GetClosedSpotOrdersOptions { get; } = new GetSpotClosedOrdersOptions(_exchangeName, true, true, false, 2000)
        {
            RequestNotes = "API request doesn't allow filtering, so filtering is done client side. This might result in missing historical data as only up to 2000 results are returned from the API"
        };
        Task<HttpResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, PageRequest? pageToken, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, GetClosedOrdersRequest, SharedSpotOrder[]>(
                this,
                client => client.GetClosedSpotOrdersOptions,
                request,
                async () =>
                {

                    // Get data
                    var orders = await Trading.GetOrderHistoryAsync(ct: ct).ConfigureAwait(false);
                    if (!orders)
                        return SharedExecutionResult<SharedSpotOrder[]>.Error(orders);

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

                    return SharedExecutionResult<SharedSpotOrder[]>.Ok(orders, ExchangeHelpers.ApplyFilter(data, x => x.Timestamp, request.StartTime, request.EndTime, direction)
                                .Select(x =>
                                    new SharedSpotOrder(
                                        ExchangeSymbolCache.ParseSymbol(_topicId, x.Order.Symbol),
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
                });
        }

        EndpointOptions<GetOrderTradesRequest, ISpotOrderRestClient> ISpotOrderRestClient.GetSpotOrderTradesOptions { get; } = new EndpointOptions<GetOrderTradesRequest, ISpotOrderRestClient>(_exchangeName, true);
        Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, GetOrderTradesRequest, SharedUserTrade[]>(
                this,
                client => client.GetSpotOrderTradesOptions,
                request,
                async () =>
                {

                    if (!long.TryParse(request.OrderId, out var orderId))
                        return SharedExecutionResult<SharedUserTrade[]>.Error(ArgumentError.Invalid(nameof(GetOrderTradesRequest.OrderId), "Invalid order id"));

                    var orders = await Trading.GetUserTradesAsync(ct: ct).ConfigureAwait(false);
                    if (!orders)
                        return SharedExecutionResult<SharedUserTrade[]>.Error(orders);

                    var data = orders.Data.Where(x => x.OrderId == orderId);
                    return SharedExecutionResult<SharedUserTrade[]>.Ok(orders, data.Select(x => new SharedUserTrade(
                        ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol),
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
                });
        }

        GetSpotUserTradesOptions ISpotOrderRestClient.GetSpotUserTradesOptions { get; } = new GetSpotUserTradesOptions(_exchangeName, true, false, true, 2000)
        {
            RequestNotes = "API request doesn't allow filtering, so filtering is done client side. This might result in missing historical data as only up to 2000 per request / 10000 results in total are returned from the API"
        };
        Task<HttpResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotUserTradesAsync(GetUserTradesRequest request, PageRequest? pageRequest, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, GetUserTradesRequest, SharedUserTrade[]>(
                this,
                client => client.GetSpotUserTradesOptions,
                request,
                async () =>
                {

                    int limit = request.Limit ?? 2000;
                    var direction = DataDirection.Ascending;
                    var pageParams = Pagination.GetPaginationParameters(direction, limit, request.StartTime, request.EndTime ?? DateTime.UtcNow, pageRequest, false);

                    // Get data
                    var result = await Trading.GetUserTradesByTimeAsync(
                        startTime: pageParams.StartTime ?? DateTime.UtcNow.AddDays(-7),
                        ct: ct
                        ).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedUserTrade[]>.Error(result);

                    var data = result.Data.Where(x => x.Symbol == request.Symbol!.GetSymbol(FormatSymbol));
                    var nextPageRequest = Pagination.GetNextPageRequest(
                             () => Pagination.NextPageFromTime(pageParams, result.Data.Max(x => x.Timestamp)),
                             result.Data.Length,
                             result.Data.Select(x => x.Timestamp),
                             request.StartTime,
                             request.EndTime ?? DateTime.UtcNow,
                             pageParams);

                    return SharedExecutionResult<SharedUserTrade[]>.Ok(result, ExchangeHelpers.ApplyFilter(data, x => x.Timestamp, request.StartTime, request.EndTime, direction)
                            .Select(x =>
                                new SharedUserTrade(
                                ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol),
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
                });
        }

        EndpointOptions<CancelOrderRequest, ISpotOrderRestClient> ISpotOrderRestClient.CancelSpotOrderOptions { get; } = new EndpointOptions<CancelOrderRequest, ISpotOrderRestClient>(_exchangeName, true);
        Task<HttpResult<SharedId>> ISpotOrderRestClient.CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderRestClient, CancelOrderRequest, SharedId>(
                this,
                client => client.CancelSpotOrderOptions,
                request,
                async () =>
                {

                    if (!long.TryParse(request.OrderId, out var orderId))
                        return SharedExecutionResult<SharedId>.Error(ArgumentError.Invalid(nameof(CancelOrderRequest.OrderId), "Invalid order id"));

                    var order = await Trading.CancelOrderAsync(request.Symbol!.GetSymbol(FormatSymbol), orderId, ct: ct).ConfigureAwait(false);
                    if (!order)
                        return SharedExecutionResult<SharedId>.Error(order);

                    return SharedExecutionResult<SharedId>.Ok(order, new SharedId(request.OrderId));
                });
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

        EndpointOptions<GetOrderRequest, ISpotOrderClientIdRestClient> ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdOptions { get; } = new EndpointOptions<GetOrderRequest, ISpotOrderClientIdRestClient>(_exchangeName, true);
        Task<HttpResult<SharedSpotOrder>> ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderClientIdRestClient, GetOrderRequest, SharedSpotOrder>(
                this,
                client => client.GetSpotOrderByClientOrderIdOptions,
                request,
                async () =>
                {

                    var order = await Trading.GetOrderAsync(clientOrderId: request.OrderId, ct: ct).ConfigureAwait(false);
                    if (!order)
                        return SharedExecutionResult<SharedSpotOrder>.Error(order);

                    return SharedExecutionResult<SharedSpotOrder>.Ok(order, new SharedSpotOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, order.Data.Order.Symbol!),
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
                });
        }

        EndpointOptions<CancelOrderRequest, ISpotOrderClientIdRestClient> ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdOptions { get; } = new EndpointOptions<CancelOrderRequest, ISpotOrderClientIdRestClient>(_exchangeName, true);
        Task<HttpResult<SharedId>> ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ISpotOrderClientIdRestClient, CancelOrderRequest, SharedId>(
                this,
                client => client.CancelSpotOrderByClientOrderIdOptions,
                request,
                async () =>
                {

                    var order = await Trading.CancelOrderByClientOrderIdAsync(request.Symbol!.GetSymbol(FormatSymbol), clientOrderId: request.OrderId, ct: ct).ConfigureAwait(false);
                    if (!order)
                        return SharedExecutionResult<SharedId>.Error(order);

                    return SharedExecutionResult<SharedId>.Ok(order, new SharedId(request.OrderId));
                });
        }
        #endregion

        #region Asset client
        EndpointOptions<GetAssetsRequest, IAssetsRestClient> IAssetsRestClient.GetAssetsOptions { get; } = new EndpointOptions<GetAssetsRequest, IAssetsRestClient>(_exchangeName, true);

        Task<HttpResult<SharedAsset[]>> IAssetsRestClient.GetAssetsAsync(GetAssetsRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IAssetsRestClient, GetAssetsRequest, SharedAsset[]>(
                this,
                client => client.GetAssetsOptions,
                request,
                async () =>
                {

                    var assets = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!assets)
                        return SharedExecutionResult<SharedAsset[]>.Error(assets);

                    return SharedExecutionResult<SharedAsset[]>.Ok(assets, assets.Data.Assets.Select(x => new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Name))
                    {
                        FullName = x.FullName,
                        Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = x.AssetId
                }]
                    }).ToArray());
                });
        }

        EndpointOptions<GetAssetRequest, IAssetsRestClient> IAssetsRestClient.GetAssetOptions { get; } = new EndpointOptions<GetAssetRequest, IAssetsRestClient>(_exchangeName, false);
        Task<HttpResult<SharedAsset>> IAssetsRestClient.GetAssetAsync(GetAssetRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IAssetsRestClient, GetAssetRequest, SharedAsset>(
                this,
                client => client.GetAssetOptions,
                request,
                async () =>
                {

                    var assets = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!assets)
                        return SharedExecutionResult<SharedAsset>.Error(assets);

                    var asset = assets.Data.Assets.SingleOrDefault(x => x.Name.Equals(HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset), StringComparison.InvariantCultureIgnoreCase));
                    if (asset == null)
                        return SharedExecutionResult<SharedAsset>.Error(assets.AsError<SharedAsset>(new ServerError(new ErrorInfo(ErrorType.UnknownAsset, "Asset not found"))));

                    return SharedExecutionResult<SharedAsset>.Ok(assets, new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(asset.Name))
                    {
                        FullName = asset.FullName,
                        Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = asset.AssetId
                }]
                    });
                });
        }

        #endregion

        #region Fee Client
        EndpointOptions<GetFeeRequest, IFeeRestClient> IFeeRestClient.GetFeeOptions { get; } = new EndpointOptions<GetFeeRequest, IFeeRestClient>(_exchangeName, true);

        Task<HttpResult<SharedFee>> IFeeRestClient.GetFeesAsync(GetFeeRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFeeRestClient, GetFeeRequest, SharedFee>(
                this,
                client => client.GetFeeOptions,
                request,
                async () =>
                {

                    // Get data
                    var result = await Account.GetFeeInfoAsync(ct: ct).ConfigureAwait(false);
                    if (!result)
                        return SharedExecutionResult<SharedFee>.Error(result);

                    // Return
                    return SharedExecutionResult<SharedFee>.Ok(result, new SharedFee(result.Data.MakerFeeRateSpot * 100, result.Data.TakerFeeRateSpot * 100));
                });
        }
        #endregion

        #region Withdraw client

        WithdrawOptions IWithdrawRestClient.WithdrawOptions { get; } = new WithdrawOptions(_exchangeName);
        Task<HttpResult<SharedId>> IWithdrawRestClient.WithdrawAsync(WithdrawRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IWithdrawRestClient, WithdrawRequest, SharedId>(
                this,
                client => client.WithdrawOptions,
                request,
                async () =>
                {

                    // Get data
                    var withdrawal = await Account.TransferSpotAsync(
                        request.Address,
                        HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset),
                        request.Quantity,
                        ct: ct).ConfigureAwait(false);
                    if (!withdrawal)
                        return SharedExecutionResult<SharedId>.Error(withdrawal);

                    return SharedExecutionResult<SharedId>.Ok(withdrawal, new SharedId(string.Empty));
                });
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
        Task<HttpResult<SharedId>> ITransferRestClient.TransferAsync(TransferRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ITransferRestClient, TransferRequest, SharedId>(
                this,
                client => client.TransferOptions,
                request,
                async () =>
                {

                    if (!request.Asset.Equals("USDC", StringComparison.InvariantCultureIgnoreCase)
                        && !request.Asset.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return SharedExecutionResult<SharedId>.Error(ArgumentError.Invalid("Asset", "invalid asset, only USD is supported"));
                    }

                    var type = GetTransferType(request);
                    if (type == null)
                        return SharedExecutionResult<SharedId>.Error(ArgumentError.Invalid("To/From AccountType", "invalid to/from account combination"));

                    // Get data
                    var transfer = await Account.TransferInternalAsync(
                        type.Value,
                        request.Quantity,
                        ct: ct).ConfigureAwait(false);
                    if (!transfer)
                        return SharedExecutionResult<SharedId>.Error(transfer);

                    return SharedExecutionResult<SharedId>.Ok(transfer, new SharedId(""));
                });
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