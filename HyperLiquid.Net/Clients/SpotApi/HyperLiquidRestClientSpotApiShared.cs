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
        private const string _topicId = "HyperLiquidSpot";
        public string Exchange => "HyperLiquid";

        public TradingMode[] SupportedTradingModes => new[] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

        #region Balance Client
        GetBalancesOptions IBalanceRestClient.GetBalancesOptions { get; } = new GetBalancesOptions(AccountTypeFilter.Spot);

        async Task<ExchangeWebResult<SharedBalance[]>> IBalanceRestClient.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        {
            var validationError = ((IBalanceRestClient)this).GetBalancesOptions.ValidateRequest(Exchange, request, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedBalance[]>(Exchange, validationError);

            var result = await Account.GetBalancesAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedBalance[]>(Exchange, null, default);

            return result.AsExchangeResult<SharedBalance[]>(Exchange, TradingMode.Spot, result.Data.Select(x => new SharedBalance(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Asset), x.Total - x.Hold, x.Total)).ToArray());
        }

        #endregion

        #region Klines Client

        GetKlinesOptions IKlineRestClient.GetKlinesOptions { get; } = new GetKlinesOptions(SharedPaginationSupport.Descending, true, 1000, false,
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

        async Task<ExchangeWebResult<SharedKline[]>> IKlineRestClient.GetKlinesAsync(GetKlinesRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var interval = (Enums.KlineInterval)request.Interval;
            if (!Enum.IsDefined(typeof(Enums.KlineInterval), interval))
                return new ExchangeWebResult<SharedKline[]>(Exchange, ArgumentError.Invalid(nameof(GetKlinesRequest.Interval), "Interval not supported"));

            var validationError = ((IKlineRestClient)this).GetKlinesOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedKline[]>(Exchange, validationError);

            // Determine pagination
            // Data is normally returned oldest first, so to do newest first pagination we have to do some calc
            DateTime endTime = request.EndTime ?? DateTime.UtcNow;
            DateTime? startTime = request.StartTime;
            if (pageToken is DateTimeToken dateTimeToken)
                endTime = dateTimeToken.LastTime;

            var limit = request.Limit ?? 1000;
            if (startTime == null || startTime < endTime)
            {
                var offset = (int)interval * (limit - 1);
                startTime = endTime.AddSeconds(-offset);
            }

            if (startTime < request.StartTime)
                startTime = request.StartTime;

            // Get data
            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.GetKlinesAsync(
                symbol,
                interval,
                startTime ?? DateTime.UtcNow.AddSeconds((-(int)interval) * 100),
                endTime,
                ct: ct
                ).ConfigureAwait(false);
            if (!result)
                return new ExchangeWebResult<SharedKline[]>(Exchange, TradingMode.Spot, result.As<SharedKline[]>(default));

            // Get next token
            DateTimeToken? nextToken = null;
            if (result.Data.Count() == limit)
            {
                var minOpenTime = result.Data.Min(x => x.OpenTime);
                if (request.StartTime == null || minOpenTime > request.StartTime.Value)
                    nextToken = new DateTimeToken(minOpenTime.AddSeconds(-(int)(interval - 1)));
            }

            return result.AsExchangeResult<SharedKline[]>(Exchange, request.Symbol.TradingMode, result.Data.AsEnumerable().Reverse().Select(x => 
                new SharedKline(request.Symbol, symbol, x.OpenTime, x.ClosePrice, x.HighPrice, x.LowPrice, x.OpenPrice, x.Volume)).ToArray(), nextToken);
        }

        #endregion

        #region Order Book client
        GetOrderBookOptions IOrderBookRestClient.GetOrderBookOptions { get; } = new GetOrderBookOptions([20], false);
        async Task<ExchangeWebResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
        {
            var validationError = ((IOrderBookRestClient)this).GetOrderBookOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedOrderBook>(Exchange, validationError);

            var result = await ExchangeData.GetOrderBookAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedOrderBook>(Exchange, null, default);

            if (result.Data == null)
                return result.AsExchangeError<SharedOrderBook>(Exchange, new ServerError(ErrorInfo.Unknown with { Message = "No response" }));

            return result.AsExchangeResult(Exchange, TradingMode.Spot, new SharedOrderBook(result.Data.Levels.Asks, result.Data.Levels.Bids));
        }

        #endregion

        #region Ticker client

        GetTickerOptions ISpotTickerRestClient.GetSpotTickerOptions { get; } = new GetTickerOptions();
        async Task<ExchangeWebResult<SharedSpotTicker>> ISpotTickerRestClient.GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotTickerRestClient)this).GetSpotTickerOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotTicker>(Exchange, validationError);

            var symbolName = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedSpotTicker>(Exchange, null, default);
            
            var symbol = result.Data.Tickers.SingleOrDefault(x => x.Symbol == symbolName);
            if (symbol == null)
                return result.AsExchangeError<SharedSpotTicker>(Exchange, new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return result.AsExchangeResult(Exchange, TradingMode.Spot, new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, symbol.Symbol!), symbol.Symbol!, symbol.MidPrice, null, null, symbol.BaseVolume, (symbol.MidPrice == null || symbol.PreviousDayPrice == 0) ? null : Math.Round((symbol.MidPrice.Value / symbol.PreviousDayPrice * 100 - 100) / 10, 3))
            {
                QuoteVolume = symbol.QuoteVolume
            });
        }

        GetTickersOptions ISpotTickerRestClient.GetSpotTickersOptions { get; } = new GetTickersOptions();
        async Task<ExchangeWebResult<SharedSpotTicker[]>> ISpotTickerRestClient.GetSpotTickersAsync(GetTickersRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotTickerRestClient)this).GetSpotTickersOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotTicker[]>(Exchange, validationError);

            var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedSpotTicker[]>(Exchange, null, default);

            return result.AsExchangeResult<SharedSpotTicker[]>(Exchange, TradingMode.Spot, result.Data.Tickers.Select(x => new SharedSpotTicker(ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol!), x.Symbol!, x.MidPrice, null, null, x.BaseVolume, (x.MidPrice == null || x.PreviousDayPrice == 0) ? null : Math.Round((x.MidPrice.Value / x.PreviousDayPrice * 100 - 100) / 10, 3))
            {
                QuoteVolume = x.QuoteVolume
            }).ToArray());
        }

        #endregion

        #region Book Ticker client

        EndpointOptions<GetBookTickerRequest> IBookTickerRestClient.GetBookTickerOptions { get; } = new EndpointOptions<GetBookTickerRequest>(false);
        async Task<ExchangeWebResult<SharedBookTicker>> IBookTickerRestClient.GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct)
        {
            var validationError = ((IBookTickerRestClient)this).GetBookTickerOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedBookTicker>(Exchange, validationError);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var resultTicker = await ExchangeData.GetOrderBookAsync(symbol, ct: ct).ConfigureAwait(false);
            if (!resultTicker)
                return resultTicker.AsExchangeResult<SharedBookTicker>(Exchange, null, default);

            if (resultTicker.Data == null)
                return resultTicker.AsExchangeError<SharedBookTicker>(Exchange, new ServerError(new ErrorInfo(ErrorType.Unknown, "No response")));

            return resultTicker.AsExchangeResult(Exchange, request.Symbol.TradingMode, new SharedBookTicker(
                ExchangeSymbolCache.ParseSymbol(_topicId, symbol),
                symbol,
                resultTicker.Data.Levels.Asks[0].Price,
                resultTicker.Data.Levels.Asks[0].Quantity,
                resultTicker.Data.Levels.Bids[0].Price,
                resultTicker.Data.Levels.Bids[0].Quantity));
        }

        #endregion

        #region Spot Symbol client
        EndpointOptions<GetSymbolsRequest> ISpotSymbolRestClient.GetSpotSymbolsOptions { get; } = new EndpointOptions<GetSymbolsRequest>(false);

        async Task<ExchangeWebResult<SharedSpotSymbol[]>> ISpotSymbolRestClient.GetSpotSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotSymbolRestClient)this).GetSpotSymbolsOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotSymbol[]>(Exchange, validationError);

            var result = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedSpotSymbol[]>(Exchange, null, default);

            var resultData = result.AsExchangeResult<SharedSpotSymbol[]>(Exchange, TradingMode.Spot, result.Data.Symbols.Select(s => new SharedSpotSymbol(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(s.BaseAsset.Name), HyperLiquidExchange.AssetAliases.ExchangeToCommonName(s.QuoteAsset.Name), s.Name, true)
            {
                MinTradeQuantity = 1m / (decimal)(Math.Pow(10, s.BaseAsset.QuantityDecimals)),
                MinNotionalValue = 10, // Order API returns error mentioning at least 10$ order value, but value isn't returned by symbol API
                QuantityDecimals = s.BaseAsset.QuantityDecimals,
                PriceSignificantFigures = 5,
                PriceDecimals = 8 - s.BaseAsset.QuantityDecimals
            }).ToArray());

            ExchangeSymbolCache.UpdateSymbolInfo(_topicId, resultData.Data);
            return resultData;
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

        PlaceSpotOrderOptions ISpotOrderRestClient.PlaceSpotOrderOptions { get; } = new PlaceSpotOrderOptions()
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(PlaceSpotOrderRequest.Price), typeof(decimal), "Price for the order. For market orders this should be the current symbol price", 21.5m)
            }
        };

        async Task<ExchangeWebResult<SharedId>> ISpotOrderRestClient.PlaceSpotOrderAsync(PlaceSpotOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).PlaceSpotOrderOptions.ValidateRequest(
                Exchange,
                request,
                request.TradingMode,
                SupportedTradingModes,
                ((ISpotOrderRestClient)this).SpotSupportedOrderTypes,
                ((ISpotOrderRestClient)this).SpotSupportedTimeInForce,
                ((ISpotOrderRestClient)this).SpotSupportedOrderQuantity);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            var result = await Trading.PlaceOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                request.OrderType == SharedOrderType.Limit || request.OrderType == SharedOrderType.LimitMaker ? Enums.OrderType.Limit : Enums.OrderType.Market,
                quantity: request.Quantity?.QuantityInBaseAsset ?? 0,
                price: request.Price!.Value,
                timeInForce: GetTimeInForce(request.TimeInForce, request.OrderType),
                clientOrderId: request.ClientOrderId,
                ct: ct).ConfigureAwait(false);

            if (!result)
                return result.AsExchangeResult<SharedId>(Exchange, null, default);

            return result.AsExchangeResult(Exchange, TradingMode.Spot, new SharedId(result.Data.OrderId.ToString()));
        }

        EndpointOptions<GetOrderRequest> ISpotOrderRestClient.GetSpotOrderOptions { get; } = new EndpointOptions<GetOrderRequest>(true);
        async Task<ExchangeWebResult<SharedSpotOrder>> ISpotOrderRestClient.GetSpotOrderAsync(GetOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotOrderOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotOrder>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId))
                return new ExchangeWebResult<SharedSpotOrder>(Exchange, ArgumentError.Invalid(nameof(GetOrderRequest.OrderId), "Invalid order id"));

            var order = await Trading.GetOrderAsync(orderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return order.AsExchangeResult<SharedSpotOrder>(Exchange, null, default);

            return order.AsExchangeResult(Exchange, TradingMode.Spot, new SharedSpotOrder(
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
        }

        EndpointOptions<GetOpenOrdersRequest> ISpotOrderRestClient.GetOpenSpotOrdersOptions { get; } = new EndpointOptions<GetOpenOrdersRequest>(true);
        async Task<ExchangeWebResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetOpenSpotOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetOpenSpotOrdersOptions.ValidateRequest(Exchange, request, request.Symbol?.TradingMode ?? request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotOrder[]>(Exchange, validationError);

            var symbol = request.Symbol?.GetSymbol(FormatSymbol);
            var orders = await Trading.GetOpenOrdersExtendedAsync(ct: ct).ConfigureAwait(false);
            if (!orders)
                return orders.AsExchangeResult<SharedSpotOrder[]>(Exchange, null, default);

            var data = orders.Data.Where(x => x.SymbolType == Enums.SymbolType.Spot);
            if (symbol != null)
                data = data.Where(x => x.Symbol == symbol);

            return orders.AsExchangeResult<SharedSpotOrder[]>(Exchange, TradingMode.Spot, data.Select(x => new SharedSpotOrder(
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
        }

        PaginatedEndpointOptions<GetClosedOrdersRequest> ISpotOrderRestClient.GetClosedSpotOrdersOptions { get; } = new PaginatedEndpointOptions<GetClosedOrdersRequest>(SharedPaginationSupport.NotSupported, false, 2000, true)
        {
            RequestNotes = "API request doesn't allow filtering, so filtering is done client side. This might result in missing historical data as only up to 2000 results are returned from the API"
        };
        async Task<ExchangeWebResult<SharedSpotOrder[]>> ISpotOrderRestClient.GetClosedSpotOrdersAsync(GetClosedOrdersRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetClosedSpotOrdersOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotOrder[]>(Exchange, validationError);

            // Get data
            var orders = await Trading.GetOrderHistoryAsync(ct: ct).ConfigureAwait(false);
            if (!orders)
                return orders.AsExchangeResult<SharedSpotOrder[]>(Exchange, null, default);

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var data = orders.Data.Where(x => 
                x.Order.SymbolType == Enums.SymbolType.Spot 
                && x.Order.Symbol == symbol
                && x.Status != Enums.OrderStatus.Open);
            if (request.Limit != null)
                data = data.Take(request.Limit.Value);

            return orders.AsExchangeResult<SharedSpotOrder[]>(Exchange, TradingMode.Spot, data.Select(x => new SharedSpotOrder(
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
            }).ToArray());
        }

        EndpointOptions<GetOrderTradesRequest> ISpotOrderRestClient.GetSpotOrderTradesOptions { get; } = new EndpointOptions<GetOrderTradesRequest>(true);
        async Task<ExchangeWebResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotOrderTradesOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedUserTrade[]>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId))
                return new ExchangeWebResult<SharedUserTrade[]>(Exchange, ArgumentError.Invalid(nameof(GetOrderTradesRequest.OrderId), "Invalid order id"));

            var orders = await Trading.GetUserTradesAsync(ct: ct).ConfigureAwait(false);
            if (!orders)
                return orders.AsExchangeResult<SharedUserTrade[]>(Exchange, null, default);

            var data = orders.Data.Where(x => x.OrderId == orderId);
            return orders.AsExchangeResult<SharedUserTrade[]>(Exchange, TradingMode.Spot, data.Select(x => new SharedUserTrade(
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
        }

        PaginatedEndpointOptions<GetUserTradesRequest> ISpotOrderRestClient.GetSpotUserTradesOptions { get; } = new PaginatedEndpointOptions<GetUserTradesRequest>(SharedPaginationSupport.Ascending, true, 2000, true)
        {
            RequestNotes = "API request doesn't allow filtering, so filtering is done client side. This might result in missing historical data as only up to 2000 per request / 10000 results in total are returned from the API"
        };
        async Task<ExchangeWebResult<SharedUserTrade[]>> ISpotOrderRestClient.GetSpotUserTradesAsync(GetUserTradesRequest request, INextPageToken? pageToken, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotUserTradesOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedUserTrade[]>(Exchange, validationError);

            // Determine page token
            DateTime? fromTimestamp = null;
            if (pageToken is DateTimeToken dateTimeToken)
                fromTimestamp = dateTimeToken.LastTime;

            // Get data
            var orders = await Trading.GetUserTradesByTimeAsync(
                startTime: fromTimestamp ?? request.StartTime ?? DateTime.UtcNow.AddDays(-7),
                endTime: request.EndTime ?? DateTime.UtcNow,
                ct: ct
                ).ConfigureAwait(false);
            if (!orders)
                return orders.AsExchangeResult<SharedUserTrade[]>(Exchange, null, default);

            var data = orders.Data.Where(x => x.Symbol == request.Symbol!.GetSymbol(FormatSymbol));

            // Get next token
            DateTimeToken? nextToken = null;
            if (orders.Data.Count() == 2000)
                nextToken = new DateTimeToken(orders.Data.Max(o => o.Timestamp).AddMilliseconds(1));

            return orders.AsExchangeResult<SharedUserTrade[]>(Exchange, TradingMode.Spot, data.Select(x => new SharedUserTrade(
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
            }).ToArray(), nextToken);
        }

        EndpointOptions<CancelOrderRequest> ISpotOrderRestClient.CancelSpotOrderOptions { get; } = new EndpointOptions<CancelOrderRequest>(true);
        async Task<ExchangeWebResult<SharedId>> ISpotOrderRestClient.CancelSpotOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).CancelSpotOrderOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            if (!long.TryParse(request.OrderId, out var orderId))
                return new ExchangeWebResult<SharedId>(Exchange, ArgumentError.Invalid(nameof(CancelOrderRequest.OrderId), "Invalid order id"));

            var order = await Trading.CancelOrderAsync(request.Symbol!.GetSymbol(FormatSymbol), orderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return order.AsExchangeResult<SharedId>(Exchange, null, default);

            return order.AsExchangeResult(Exchange, TradingMode.Spot, new SharedId(request.OrderId));
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
            if (status == OrderStatus.Filled) return SharedOrderStatus.Filled;
            return SharedOrderStatus.Canceled;
        }

        private SharedOrderType ParseOrderType(Enums.OrderType type)
        {
            if (type == Enums.OrderType.Market) return SharedOrderType.Market;
            if (type == Enums.OrderType.Limit) return SharedOrderType.Limit;

            return SharedOrderType.Other;
        }

        #endregion

        #region Spot Client Id Order Client

        EndpointOptions<GetOrderRequest> ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdOptions { get; } = new EndpointOptions<GetOrderRequest>(true);
        async Task<ExchangeWebResult<SharedSpotOrder>> ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).GetSpotOrderOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedSpotOrder>(Exchange, validationError);

            var order = await Trading.GetOrderAsync(clientOrderId: request.OrderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return order.AsExchangeResult<SharedSpotOrder>(Exchange, null, default);

            return order.AsExchangeResult(Exchange, TradingMode.Spot, new SharedSpotOrder(
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
        }

        EndpointOptions<CancelOrderRequest> ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdOptions { get; } = new EndpointOptions<CancelOrderRequest>(true);
        async Task<ExchangeWebResult<SharedId>> ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct)
        {
            var validationError = ((ISpotOrderRestClient)this).CancelSpotOrderOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            var order = await Trading.CancelOrderByClientOrderIdAsync(request.Symbol!.GetSymbol(FormatSymbol), clientOrderId: request.OrderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return order.AsExchangeResult<SharedId>(Exchange, null, default);

            return order.AsExchangeResult(Exchange, TradingMode.Spot, new SharedId(request.OrderId));
        }
        #endregion

        #region Asset client
        EndpointOptions<GetAssetsRequest> IAssetsRestClient.GetAssetsOptions { get; } = new EndpointOptions<GetAssetsRequest>(true);

        async Task<ExchangeWebResult<SharedAsset[]>> IAssetsRestClient.GetAssetsAsync(GetAssetsRequest request, CancellationToken ct)
        {
            var validationError = ((IAssetsRestClient)this).GetAssetsOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedAsset[]>(Exchange, validationError);
            
            var assets = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
            if (!assets)
                return assets.AsExchangeResult<SharedAsset[]>(Exchange, null, default);

            return assets.AsExchangeResult<SharedAsset[]>(Exchange, TradingMode.Spot, assets.Data.Assets.Select(x => new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(x.Name))
            {
                FullName = x.FullName,
                Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = x.AssetId
                }]
            }).ToArray());
        }

        EndpointOptions<GetAssetRequest> IAssetsRestClient.GetAssetOptions { get; } = new EndpointOptions<GetAssetRequest>(false);
        async Task<ExchangeWebResult<SharedAsset>> IAssetsRestClient.GetAssetAsync(GetAssetRequest request, CancellationToken ct)
        {
            var validationError = ((IAssetsRestClient)this).GetAssetOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedAsset>(Exchange, validationError);

            var assets = await ExchangeData.GetExchangeInfoAsync(ct: ct).ConfigureAwait(false);
            if (!assets)
                return assets.AsExchangeResult<SharedAsset>(Exchange, null, default);

            var asset = assets.Data.Assets.SingleOrDefault(x => x.Name.Equals(HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset), StringComparison.InvariantCultureIgnoreCase));
            if (asset == null)
                return assets.AsExchangeError<SharedAsset>(Exchange, new ServerError(new ErrorInfo(ErrorType.UnknownAsset, "Asset not found")));

            return assets.AsExchangeResult(Exchange, TradingMode.Spot, new SharedAsset(HyperLiquidExchange.AssetAliases.ExchangeToCommonName(asset.Name))
            {
                FullName = asset.FullName,
                Networks = [new SharedAssetNetwork("HyperLiquid") {
                    ContractAddress = asset.AssetId
                }]
            });
        }

        #endregion

        #region Fee Client
        EndpointOptions<GetFeeRequest> IFeeRestClient.GetFeeOptions { get; } = new EndpointOptions<GetFeeRequest>(true);

        async Task<ExchangeWebResult<SharedFee>> IFeeRestClient.GetFeesAsync(GetFeeRequest request, CancellationToken ct)
        {
            var validationError = ((IFeeRestClient)this).GetFeeOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedFee>(Exchange, validationError);

            // Get data
            var result = await Account.GetFeeInfoAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return result.AsExchangeResult<SharedFee>(Exchange, null, default);

            // Return
            return result.AsExchangeResult(Exchange, TradingMode.Spot, new SharedFee(result.Data.MakerFeeRateSpot * 100, result.Data.TakerFeeRateSpot * 100));
        }
        #endregion

        #region Withdraw client

        WithdrawOptions IWithdrawRestClient.WithdrawOptions { get; } = new WithdrawOptions();
        async Task<ExchangeWebResult<SharedId>> IWithdrawRestClient.WithdrawAsync(WithdrawRequest request, CancellationToken ct)
        {
            var validationError = ((IWithdrawRestClient)this).WithdrawOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            // Get data
            var withdrawal = await Account.TransferSpotAsync(
                request.Address,
                HyperLiquidExchange.AssetAliases.CommonToExchangeName(request.Asset),
                request.Quantity,
                ct: ct).ConfigureAwait(false);
            if (!withdrawal)
                return withdrawal.AsExchangeResult<SharedId>(Exchange, null, default);

            return withdrawal.AsExchangeResult(Exchange, TradingMode.Spot, new SharedId(string.Empty));
        }

        #endregion

        #region Transfer client

        TransferOptions ITransferRestClient.TransferOptions { get; } = new TransferOptions([
            SharedAccountType.PerpetualLinearFutures,
            SharedAccountType.PerpetualInverseFutures,
            SharedAccountType.DeliveryLinearFutures,
            SharedAccountType.DeliveryInverseFutures,
            SharedAccountType.Spot
            ]);
        async Task<ExchangeWebResult<SharedId>> ITransferRestClient.TransferAsync(TransferRequest request, CancellationToken ct)
        {
            var validationError = ((ITransferRestClient)this).TransferOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeWebResult<SharedId>(Exchange, validationError);

            if (!request.Asset.Equals("USDC", StringComparison.InvariantCultureIgnoreCase)
                && !request.Asset.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ExchangeWebResult<SharedId>(Exchange, ArgumentError.Invalid("Asset", "invalid asset, only USD is supported"));
            }

            var type = GetTransferType(request);
            if (type == null)
                return new ExchangeWebResult<SharedId>(Exchange, ArgumentError.Invalid("To/From AccountType", "invalid to/from account combination"));

            // Get data
            var transfer = await Account.TransferInternalAsync(
                type.Value,
                request.Quantity,
                ct: ct).ConfigureAwait(false);
            if (!transfer)
                return transfer.AsExchangeResult<SharedId>(Exchange, null, default);

            return transfer.AsExchangeResult(Exchange, TradingMode.Spot, new SharedId(""));
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
