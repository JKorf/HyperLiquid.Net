using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using CryptoExchange.Net.Objects;
using System.Linq;
using HyperLiquid.Net.Enums;
using CryptoExchange.Net;
using HyperLiquid.Net.Objects.Models;
using CryptoExchange.Net.Objects.Errors;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    internal partial class HyperLiquidRestClientFuturesApi : IHyperLiquidRestClientFuturesApiShared
    {
        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidFutures";
        public string Exchange => "HyperLiquid";

        public TradingMode[] SupportedTradingModes => new[] { TradingMode.PerpetualLinear };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();


        #region Balance Client
        GetBalancesOptions IBalanceRestClient.GetBalancesOptions { get; } = new GetBalancesOptions(_exchangeName, AccountTypeFilter.Futures)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("dex", typeof(string), "DEX to retrieve balances for", "xyz")
            }
        };

        Task<ExchangeWebResult<SharedBalance[]>> IBalanceRestClient.GetBalancesAsync(GetBalancesRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IBalanceRestClient, GetBalancesRequest, SharedBalance[]>(
                this,
                client => client.GetBalancesOptions,
                request,
                async () =>
                {

            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            var result = await Account.GetAccountInfoAsync(dex: dex, ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedBalance[]>.Error(result);

            return SharedExecutionResult<SharedBalance[]>.Ok(result, [
                new SharedBalance(
                    "USDC",
                    result.Data.MarginSummary.AccountValue - result.Data.MarginSummary.TotalMarginUsed,
                    result.Data.MarginSummary.AccountValue)
                ]);
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

        Task<ExchangeWebResult<SharedKline[]>> IKlineRestClient.GetKlinesAsync(GetKlinesRequest request, PageRequest? pageRequest, CancellationToken ct)
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
        Task<ExchangeWebResult<SharedOrderBook>> IOrderBookRestClient.GetOrderBookAsync(GetOrderBookRequest request, CancellationToken ct)
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

            return SharedExecutionResult<SharedOrderBook>.Ok(result, new SharedOrderBook(result.Data.Levels.Asks, result.Data.Levels.Bids));
                                });
        }

        #endregion

        #region Ticker client

        GetFuturesTickerOptions IFuturesTickerRestClient.GetFuturesTickerOptions { get; } = new GetFuturesTickerOptions(_exchangeName);
        Task<ExchangeWebResult<SharedFuturesTicker>> IFuturesTickerRestClient.GetFuturesTickerAsync(GetTickerRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesTickerRestClient, GetTickerRequest, SharedFuturesTicker>(
                this,
                client => client.GetFuturesTickerOptions,
                request,
                async () =>
                {

            var symbolName = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedFuturesTicker>.Error(result);

            var symbol = result.Data.Tickers.SingleOrDefault(x => x.Symbol == symbolName);
            if (symbol == null)
                return SharedExecutionResult<SharedFuturesTicker>.Error(result.AsError<SharedFuturesTicker>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found"))));

            return SharedExecutionResult<SharedFuturesTicker>.Ok(result, new SharedFuturesTicker(ExchangeSymbolCache.ParseSymbol(_topicId, symbol.Symbol), symbol.Symbol, symbol.MidPrice, null, null, symbol.NotionalVolume, (symbol.MidPrice == null || symbol.PreviousDayPrice == 0) ? null : Math.Round((symbol.MidPrice.Value / symbol.PreviousDayPrice * 100 - 100), 3))
            {
                FundingRate = symbol.FundingRate,
                MarkPrice = symbol.MarkPrice
            });
                                });
        }

        GetFuturesTickersOptions IFuturesTickerRestClient.GetFuturesTickersOptions { get; } = new GetFuturesTickersOptions(_exchangeName);
        Task<ExchangeWebResult<SharedFuturesTicker[]>> IFuturesTickerRestClient.GetFuturesTickersAsync(GetTickersRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesTickerRestClient, GetTickersRequest, SharedFuturesTicker[]>(
                this,
                client => client.GetFuturesTickersOptions,
                request,
                async () =>
                {

            var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedFuturesTicker[]>.Error(result);

            return SharedExecutionResult<SharedFuturesTicker[]>.Ok(result, result.Data.Tickers.Select(x => new SharedFuturesTicker(ExchangeSymbolCache.ParseSymbol(_topicId, x.Symbol), x.Symbol, x.MidPrice, null, null, x.NotionalVolume, (x.MidPrice == null || x.PreviousDayPrice == 0) ? null : Math.Round((x.MidPrice.Value / x.PreviousDayPrice * 100 - 100), 3))
            {
                FundingRate = x.FundingRate,
                MarkPrice = x.MarkPrice
            }).ToArray());
                                });
        }

        #endregion

        #region Book Ticker client

        EndpointOptions<GetBookTickerRequest, IBookTickerRestClient> IBookTickerRestClient.GetBookTickerOptions { get; } = new EndpointOptions<GetBookTickerRequest, IBookTickerRestClient>(_exchangeName, false);
        Task<ExchangeWebResult<SharedBookTicker>> IBookTickerRestClient.GetBookTickerAsync(GetBookTickerRequest request, CancellationToken ct)
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

        #region Futures Symbol client
        EndpointOptions<GetSymbolsRequest, IFuturesSymbolRestClient> IFuturesSymbolRestClient.GetFuturesSymbolsOptions { get; } = new EndpointOptions<GetSymbolsRequest, IFuturesSymbolRestClient>(_exchangeName, false)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("dex", typeof(string), "DEX to retrieve symbols for", "xyz")
            }
        };

        Task<ExchangeWebResult<SharedFuturesSymbol[]>> IFuturesSymbolRestClient.GetFuturesSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesSymbolRestClient, GetSymbolsRequest, SharedFuturesSymbol[]>(
                this,
                client => client.GetFuturesSymbolsOptions,
                request,
                async () =>
                {

            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            var result = await ExchangeData.GetExchangeInfoAllDexesAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedFuturesSymbol[]>.Error(result);

            var data = result.Data.SelectMany(x => x.Symbols);
            if (dex != null)
                data = result.Data.SingleOrDefault(x => x.Name == dex)?.Symbols ?? [];

            var response = data.Where(x => !x.IsDelisted).Select(ParseSymbol).ToArray();

            // Register both HYPE/USDC and HYPE as symbol names
            var symbolRegistrations = result.Data.SelectMany(x => x.Symbols.Where(x => !x.IsDelisted).Select(ParseSymbol))
                .Concat(result.Data.SelectMany(x => x.Symbols.Select(x => new SharedSpotSymbol(x.Name, "USDC", x.Name, true, TradingMode.PerpetualLinear)))).ToArray();

            ExchangeSymbolCache.UpdateSymbolInfo(_topicId, symbolRegistrations);
            return SharedExecutionResult<SharedFuturesSymbol[]>.Ok(result, response);
                                });
        }

        private SharedFuturesSymbol ParseSymbol(HyperLiquidFuturesSymbol symbol)
        {
            return new SharedFuturesSymbol(
                TradingMode.PerpetualLinear,
                symbol.Name,
                "USDC",
                symbol.Name + "/USDC",
                true)
            {
                MinTradeQuantity = 1m / (decimal)(Math.Pow(10, symbol.QuantityDecimals)),
                MinNotionalValue = 10, // Order API returns error mentioning at least 10$ order value, but value isn't returned by symbol API
                QuantityDecimals = symbol.QuantityDecimals,
                PriceSignificantFigures = 5,
                PriceDecimals = 6 - symbol.QuantityDecimals,
                MaxLongLeverage = symbol.MaxLeverage,
                MaxShortLeverage = symbol.MaxLeverage
            };
        }

        async Task<ExchangeResult<SharedSymbol[]>> IFuturesSymbolRestClient.GetFuturesSymbolsForBaseAssetAsync(string baseAsset)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId))
            {
                var symbols = await ((IFuturesSymbolRestClient)this).GetFuturesSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols)
                    return new ExchangeResult<SharedSymbol[]>(Exchange, symbols.Error!);
            }

            return new ExchangeResult<SharedSymbol[]>(Exchange, ExchangeSymbolCache.GetSymbolsForBaseAsset(_topicId, baseAsset));
        }

        async Task<ExchangeResult<bool>> IFuturesSymbolRestClient.SupportsFuturesSymbolAsync(SharedSymbol symbol)
        {
            if (symbol.TradingMode == TradingMode.Spot)
                throw new ArgumentException(nameof(symbol), "Spot symbols not allowed");

            if (!ExchangeSymbolCache.HasCached(_topicId))
            {
                var symbols = await ((IFuturesSymbolRestClient)this).GetFuturesSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols)
                    return new ExchangeResult<bool>(Exchange, symbols.Error!);
            }

            return new ExchangeResult<bool>(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, symbol));
        }

        async Task<ExchangeResult<bool>> IFuturesSymbolRestClient.SupportsFuturesSymbolAsync(string symbolName)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId))
            {
                var symbols = await ((IFuturesSymbolRestClient)this).GetFuturesSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
                if (!symbols)
                    return new ExchangeResult<bool>(Exchange, symbols.Error!);
            }

            return new ExchangeResult<bool>(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, symbolName));
        }
        #endregion

        #region Fee Client
        EndpointOptions<GetFeeRequest, IFeeRestClient> IFeeRestClient.GetFeeOptions { get; } = new EndpointOptions<GetFeeRequest, IFeeRestClient>(_exchangeName, true);

        Task<ExchangeWebResult<SharedFee>> IFeeRestClient.GetFeesAsync(GetFeeRequest request, CancellationToken ct)
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
            return SharedExecutionResult<SharedFee>.Ok(result, new SharedFee(result.Data.MakerFeeRate * 100, result.Data.TakerFeeRate * 100));
                                });
        }
        #endregion

        #region Funding Rate client
        GetFundingRateHistoryOptions IFundingRateRestClient.GetFundingRateHistoryOptions { get; } = new GetFundingRateHistoryOptions(_exchangeName, true, false, true, 500, false);

        Task<ExchangeWebResult<SharedFundingRate[]>> IFundingRateRestClient.GetFundingRateHistoryAsync(GetFundingRateHistoryRequest request, PageRequest? pageToken, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFundingRateRestClient, GetFundingRateHistoryRequest, SharedFundingRate[]>(
                this,
                client => client.GetFundingRateHistoryOptions,
                request,
                async () =>
                {

            //DateTime? fromTime = null;
            //if (pageToken is DateTimeToken token)
            //    fromTime = token.LastTime;

            // Get data
            var result = await ExchangeData.GetFundingRateHistoryAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.StartTime ?? DateTime.UtcNow.AddDays(-30),
                //endTime: request.EndTime,
                ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedFundingRate[]>.Error(result);

            //DateTimeToken? nextToken = null;
            //if (result.Data.Count() == (request.Limit ?? 500))
            //    nextToken = new DateTimeToken(result.Data.Max(x => x.Timestamp).AddSeconds(1));

            // Return
            return SharedExecutionResult<SharedFundingRate[]>.Ok(result, result.Data.Select(x => new SharedFundingRate(x.FundingRate, x.Timestamp)).ToArray()/*, nextToken*/);
                                });
        }
        #endregion

        #region Leverage client
        SharedLeverageSettingMode ILeverageRestClient.LeverageSettingType => SharedLeverageSettingMode.PerSymbol;

        EndpointOptions<GetLeverageRequest, ILeverageRestClient> ILeverageRestClient.GetLeverageOptions { get; } = new EndpointOptions<GetLeverageRequest, ILeverageRestClient>(_exchangeName, true)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("dex", typeof(string), "DEX to retrieve leverage for", "xyz")
            }
        };
        Task<ExchangeWebResult<SharedLeverage>> ILeverageRestClient.GetLeverageAsync(GetLeverageRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ILeverageRestClient, GetLeverageRequest, SharedLeverage>(
                this,
                client => client.GetLeverageOptions,
                request,
                async () =>
                {

            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            var result = await Account.GetAccountInfoAsync(dex: dex, ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedLeverage>.Error(result);

            var position = result.Data.Positions.SingleOrDefault(x => x.Position.Symbol == request.Symbol!.GetSymbol(FormatSymbol));
            if (position == null)
                return SharedExecutionResult<SharedLeverage>.Error(result.AsError<SharedLeverage>(new ServerError(new ErrorInfo(ErrorType.NoPosition, "Not found"))));

            return SharedExecutionResult<SharedLeverage>.Ok(result, new SharedLeverage(position.Position.Leverage!.Value));
                                });
        }

        SetLeverageOptions ILeverageRestClient.SetLeverageOptions { get; } = new SetLeverageOptions(_exchangeName)
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(SetLeverageRequest.MarginMode), typeof(SharedMarginMode), "Margin mode", SharedMarginMode.Isolated)
            }
        };
        Task<ExchangeWebResult<SharedLeverage>> ILeverageRestClient.SetLeverageAsync(SetLeverageRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<ILeverageRestClient, SetLeverageRequest, SharedLeverage>(
                this,
                client => client.SetLeverageOptions,
                request,
                async () =>
                {

            var result = await Trading.SetLeverageAsync(symbol: request.Symbol!.GetSymbol(FormatSymbol), (int)request.Leverage, request.MarginMode == SharedMarginMode.Isolated ? Enums.MarginType.Isolated : Enums.MarginType.Cross, ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedLeverage>.Error(result);

            return SharedExecutionResult<SharedLeverage>.Ok(result, new SharedLeverage(request.Leverage)
            {
                MarginMode = request.MarginMode
            });
                                });
        }
        #endregion

        #region Open Interest client

        EndpointOptions<GetOpenInterestRequest, IOpenInterestRestClient> IOpenInterestRestClient.GetOpenInterestOptions { get; } = new EndpointOptions<GetOpenInterestRequest, IOpenInterestRestClient>(_exchangeName, true);
        Task<ExchangeWebResult<SharedOpenInterest>> IOpenInterestRestClient.GetOpenInterestAsync(GetOpenInterestRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IOpenInterestRestClient, GetOpenInterestRequest, SharedOpenInterest>(
                this,
                client => client.GetOpenInterestOptions,
                request,
                async () =>
                {

            var result = await ExchangeData.GetExchangeInfoAndTickersAsync(ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedOpenInterest>.Error(result);

            var ticker = result.Data.Tickers.SingleOrDefault(x => x.Symbol == request.Symbol!.GetSymbol(FormatSymbol));
            if (ticker == null)
                return SharedExecutionResult<SharedOpenInterest>.Error(result.AsError<SharedOpenInterest>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found"))));

            return SharedExecutionResult<SharedOpenInterest>.Ok(result, new SharedOpenInterest(ticker.OpenInterest ?? 0));
                                });
        }

        #endregion

        #region Futures Order Client

        SharedFeeDeductionType IFuturesOrderRestClient.FuturesFeeDeductionType => SharedFeeDeductionType.AddToCost;
        SharedFeeAssetType IFuturesOrderRestClient.FuturesFeeAssetType => SharedFeeAssetType.QuoteAsset;

        SharedOrderType[] IFuturesOrderRestClient.FuturesSupportedOrderTypes { get; } = new[] { SharedOrderType.Limit, SharedOrderType.Market };
        SharedTimeInForce[] IFuturesOrderRestClient.FuturesSupportedTimeInForce { get; } = new[] { SharedTimeInForce.GoodTillCanceled, SharedTimeInForce.ImmediateOrCancel };
        SharedQuantitySupport IFuturesOrderRestClient.FuturesSupportedOrderQuantity { get; } = new SharedQuantitySupport(
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset,
                SharedQuantityType.BaseAsset);

        string IFuturesOrderRestClient.GenerateClientOrderId() => ExchangeHelpers.RandomHexString(16)!.ToLowerInvariant();

        PlaceFuturesOrderOptions IFuturesOrderRestClient.PlaceFuturesOrderOptions { get; } = new PlaceFuturesOrderOptions(_exchangeName, false)
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(PlaceSpotOrderRequest.Price), typeof(decimal), "Price for the order. For market orders this should be the current symbol price", 21.5m)
            }
        };
        Task<ExchangeWebResult<SharedId>> IFuturesOrderRestClient.PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, PlaceFuturesOrderRequest, SharedId>(
                this,
                client => client.PlaceFuturesOrderOptions,
                request,
                async () =>
                {

            var result = await Trading.PlaceOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.Side == SharedOrderSide.Buy ? Enums.OrderSide.Buy : Enums.OrderSide.Sell,
                request.OrderType == SharedOrderType.Limit || request.OrderType == SharedOrderType.LimitMaker ? Enums.OrderType.Limit : Enums.OrderType.Market,
                quantity: request.Quantity?.QuantityInBaseAsset ?? request.Quantity?.QuantityInContracts ?? 0,
                price: request.Price!.Value,
                reduceOnly: request.ReduceOnly,
                timeInForce: GetTimeInForce(request.TimeInForce, request.OrderType),
                clientOrderId: request.ClientOrderId,
                ct: ct).ConfigureAwait(false);

            if (!result)
                return SharedExecutionResult<SharedId>.Error(result);

            return SharedExecutionResult<SharedId>.Ok(result, new SharedId(result.Data.OrderId.ToString()));
                                });
        }

        EndpointOptions<GetOrderRequest, IFuturesOrderRestClient> IFuturesOrderRestClient.GetFuturesOrderOptions { get; } = new EndpointOptions<GetOrderRequest, IFuturesOrderRestClient>(_exchangeName, true);
        Task<ExchangeWebResult<SharedFuturesOrder>> IFuturesOrderRestClient.GetFuturesOrderAsync(GetOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, GetOrderRequest, SharedFuturesOrder>(
                this,
                client => client.GetFuturesOrderOptions,
                request,
                async () =>
                {

            if (!long.TryParse(request.OrderId, out var orderId))
                return SharedExecutionResult<SharedFuturesOrder>.Error(ArgumentError.Invalid(nameof(GetOrderRequest.OrderId), "Invalid order id"));

            var order = await Trading.GetOrderAsync(orderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return SharedExecutionResult<SharedFuturesOrder>.Error(order);

            return SharedExecutionResult<SharedFuturesOrder>.Ok(order, new SharedFuturesOrder(
                ExchangeSymbolCache.ParseSymbol(_topicId, order.Data.Order.Symbol),
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
                OrderQuantity = new SharedOrderQuantity(order.Data.Order.Quantity, contractQuantity: order.Data.Order.Quantity),
                QuantityFilled = new SharedOrderQuantity(order.Data.Order.Quantity - order.Data.Order.QuantityRemaining, contractQuantity: order.Data.Order.Quantity - order.Data.Order.QuantityRemaining),
                UpdateTime = order.Data.Timestamp,
                ReduceOnly = order.Data.Order.ReduceOnly,
                TriggerPrice = order.Data.Order.TriggerPrice == 0 ? null : order.Data.Order.TriggerPrice,
                IsTriggerOrder = order.Data.Order.TriggerPrice > 0
            });
                                });
        }

        EndpointOptions<GetOpenOrdersRequest, IFuturesOrderRestClient> IFuturesOrderRestClient.GetOpenFuturesOrdersOptions { get; } = new EndpointOptions<GetOpenOrdersRequest, IFuturesOrderRestClient>(_exchangeName, true)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("dex", typeof(string), "DEX to retrieve open orders for", "xyz")
            }
        };
        Task<ExchangeWebResult<SharedFuturesOrder[]>> IFuturesOrderRestClient.GetOpenFuturesOrdersAsync(GetOpenOrdersRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, GetOpenOrdersRequest, SharedFuturesOrder[]>(
                this,
                client => client.GetOpenFuturesOrdersOptions,
                request,
                async () =>
                {

            var symbol = request.Symbol?.GetSymbol(FormatSymbol);

            // Get dex from parameters or symbol (if symbol is in format DEX:SYMBOL)
            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            if (dex == null && symbol != null)
            {
                var dexSeparatorIndex = symbol.IndexOf(':');
                if (dexSeparatorIndex != -1)
                    dex = symbol.Substring(0, dexSeparatorIndex);
            }

            var orders = await Trading.GetOpenOrdersExtendedAsync(dex: dex, ct: ct).ConfigureAwait(false);
            if (!orders)
                return SharedExecutionResult<SharedFuturesOrder[]>.Error(orders);

            var data = orders.Data.Where(x => x.SymbolType == Enums.SymbolType.Futures);
            if (symbol != null)
                data = data.Where(x => x.Symbol == symbol);

            return SharedExecutionResult<SharedFuturesOrder[]>.Ok(orders, orders.Data.Select(x => new SharedFuturesOrder(
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
                OrderQuantity = new SharedOrderQuantity(x.Quantity, contractQuantity: x.Quantity),
                QuantityFilled = new SharedOrderQuantity(x.Quantity - x.QuantityRemaining, contractQuantity: x.Quantity - x.QuantityRemaining),
                UpdateTime = x.Timestamp,
                ReduceOnly = x.ReduceOnly,
                TriggerPrice = x.TriggerPrice == 0 ? null : x.TriggerPrice,
                IsTriggerOrder = x.TriggerPrice > 0
            }).ToArray());
                                });
        }

        GetFuturesClosedOrdersOptions IFuturesOrderRestClient.GetClosedFuturesOrdersOptions { get; } = new GetFuturesClosedOrdersOptions(_exchangeName, true, true, false, 2000);
        Task<ExchangeWebResult<SharedFuturesOrder[]>> IFuturesOrderRestClient.GetClosedFuturesOrdersAsync(GetClosedOrdersRequest request, PageRequest? pageToken, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, GetClosedOrdersRequest, SharedFuturesOrder[]>(
                this,
                client => client.GetClosedFuturesOrdersOptions,
                request,
                async () =>
                {

            // Get data
            var orders = await Trading.GetOrderHistoryAsync(ct: ct).ConfigureAwait(false);
            if (!orders)
                return SharedExecutionResult<SharedFuturesOrder[]>.Error(orders);


            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var data = orders.Data.Where(x =>
                                        x.Order.SymbolType == Enums.SymbolType.Futures
                                        && x.Order.Symbol == symbol
                                        && x.Status != Enums.OrderStatus.Open);
            if (request.Limit != null)
                data = data.Take(request.Limit.Value);

            return SharedExecutionResult<SharedFuturesOrder[]>.Ok(orders, ExchangeHelpers.ApplyFilter(data, x => x.Timestamp, request.StartTime, request.EndTime, request.Direction ?? DataDirection.Descending)
                .Select(x =>
                    new SharedFuturesOrder(
                        ExchangeSymbolCache.ParseSymbol(_topicId, x.Order.Symbol!), 
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
                        OrderQuantity = new SharedOrderQuantity(x.Order.Quantity, contractQuantity: x.Order.Quantity),
                        QuantityFilled = new SharedOrderQuantity(x.Order.Quantity - x.Order.QuantityRemaining, contractQuantity: x.Order.Quantity - x.Order.QuantityRemaining),
                        UpdateTime = x.Timestamp,
                        ReduceOnly = x.Order.ReduceOnly,
                        TriggerPrice = x.Order.TriggerPrice == 0 ? null : x.Order.TriggerPrice,
                        IsTriggerOrder = x.Order.TriggerPrice > 0
                    }).ToArray());
                                });
        }

        EndpointOptions<GetOrderTradesRequest, IFuturesOrderRestClient> IFuturesOrderRestClient.GetFuturesOrderTradesOptions { get; } = new EndpointOptions<GetOrderTradesRequest, IFuturesOrderRestClient>(_exchangeName, true);
        Task<ExchangeWebResult<SharedUserTrade[]>> IFuturesOrderRestClient.GetFuturesOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, GetOrderTradesRequest, SharedUserTrade[]>(
                this,
                client => client.GetFuturesOrderTradesOptions,
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
                FeeAsset = x.FeeToken,
                Role = x.Crossed ? SharedRole.Taker : SharedRole.Maker
            }).ToArray());
                                });
        }

        GetFuturesUserTradesOptions IFuturesOrderRestClient.GetFuturesUserTradesOptions { get; } = new GetFuturesUserTradesOptions(_exchangeName, true, false, true, 2000);
        Task<ExchangeWebResult<SharedUserTrade[]>> IFuturesOrderRestClient.GetFuturesUserTradesAsync(GetUserTradesRequest request, PageRequest? pageRequest, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, GetUserTradesRequest, SharedUserTrade[]>(
                this,
                client => client.GetFuturesUserTradesOptions,
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
                            FeeAsset = x.FeeToken,
                            Role = x.Crossed ? SharedRole.Taker : SharedRole.Maker
                        })
                    .ToArray(), nextPageRequest);
                                });
        }

        EndpointOptions<CancelOrderRequest, IFuturesOrderRestClient> IFuturesOrderRestClient.CancelFuturesOrderOptions { get; } = new EndpointOptions<CancelOrderRequest, IFuturesOrderRestClient>(_exchangeName, true);
        Task<ExchangeWebResult<SharedId>> IFuturesOrderRestClient.CancelFuturesOrderAsync(CancelOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, CancelOrderRequest, SharedId>(
                this,
                client => client.CancelFuturesOrderOptions,
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

        EndpointOptions<GetPositionsRequest, IFuturesOrderRestClient> IFuturesOrderRestClient.GetPositionsOptions { get; } = new EndpointOptions<GetPositionsRequest, IFuturesOrderRestClient>(_exchangeName, true)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("dex", typeof(string), "DEX to retrieve leverage for", "xyz")
            }
        };
        Task<ExchangeWebResult<SharedPosition[]>> IFuturesOrderRestClient.GetPositionsAsync(GetPositionsRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, GetPositionsRequest, SharedPosition[]>(
                this,
                client => client.GetPositionsOptions,
                request,
                async () =>
                {

            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            var result = await Account.GetAccountInfoAsync(dex: dex, ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedPosition[]>.Error(result);

            IEnumerable<HyperLiquidPosition> data = result.Data.Positions;
            if (request.Symbol != null)
                data = data.Where(x => x.Position.Symbol == request.Symbol.GetSymbol(FormatSymbol));

            var resultTypes = request.Symbol == null && request.TradingMode == null ? SupportedTradingModes : request.Symbol != null ? new[] { request.Symbol.TradingMode } : new[] { request.TradingMode!.Value };
            return SharedExecutionResult<SharedPosition[]>.Ok(result, data.Select(x => new SharedPosition(ExchangeSymbolCache.ParseSymbol(_topicId, x.Position.Symbol), x.Position.Symbol, Math.Abs(x.Position.PositionQuantity ?? 0), null)
            {
                UnrealizedPnl = x.Position.UnrealizedPnl,
                LiquidationPrice = x.Position.LiquidationPrice == 0 ? null : x.Position.LiquidationPrice,
                Leverage = x.Position.Leverage?.Value,
                AverageOpenPrice = x.Position.AverageEntryPrice,
                PositionMode = SharedPositionMode.OneWay,
                PositionSide = x.Position.PositionQuantity >= 0 ? SharedPositionSide.Long : SharedPositionSide.Short
            }).ToArray());
                                });
        }

        EndpointOptions<ClosePositionRequest, IFuturesOrderRestClient> IFuturesOrderRestClient.ClosePositionOptions { get; } = new EndpointOptions<ClosePositionRequest, IFuturesOrderRestClient>(_exchangeName, true)
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(ClosePositionRequest.PositionSide), typeof(SharedPositionSide), "The position side to close", SharedPositionSide.Long),
                new ParameterDescription(nameof(ClosePositionRequest.Quantity), typeof(decimal), "Quantity of the position is required", 0.1m)
            },
            RequiredExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("Price", typeof(decimal), "The current price of the symbol. Required to calculate max slippage.", 21.5m)
            }
        };
        Task<ExchangeWebResult<SharedId>> IFuturesOrderRestClient.ClosePositionAsync(ClosePositionRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderRestClient, ClosePositionRequest, SharedId>(
                this,
                client => client.ClosePositionOptions,
                request,
                async () =>
                {

            var symbol = request.Symbol!.GetSymbol(FormatSymbol);
            var result = await Trading.PlaceOrderAsync(
                request.Symbol.GetSymbol(FormatSymbol),
                request.PositionSide == SharedPositionSide.Long ? Enums.OrderSide.Sell : Enums.OrderSide.Buy,
                Enums.OrderType.Market,
                quantity: request.Quantity!.Value,
                price: ExchangeParameters.GetValue<decimal>(request.ExchangeParameters, ExchangeName, "Price"),
                reduceOnly: true,
                timeInForce: Enums.TimeInForce.ImmediateOrCancel,
                ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<SharedId>.Error(result);

            return SharedExecutionResult<SharedId>.Ok(result, new SharedId(result.Data.OrderId.ToString()));
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
            if (type == Enums.OrderType.Market || type == OrderType.StopMarket || type == OrderType.TakeProfitMarket) return SharedOrderType.Market;
            if (type == Enums.OrderType.Limit || type == OrderType.StopLimit || type == OrderType.TakeProfit) return SharedOrderType.Limit;

            return SharedOrderType.Other;
        }

        #endregion

        #region Futures Client Id Order Client

        EndpointOptions<GetOrderRequest, IFuturesOrderClientIdRestClient> IFuturesOrderClientIdRestClient.GetFuturesOrderByClientOrderIdOptions { get; } = new EndpointOptions<GetOrderRequest, IFuturesOrderClientIdRestClient>(_exchangeName, true);
        Task<ExchangeWebResult<SharedFuturesOrder>> IFuturesOrderClientIdRestClient.GetFuturesOrderByClientOrderIdAsync(GetOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderClientIdRestClient, GetOrderRequest, SharedFuturesOrder>(
                this,
                client => client.GetFuturesOrderByClientOrderIdOptions,
                request,
                async () =>
                {

            var order = await Trading.GetOrderAsync(clientOrderId: request.OrderId, ct: ct).ConfigureAwait(false);
            if (!order)
                return SharedExecutionResult<SharedFuturesOrder>.Error(order);

            return SharedExecutionResult<SharedFuturesOrder>.Ok(order, new SharedFuturesOrder(
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
                OrderQuantity = new SharedOrderQuantity(order.Data.Order.Quantity, contractQuantity: order.Data.Order.Quantity),
                QuantityFilled = new SharedOrderQuantity(order.Data.Order.Quantity - order.Data.Order.QuantityRemaining, contractQuantity: order.Data.Order.Quantity - order.Data.Order.QuantityRemaining),
                UpdateTime = order.Data.Timestamp,
                TriggerPrice = order.Data.Order.TriggerPrice == 0 ? null : order.Data.Order.TriggerPrice,
                IsTriggerOrder = order.Data.Order.TriggerPrice > 0
            });
                                });
        }

        EndpointOptions<CancelOrderRequest, IFuturesOrderClientIdRestClient> IFuturesOrderClientIdRestClient.CancelFuturesOrderByClientOrderIdOptions { get; } = new EndpointOptions<CancelOrderRequest, IFuturesOrderClientIdRestClient>(_exchangeName, true);
        Task<ExchangeWebResult<SharedId>> IFuturesOrderClientIdRestClient.CancelFuturesOrderByClientOrderIdAsync(CancelOrderRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesOrderClientIdRestClient, CancelOrderRequest, SharedId>(
                this,
                client => client.CancelFuturesOrderByClientOrderIdOptions,
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

        #region Tp/SL Client
        EndpointOptions<SetTpSlRequest, IFuturesTpSlRestClient> IFuturesTpSlRestClient.SetFuturesTpSlOptions { get; } = new EndpointOptions<SetTpSlRequest, IFuturesTpSlRestClient>(_exchangeName, true)
        {
            RequestNotes = "API doesn't return an id for trigger orders",
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(SetTpSlRequest.Quantity), typeof(decimal), "The quantity to close", 0.123m)
            }
        };

        Task<ExchangeWebResult<SharedId>> IFuturesTpSlRestClient.SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesTpSlRestClient, SetTpSlRequest, SharedId>(
                this,
                client => client.SetFuturesTpSlOptions,
                request,
                async () =>
                {

            var result = await Trading.PlaceOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                request.PositionSide == SharedPositionSide.Long ? OrderSide.Sell : OrderSide.Buy,
                OrderType.StopMarket,
                request.Quantity!.Value,
                price: request.TriggerPrice,
                triggerPrice: request.TriggerPrice,
                tpSlType: request.TpSlSide == SharedTpSlSide.TakeProfit ? TpSlType.TakeProfit : TpSlType.StopLoss,
                tpSlGrouping: TpSlGrouping.PositionTpSl,
                reduceOnly: true,
                ct: ct).ConfigureAwait(false);

            if (!result)
                return SharedExecutionResult<SharedId>.Error(result);

            // Return
            return SharedExecutionResult<SharedId>.Ok(result, new SharedId(result.Data.OrderId.ToString()));
                                });
        }

        EndpointOptions<CancelTpSlRequest, IFuturesTpSlRestClient> IFuturesTpSlRestClient.CancelFuturesTpSlOptions { get; } = new EndpointOptions<CancelTpSlRequest, IFuturesTpSlRestClient>(_exchangeName, true)
        {
            RequiredOptionalParameters = new List<ParameterDescription>
            {
                new ParameterDescription(nameof(CancelTpSlRequest.OrderId), typeof(string), "Id of the tp/sl order", "123123")
            }
        };

        Task<ExchangeWebResult<bool>> IFuturesTpSlRestClient.CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct)
        {
            return SharedUtils.ExecuteSharedAsync<IFuturesTpSlRestClient, CancelTpSlRequest, bool>(
                this,
                client => client.CancelFuturesTpSlOptions,
                request,
                async () =>
                {

            if (!long.TryParse(request.OrderId, out var orderId) || orderId == 0)
                return SharedExecutionResult<bool>.Error(ArgumentError.Invalid(nameof(CancelTpSlRequest.OrderId), "Invalid order id"));

            var result = await Trading.CancelOrderAsync(
                request.Symbol!.GetSymbol(FormatSymbol),
                orderId,
                ct: ct).ConfigureAwait(false);
            if (!result)
                return SharedExecutionResult<bool>.Error(result);

            // Return
            return SharedExecutionResult<bool>.Ok(result, true);
                                });
        }

        #endregion
    }
}
