using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using HyperLiquid.Net.Utils;
using HyperLiquid.Net.Enums;
using System.Linq;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using CryptoExchange.Net;

namespace HyperLiquid.Net.Clients.BaseApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientTrading : IHyperLiquidRestClientTrading
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly HyperLiquidRestClientApi _baseClient;
        private readonly ILogger _logger;

        internal HyperLiquidRestClientTrading(ILogger logger, HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
            _logger = logger;
        }

        #region Get Open Orders

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOpenOrder[]>> GetOpenOrdersAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "openOrders" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey  }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidOpenOrder[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.ExchangeSymbol).ConfigureAwait(false);
                    if (symbolName == null)
                        continue;

                    order.Symbol = symbolName.Data;
                    order.SymbolType = SymbolType.Spot;
                }
                else
                {
                    order.Symbol = order.ExchangeSymbol;
                    order.SymbolType = SymbolType.Futures;
                }
            }

            return result;
        }

        #endregion

        #region Get Open Orders Extended

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOrder[]>> GetOpenOrdersExtendedAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "frontendOpenOrders" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrder[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.ExchangeSymbol).ConfigureAwait(false);
                    if (symbolName == null)
                        continue;

                    order.Symbol = symbolName.Data;
                    order.SymbolType = SymbolType.Spot;
                }
                else
                {
                    order.Symbol = order.ExchangeSymbol;
                    order.SymbolType = SymbolType.Futures;
                }
            }

            return result;
        }

        #endregion

        #region Get User Trades

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidUserTrade[]>> GetUserTradesAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userFills" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidUserTrade[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.ExchangeSymbol).ConfigureAwait(false);
                    if (symbolName == null)
                        continue;

                    order.Symbol = symbolName.Data;
                    order.SymbolType = SymbolType.Spot;
                }
                else
                {
                    order.Symbol = order.ExchangeSymbol;
                    order.SymbolType = SymbolType.Futures;
                }
            }

            return result;
        }

        #endregion

        #region Get User Trades By Time

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidUserTrade[]>> GetUserTradesByTimeAsync(
            DateTime startTime,
            DateTime? endTime = null,
            bool? aggregateByTime = null,
            string? address = null,
            CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "userFillsByTime" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            parameters.AddOptional("aggregateByTime", aggregateByTime);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidUserTrade[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.ExchangeSymbol).ConfigureAwait(false);
                    if (symbolName == null)
                        continue;

                    order.Symbol = symbolName.Data;
                    order.SymbolType = SymbolType.Spot;
                }
                else
                {
                    order.Symbol = order.ExchangeSymbol;
                    order.SymbolType = SymbolType.Futures;
                }
            }

            return result;
        }

        #endregion

        #region Get Order

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOrderStatus>> GetOrderAsync(long? orderId = null, string? clientOrderId = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "orderStatus" },
                { "user", address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };

            parameters.AddOptional("oid", orderId);
            parameters.AddOptional("oid", clientOrderId);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrderStatusResult>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result.As<HyperLiquidOrderStatus>(default);

            if (result.Data.Status != "order")
                return result.AsError<HyperLiquidOrderStatus>(new ServerError(result.Data.Status));

            if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(result.Data.Order!.Order.ExchangeSymbol))
            {
                var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, result.Data.Order!.Order.ExchangeSymbol).ConfigureAwait(false);
                if (symbolName != null)
                {
                    result.Data.Order!.Order.Symbol = symbolName.Data;
                    result.Data.Order!.Order.SymbolType = SymbolType.Spot;
                }
            }
            else
            {
                result.Data.Order!.Order.Symbol = result.Data.Order!.Order.ExchangeSymbol;
                result.Data.Order!.Order.SymbolType = SymbolType.Futures;
            }

            return result.As(result.Data.Order);
        }

        #endregion

        #region Get Order History

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOrderStatus[]>> GetOrderHistoryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            var parameters = new ParameterCollection()
            {
                { "type", "historicalOrders" },
                { "user",  address ?? _baseClient.AuthenticationProvider!.ApiKey }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrderStatus[]>(request, parameters, ct).ConfigureAwait(false);

            if (!result)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.Order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.Order.ExchangeSymbol).ConfigureAwait(false);
                    if (symbolName == null)
                        continue;

                    order.Order.Symbol = symbolName.Data;
                    order.Order.SymbolType = SymbolType.Spot;
                }
                else
                {
                    order.Order.Symbol = order.Order.ExchangeSymbol;
                    order.Order.SymbolType = SymbolType.Futures;
                }
            }

            return result;
        }

        #endregion

        #region Place Order

        public async Task<WebCallResult<HyperLiquidOrderResult>> PlaceOrderAsync(
            string symbol,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal price,
            TimeInForce? timeInForce = null,
            bool? reduceOnly = null,
            string? clientOrderId = null,
            decimal? triggerPrice = null,
            TpSlType? tpSlType = null,
            TpSlGrouping? tpSlGrouping = null,
            CancellationToken ct = default)
        {
            var result = await PlaceMultipleOrdersAsync([
                new HyperLiquidOrderRequest(symbol, side, orderType, quantity, price, timeInForce, reduceOnly, triggerPrice: triggerPrice, tpSlType: tpSlType, clientOrderId: clientOrderId)
                ], tpSlGrouping, ct).ConfigureAwait(false);

            if (!result)
                return result.As<HyperLiquidOrderResult>(default);

            var orderResult = result.Data.Single();
            if (!orderResult)
                return result.AsError<HyperLiquidOrderResult>(orderResult.Error!);

            return result.As(result.Data.Single().Data);
        }

        #endregion

        #region Place Multiple Orders

        /// <inheritdoc />
        public async Task<WebCallResult<CallResult<HyperLiquidOrderResult>[]>> PlaceMultipleOrdersAsync(
            IEnumerable<HyperLiquidOrderRequest> orders,
            TpSlGrouping? tpSlGrouping = null,
            CancellationToken ct = default)
        {
            var orderRequests = new List<ParameterCollection>();
            foreach (var order in orders)
            {
                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId)
                    return new WebCallResult<CallResult<HyperLiquidOrderResult>[]>(symbolId.Error);

                var orderParameters = new ParameterCollection();
                orderParameters.Add("a", symbolId.Data);
                orderParameters.Add("b", order.Side == OrderSide.Buy);

                var orderTypeParameters = new ParameterCollection();                
                if (order.OrderType == OrderType.Limit)
                {
                    orderParameters.AddString("p", order.Price?.Normalize() ?? 0);
                    orderParameters.AddString("s", order.Quantity);
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var limitParameters = new ParameterCollection();
                    limitParameters.AddEnum("tif", order.OrderType == OrderType.Market ? TimeInForce.ImmediateOrCancel : order.TimeInForce ?? TimeInForce.GoodTillCanceled);
                    orderTypeParameters.Add("limit", limitParameters);
                }
                else if (order.OrderType == OrderType.Market)
                {
                    var maxSlippage = order.MaxSlippage ?? 5;
                    var price = order.Side == OrderSide.Buy ? order.Price * (1 + maxSlippage / 100m) : order.Price * (1 - maxSlippage / 100m);
                    var decimalPlaces = CountDecimalDigits(order.Price ?? 0);
                    orderParameters.AddString("p", Math.Round(price ?? 0, decimalPlaces).Normalize());
                    orderParameters.AddString("s", order.Quantity);
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var limitParameters = new ParameterCollection();
                    limitParameters.AddEnum("tif", order.OrderType == OrderType.Market ? TimeInForce.ImmediateOrCancel : order.TimeInForce ?? TimeInForce.GoodTillCanceled);
                    orderTypeParameters.Add("limit", limitParameters);
                }
                else
                {
                    if (order.TriggerPrice == null)
                       throw new ArgumentNullException(nameof(order.TriggerPrice), "Stop order should have a trigger price");

                    if (order.TpSlType == null)
                        throw new ArgumentNullException(nameof(order.TpSlType), "Stop order should have a TpSlType");

                    orderParameters.AddString("p", order.Price?.Normalize() ?? 0);
                    orderParameters.AddString("s", order.Quantity);
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var triggerParameters = new ParameterCollection();
                    triggerParameters.Add("isMarket", order.OrderType == OrderType.StopMarket);
                    triggerParameters.AddString("triggerPx", order.TriggerPrice.Value);
                    triggerParameters.AddEnum("tpsl", order.TpSlType.Value);
                    orderTypeParameters.Add("trigger", triggerParameters);
                }

                orderParameters.Add("t", orderTypeParameters);
                orderParameters.AddOptional("c", order.ClientOrderId);

                orderRequests.Add(orderParameters);
            }

            var actionParameters = new ParameterCollection
            {
                { "type", "order" },
                { "orders", orderRequests },
            };

            if (tpSlGrouping != null)
                actionParameters.AddEnum("grouping", tpSlGrouping.Value);
            else
                actionParameters.Add("grouping", "na");

            if (_baseClient.ClientOptions.BuilderFeePercentage > 0)
            {
                // Convert from percentage to 1/10 basis point
                var tenthPoints = (int)(_baseClient.ClientOptions.BuilderFeePercentage * 1000);
                actionParameters.Add("builder",
                    new ParameterCollection
                    {
                        { "b", "0x64134a9577A857BcC5dAfa42E1647E1439e5F8E7".ToLower() },
                        { "f", tenthPoints }
                    }
                );
            }

            var parameters = new ParameterCollection()
            {
                { "action", actionParameters }
            };

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var intResult = await _baseClient.SendAuthAsync<HyperLiquidOrderResultIntWrapper>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!intResult)
                return intResult.As<CallResult<HyperLiquidOrderResult>[]>(default);

            var result = new List<CallResult<HyperLiquidOrderResult>>();
            foreach (var order in intResult.Data.Statuses)
            {
                if (order.Error != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(new ServerError(order.Error)));
                else if (order.ResultResting != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.ResultResting with { Status = OrderStatus.Open }));
                else if (order.ResultFilled != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.ResultFilled! with { Status = OrderStatus.Filled }));
                else if (order.WaitingForFill != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.WaitingForFill! with { Status = OrderStatus.WaitingTrigger }));
                else
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.WaitingForTrigger! with { Status = OrderStatus.WaitingTrigger }));
            }

            if (result.All(x => !x.Success))
                return intResult.AsErrorWithData<CallResult<HyperLiquidOrderResult>[]>(result.First().Error!, result.ToArray());

            return intResult.As<CallResult<HyperLiquidOrderResult>[]>(result.ToArray());
        }

        #endregion

        #region Cancel Order

        public async Task<WebCallResult> CancelOrderAsync(string symbol, long orderId, CancellationToken ct = default)
        {
            var result = await CancelOrdersAsync([new HyperLiquidCancelRequest(symbol, orderId)], ct).ConfigureAwait(false);
            if (!result)
                return result.AsDataless();

            var cancelResult = result.Data.Single();
            if (!cancelResult)
                return result.AsDatalessError(cancelResult.Error!);

            return result.AsDataless();
        }

        #endregion

        #region Cancel Orders

        /// <inheritdoc />
        public async Task<WebCallResult<CallResult[]>> CancelOrdersAsync(IEnumerable<HyperLiquidCancelRequest> requests, CancellationToken ct = default)
        {
            var orderRequests = new List<ParameterCollection>();
            foreach (var order in requests)
            {
                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId)
                    return new WebCallResult<CallResult[]>(symbolId.Error);

                orderRequests.Add(new ParameterCollection
                    {
                        { "a", symbolId.Data },
                        { "o", order.OrderId }
                    }
                );
            }

            var parameters = new ParameterCollection()
            {
                {
                    "action", new ParameterCollection
                    {
                        { "type", "cancel" },
                        { "cancels", orderRequests
                        }
                    }
                }
            };

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var resultInt = await _baseClient.SendAuthAsync<HyperLiquidCancelResult>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!resultInt)
                return resultInt.AsError<CallResult[]>(resultInt.Error!);

            var result = new List<CallResult>();
            foreach (var order in resultInt.Data.Statuses)
            {
                if (order.Equals("success"))
                    result.Add(CallResult.SuccessResult);
                else
                    result.Add(new CallResult(new ServerError(order)));
            }

            return resultInt.As<CallResult[]>(result.ToArray());
        }

        #endregion

        #region Cancel Order By Client Order Id

        public async Task<WebCallResult> CancelOrderByClientOrderIdAsync(string symbol, string clientOrderId, CancellationToken ct = default)
        {
            var result = await CancelOrdersByClientOrderIdAsync([new HyperLiquidCancelByClientOrderIdRequest(symbol, clientOrderId)], ct).ConfigureAwait(false);
            if (!result)
                return result.AsDataless();

            var cancelResult = result.Data.Single();
            if (!cancelResult)
                return result.AsDatalessError(cancelResult.Error!);

            return result.AsDataless();
        }

        #endregion

        #region Cancel Orders By Client Order Id

        /// <inheritdoc />
        public async Task<WebCallResult<CallResult[]>> CancelOrdersByClientOrderIdAsync(IEnumerable<HyperLiquidCancelByClientOrderIdRequest> requests, CancellationToken ct = default)
        {
            var orderRequests = new List<ParameterCollection>();
            foreach (var order in requests)
            {
                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId)
                    return new WebCallResult<CallResult[]>(symbolId.Error);

                orderRequests.Add(new ParameterCollection
                    {
                        { "asset", symbolId.Data },
                        { "cloid", order.OrderId }
                    }
                );
            }

            var parameters = new ParameterCollection()
            {
                {
                    "action", new ParameterCollection
                    {
                        { "type", "cancelByCloid" },
                        { "cancels", orderRequests
                        }
                    }
                }
            };

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var resultInt = await _baseClient.SendAuthAsync<HyperLiquidCancelResult>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!resultInt)
                return resultInt.AsError<CallResult[]>(resultInt.Error!);

            var result = new List<CallResult>();
            foreach (var order in resultInt.Data.Statuses)
            {
                if (order.Equals("success"))
                    result.Add(CallResult.SuccessResult);
                else
                    result.Add(new CallResult(new ServerError(order)));
            }

            return resultInt.As<CallResult[]>(result.ToArray());
        }

        #endregion

        #region Cancel after

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidOrderStatus[]>> CancelAfterAsync(TimeSpan? timeout, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "scheduleCancel" }
            };
            actionParameters.AddOptionalMilliseconds("time", timeout == null ? null : DateTime.UtcNow + timeout);
            parameters.Add("action", actionParameters);

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAsync<HyperLiquidOrderStatus[]>(request, parameters, ct).ConfigureAwait(false);


            return result;
        }

        #endregion

        #region Edit Order

        /// <inheritdoc />
        public async Task<WebCallResult> EditOrderAsync(
            string symbol,
            long? orderId,
            string? clientOrderId,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal price,
            TimeInForce? timeInForce = null,
            bool? reduceOnly = null,
            string? newClientOrderId = null,
            CancellationToken ct = default)
        {
            if (orderId == null == (clientOrderId == null))
                throw new ArgumentException("Either orderId or clientOrderId should be provided");

            if (orderType == OrderType.Market)
                throw new ArgumentException("Order type can't be market");

            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            var orderParameters = new ParameterCollection();
            orderParameters.Add("a", symbolId.Data);
            orderParameters.Add("b", side == OrderSide.Buy);

            var orderTypeParameters = new ParameterCollection();
            orderParameters.AddString("p", price.Normalize());
            
            orderParameters.AddString("s", quantity);
            orderParameters.Add("r", reduceOnly ?? false);
            var limitParameters = new ParameterCollection();
            limitParameters.AddEnum("tif", timeInForce ?? TimeInForce.GoodTillCanceled);
            orderTypeParameters.Add("limit", limitParameters);
            orderParameters.Add("t", orderTypeParameters);
            orderParameters.AddOptional("c", newClientOrderId);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "modify" }
            };
            actionParameters.AddOptional("oid", orderId);
            actionParameters.AddOptional("oid", clientOrderId);
            actionParameters.Add("order", orderParameters);
            parameters.Add("action", actionParameters);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAsync<HyperLiquidResponse>(request, parameters, ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Edit Orders

        /// <inheritdoc />
        public async Task<WebCallResult<CallResult<HyperLiquidOrderResult>[]>> EditOrdersAsync(
            IEnumerable<HyperLiquidEditOrderRequest> requests,
            CancellationToken ct = default)
        {
            var orderRequests = new List<ParameterCollection>();
            foreach (var order in requests)
            {
                if (order.OrderId == null == (order.ClientOrderId == null))
                    throw new ArgumentException("Either OrderId or ClientOrderId should be provided per order");

                if (order.OrderType == OrderType.Market)
                    throw new ArgumentException("Order type can't be market");

                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId)
                    return new WebCallResult<CallResult<HyperLiquidOrderResult>[]>(symbolId.Error!);

                var modifyParameters = new ParameterCollection();
                modifyParameters.AddOptional("oid", order.OrderId);
                modifyParameters.AddOptional("oid", order.ClientOrderId);
                var orderParameters = new ParameterCollection();
                orderParameters.Add("a", symbolId.Data);
                orderParameters.Add("b", order.Side == OrderSide.Buy);
                orderParameters.AddString("p", order.Price);
                orderParameters.AddString("s", order.Quantity);
                orderParameters.Add("r", order.ReduceOnly ?? false);

                var orderTypeParameters = new ParameterCollection();
                var limitParameters = new ParameterCollection();
                limitParameters.AddEnum("tif", order.OrderType == OrderType.Market ? TimeInForce.ImmediateOrCancel : order.TimeInForce ?? TimeInForce.GoodTillCanceled);
                orderTypeParameters.Add("limit", limitParameters);
                orderParameters.Add("t", orderTypeParameters);

                orderParameters.AddOptional("c", order.ClientOrderId);

                modifyParameters.Add("order", orderParameters);
                orderRequests.Add(modifyParameters);
            }

            var parameters = new ParameterCollection()
            {
                {
                    "action", new ParameterCollection
                    {
                        { "type", "batchModify" },
                        { "modifies", orderRequests }
                    }
                }
            };

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var intResult = await _baseClient.SendAuthAsync<HyperLiquidOrderResultIntWrapper>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!intResult)
                return intResult.As<CallResult<HyperLiquidOrderResult>[]>(default);

            var result = new List<CallResult<HyperLiquidOrderResult>>();
            foreach (var order in intResult.Data.Statuses)
            {
                if (order.Error != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(new ServerError(order.Error)));
                else if (order.ResultResting != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.ResultResting with { Status = OrderStatus.Open }));
                else
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.ResultFilled! with { Status = OrderStatus.Filled }));
            }

            return intResult.As<CallResult<HyperLiquidOrderResult>[]>(result.ToArray());
        }

        #endregion

        #region Place TWAP order

        /// <inheritdoc />
        public async Task<WebCallResult<HyperLiquidTwapOrderResult>> PlaceTwapOrderAsync(string symbol, OrderSide orderSide, decimal quantity, bool reduceOnly, int minutes, bool randomize, CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult<HyperLiquidTwapOrderResult>(symbolId.Error);

            var orderParameters = new ParameterCollection();
            orderParameters.Add("a", symbolId.Data);
            orderParameters.Add("b", orderSide == OrderSide.Buy);
            orderParameters.AddString("s", quantity);
            orderParameters.Add("r", reduceOnly);
            orderParameters.Add("m", minutes);
            orderParameters.Add("t", randomize);

            var actionParameters = new ParameterCollection()
            {
                { "type", "twapOrder" },
                { "twap", orderParameters }
            };
            var parameters = new ParameterCollection
            {
                { "action", actionParameters }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidTwapOrderResultIntWrapper>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result.As<HyperLiquidTwapOrderResult>(default);

            if (result.Data.Status.Error != null)
                return result.AsError<HyperLiquidTwapOrderResult>(new ServerError(result.Data.Status.Error));

            return result.As(result.Data.Status.ResultRunning!);
        }

        #endregion

        #region Cancel Twap Order

        /// <inheritdoc />
        public async Task<WebCallResult> CancelTwapOrderAsync(string symbol, long twapId, CancellationToken ct = default)
        {
            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            var actionParameters = new ParameterCollection()
            {
                { "type", "twapCancel" },
                { "a", symbolId.Data },
                { "t", twapId }
            };

            var parameters = new ParameterCollection
            {
                { "action", actionParameters }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidTwapCancelResult>(request, parameters, ct).ConfigureAwait(false);
            if (!result)
                return result.AsDataless();

            if (result.Data.Status != "success")
                return result.AsDatalessError(new ServerError(result.Data.Status));

            return result.AsDataless();
        }

        #endregion

        private int CountDecimalDigits(decimal n)
        {
            return n.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    .SkipWhile(c => c != '.')
                    .Skip(1)
                    .Count();
        }
    }
}
