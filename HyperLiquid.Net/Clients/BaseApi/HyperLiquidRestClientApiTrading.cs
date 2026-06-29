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
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Interfaces;

namespace HyperLiquid.Net.Clients.BaseApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientApiTrading : IHyperLiquidRestClientTrading
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly HyperLiquidRestClientApi _baseClient;
        private readonly ILogger _logger;

        internal HyperLiquidRestClientApiTrading(ILogger logger, HyperLiquidRestClientApi baseClient)
        {
            _baseClient = baseClient;
            _logger = logger;
        }

        #region Get Open Orders

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidOpenOrder[]>> GetOpenOrdersAsync(string? address = null, string? dex = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "openOrders" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key  }
            };
            parameters.Add("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidOpenOrder[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
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
        public async Task<HttpResult<HyperLiquidOrder[]>> GetOpenOrdersExtendedAsync(string? address = null, string? dex = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "frontendOpenOrders" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.Add("dex", dex);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrder[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
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
        public async Task<HttpResult<HyperLiquidUserTrade[]>> GetUserTradesAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userFills" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidUserTrade[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.ExchangeSymbol).ConfigureAwait(false);
                    if (!symbolName.Success)
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
        public async Task<HttpResult<HyperLiquidUserTrade[]>> GetUserTradesByTimeAsync(
            DateTime startTime,
            DateTime? endTime = null,
            bool? aggregateByTime = null,
            string? address = null,
            CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "userFillsByTime" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.Add("startTime", startTime);
            parameters.Add("endTime", endTime);
            parameters.Add("aggregateByTime", aggregateByTime);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidUserTrade[]>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            foreach (var order in result.Data)
            {
                if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                {
                    var symbolName = await HyperLiquidUtils.GetSymbolNameFromExchangeNameAsync(_baseClient.BaseClient, order.ExchangeSymbol).ConfigureAwait(false);
                    if (!symbolName.Success)
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
        public async Task<HttpResult<HyperLiquidOrderStatus>> GetOrderAsync(long? orderId = null, string? clientOrderId = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "orderStatus" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            parameters.Add("oid", orderId);
            parameters.Add("oid", clientOrderId);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 2, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrderStatusResult>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<HyperLiquidOrderStatus>(result);

            if (result.Data.Status != "order")
                return HttpResult.Fail<HyperLiquidOrderStatus>(result, new ServerError(new ErrorInfo(ErrorType.Unknown, result.Data.Status)));

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

            return HttpResult.Ok(result, result.Data.Order);
        }

        #endregion

        #region Get Order History

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidOrderStatus[]>> GetOrderHistoryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "historicalOrders" },
                { "user",  address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "info", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 20, false);
            var result = await _baseClient.SendAsync<HyperLiquidOrderStatus[]>(request, parameters, ct).ConfigureAwait(false);

            if (!result.Success)
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

        public async Task<HttpResult<HyperLiquidOrderResult>> PlaceOrderAsync(
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
			string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var result = await PlaceMultipleOrdersAsync([
                new HyperLiquidOrderRequest(symbol, side, orderType, quantity, price, timeInForce, reduceOnly, triggerPrice: triggerPrice, tpSlType: tpSlType, clientOrderId: clientOrderId)
                ], tpSlGrouping, vaultAddress, expiresAfter, ct).ConfigureAwait(false);

            if (!result.Success)
                return HttpResult.Fail<HyperLiquidOrderResult>(result);

            var orderResult = result.Data.Single();
            if (!orderResult.Success)
                return HttpResult.Fail<HyperLiquidOrderResult>(result, orderResult.Error);

            return HttpResult.Ok(result, result.Data.Single().Data!);
        }

        #endregion

        #region Place Multiple Orders

        /// <inheritdoc />
        public async Task<HttpResult<CallResult<HyperLiquidOrderResult>[]>> PlaceMultipleOrdersAsync(
            IEnumerable<HyperLiquidOrderRequest> orders,
            TpSlGrouping? tpSlGrouping = null,
			string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var orderRequests = new List<Parameters>();
            foreach (var order in orders)
            {
                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId.Success)
                    return HttpResult.Fail<CallResult<HyperLiquidOrderResult>[]>(_baseClient.Exchange, symbolId.Error);

                var orderParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                orderParameters.Add("a", symbolId.Data);
                orderParameters.Add("b", order.Side == OrderSide.Buy);

                var orderTypeParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);                
                if (order.OrderType == OrderType.Limit)
                {
                    orderParameters.Add("p", order.Price?.Normalize() ?? 0);
                    orderParameters.Add("s", order.Quantity.Normalize());
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var limitParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                    limitParameters.Add("tif", order.TimeInForce ?? TimeInForce.GoodTillCanceled);
                    orderTypeParameters.AddRaw("limit", limitParameters);
                }
                else if (order.OrderType == OrderType.Market)
                {
                    var maxSlippage = order.MaxSlippage ?? 5;
                    var price = (order.Side == OrderSide.Buy ? order.Price * (1 + maxSlippage / 100m) : order.Price * (1 - maxSlippage / 100m)) ?? 0;
                    var quantityDecimals = await HyperLiquidUtils.GetQuantityDecimalPlacesForSymbolAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                    if (!quantityDecimals.Success)
                        return HttpResult.Fail<CallResult<HyperLiquidOrderResult>[]>(_baseClient.Exchange, quantityDecimals.Error);

                    // Price can be a max of 5 significant figures
                    // but no more than (Spot:8, Futures:6) - quantityDecimals decimal places for the symbol
                    var decimalMax = symbolId.Data < 10000 ? 6 : 8; // Spot symbols have id >= 10000
                    price = ExchangeHelpers.RoundToSignificantDigits(price, 5, RoundingType.Closest);
                    price = ExchangeHelpers.RoundDown(price, decimalMax - quantityDecimals.Data);

                    orderParameters.Add("p", price.Normalize());
                    orderParameters.Add("s", order.Quantity.Normalize());
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var limitParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                    limitParameters.Add("tif", order.TimeInForce ?? TimeInForce.ImmediateOrCancel);
                    orderTypeParameters.AddRaw("limit", limitParameters);
                }
                else
                {
                    if (order.TriggerPrice == null)
                       throw new ArgumentNullException(nameof(order.TriggerPrice), "Stop order should have a trigger price");

                    if (order.TpSlType == null)
                        throw new ArgumentNullException(nameof(order.TpSlType), "Stop order should have a TpSlType");

                    orderParameters.Add("p", order.Price?.Normalize() ?? 0);
                    orderParameters.Add("s", order.Quantity.Normalize());
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var triggerParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                    triggerParameters.Add("isMarket", order.OrderType == OrderType.StopMarket);
                    triggerParameters.Add("triggerPx", order.TriggerPrice.Value.Normalize());
                    triggerParameters.Add("tpsl", order.TpSlType.Value);
                    orderTypeParameters.AddRaw("trigger", triggerParameters);
                }

                orderParameters.AddRaw("t", orderTypeParameters);
                if (order.ClientOrderId != null)
                {
                    if (!order.ClientOrderId.IsValidClientOrderId())
                        throw new ArgumentException(nameof(order.ClientOrderId), "Client order id should be a valid 128 bit hex string, for example `0x1234567890abcdef1234567890abcdef`");

                    if (!order.ClientOrderId.StartsWith("0x"))
                        order.ClientOrderId = "0x" + order.ClientOrderId;

                    orderParameters.Add("c", order.ClientOrderId);
                }

                orderRequests.Add(orderParameters);
            }

            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            actionParameters.Add("type", "order");
            actionParameters.AddRaw("orders", orderRequests);

            if (tpSlGrouping != null)
                actionParameters.Add("grouping", tpSlGrouping.Value);
            else
                actionParameters.Add("grouping", "na");

            if (_baseClient.ClientOptions.BuilderFeePercentage > 0
                && _baseClient.ClientOptions.BuilderAddress != null
                && HyperLiquidUtils._builderFeeSuccess)
            {
                // Convert from percentage to 1/10 basis point
                var tenthPoints = (int)(_baseClient.ClientOptions.BuilderFeePercentage * 1000);
                actionParameters.AddRaw("builder",
                    new Parameters(HyperLiquidExchange._parameterSerializationSettings)
                    {
                        { "b", _baseClient.ClientOptions.BuilderAddress.ToLower() },
                        { "f", tenthPoints }
                    }
                );
            }

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            parameters.AddRaw("action", actionParameters);

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expireAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var intResult = await _baseClient.SendAuthAsync<HyperLiquidOrderResultIntWrapper>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!intResult.Success)
                return HttpResult.Fail<CallResult<HyperLiquidOrderResult>[]>(intResult);

            var result = new List<CallResult<HyperLiquidOrderResult>>();
            foreach (var order in intResult.Data.Statuses)
            {
                if (order.Error != null)
                    result.Add(CallResult<HyperLiquidOrderResult>.Fail(new ServerError(_baseClient.GetErrorInfo("Order", order.Error))));
                else if (order.ResultResting != null)
                    result.Add(CallResult<HyperLiquidOrderResult>.Ok(order.ResultResting with { Status = OrderStatus.Open }));
                else if (order.ResultFilled != null)
                    result.Add(CallResult<HyperLiquidOrderResult>.Ok(order.ResultFilled! with { Status = OrderStatus.Filled }));
                else if (order.WaitingForFill != null)
                    result.Add(CallResult<HyperLiquidOrderResult>.Ok(order.WaitingForFill! with { Status = OrderStatus.WaitingTrigger }));
                else
                    result.Add(CallResult<HyperLiquidOrderResult>.Ok(order.WaitingForTrigger! with { Status = OrderStatus.WaitingTrigger }));
            }

            if (result.Count > 1 && result.All(x => !x.Success))
                return HttpResult.Fail<CallResult<HyperLiquidOrderResult>[]>(intResult, new ServerError(new ErrorInfo(ErrorType.AllOrdersFailed, "All orders failed")), result.ToArray());

            return HttpResult.Ok(intResult, result.ToArray());
        }

        #endregion

        #region Cancel Order

        public async Task<HttpResult> CancelOrderAsync(
            string symbol,
            long orderId,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var result = await CancelOrdersAsync([new HyperLiquidCancelRequest(symbol, orderId)], vaultAddress, expiresAfter, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            var cancelResult = result.Data.Single();
            if (!cancelResult.Success)
                return HttpResult.Fail(_baseClient.Exchange, cancelResult.Error!);

            return result;
        }

        #endregion

        #region Cancel Orders

        /// <inheritdoc />
        public async Task<HttpResult<CallResult[]>> CancelOrdersAsync(
            IEnumerable<HyperLiquidCancelRequest> requests, 
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var orderRequests = new List<Parameters>();
            foreach (var order in requests)
            {
                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId.Success)
                    return HttpResult.Fail<CallResult[]>(_baseClient.Exchange, symbolId.Error);

                orderRequests.Add(new Parameters(HyperLiquidExchange._parameterSerializationSettings)
                    {
                        { "a", symbolId.Data },
                        { "o", order.OrderId }
                    }
                );
            }

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                {
                    "action", new Parameters(HyperLiquidExchange._parameterSerializationSettings)
                    {
                        { "type", "cancel" },
                        { "cancels", orderRequests
                        }
                    }
                }
            };
            
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var resultInt = await _baseClient.SendAuthAsync<HyperLiquidCancelResult>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!resultInt.Success)
                return HttpResult.Fail<CallResult[]>(resultInt);

            var result = new List<CallResult>();
            foreach (var order in resultInt.Data.Statuses)
            {
                if (order.Equals("success"))
                    result.Add(CallResult.Ok());
                else
                    result.Add(CallResult.Fail(new ServerError(_baseClient.GetErrorInfo("Order", order))));
            }

            return HttpResult.Ok(resultInt, result.ToArray());
        }

        #endregion

        #region Cancel Order By Client Order Id

        public async Task<HttpResult> CancelOrderByClientOrderIdAsync(
            string symbol,
            string clientOrderId,
            string? vaultAddress,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var result = await CancelOrdersByClientOrderIdAsync([new HyperLiquidCancelByClientOrderIdRequest(symbol, clientOrderId)], vaultAddress, expiresAfter, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            var cancelResult = result.Data.Single();
            if (!cancelResult.Success)
                return HttpResult.Fail(_baseClient.Exchange, cancelResult.Error);

            return result;
        }

        #endregion

        #region Cancel Orders By Client Order Id

        /// <inheritdoc />
        public async Task<HttpResult<CallResult[]>> CancelOrdersByClientOrderIdAsync(
            IEnumerable<HyperLiquidCancelByClientOrderIdRequest> requests, 
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var orderRequests = new List<Parameters>();
            foreach (var order in requests)
            {
                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId.Success)
                    return HttpResult.Fail<CallResult[]>(_baseClient.Exchange, symbolId.Error);

                orderRequests.Add(new Parameters(HyperLiquidExchange._parameterSerializationSettings)
                    {
                        { "asset", symbolId.Data },
                        { "cloid", order.OrderId }
                    }
                );
            }

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                {
                    "action", new Parameters(HyperLiquidExchange._parameterSerializationSettings)
                    {
                        { "type", "cancelByCloid" },
                        { "cancels", orderRequests
                        }
                    }
                }
            };
            
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var resultInt = await _baseClient.SendAuthAsync<HyperLiquidCancelResult>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!resultInt.Success)
                return HttpResult.Fail<CallResult[]>(resultInt);

            var result = new List<CallResult>();
            foreach (var order in resultInt.Data.Statuses)
            {
                if (order.Equals("success"))
                    result.Add(CallResult.Ok());
                else
                    result.Add(CallResult.Fail(new ServerError(_baseClient.GetErrorInfo("Order", order))));
            }

            return HttpResult.Ok<CallResult[]>(resultInt, result.ToArray());
        }

        #endregion

        #region Cancel after

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidOrderStatus[]>> CancelAfterAsync(
            TimeSpan? timeout,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "scheduleCancel" }
            };
            actionParameters.Add("time", timeout == null ? null : DateTime.UtcNow + timeout);
            parameters.Add("action", actionParameters);
            
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAsync<HyperLiquidOrderStatus[]>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Edit Order

        /// <inheritdoc />
        public async Task<HttpResult> EditOrderAsync(
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
            decimal? triggerPrice = null,
            TpSlType? tpSlType = null,
            TpSlGrouping? tpSlGrouping = null,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            bool? alwaysPlace = null,
            CancellationToken ct = default)
        {
            if ((orderId == null) == (clientOrderId == null))
                throw new ArgumentException("Either orderId or clientOrderId should be provided");

            if (orderType == OrderType.Market)
                throw new ArgumentException("Order type can't be market");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId.Success)
                return HttpResult.Fail(_baseClient.Exchange, symbolId.Error!);

            var orderParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            orderParameters.Add("a", symbolId.Data);
            orderParameters.Add("b", side == OrderSide.Buy);
            orderParameters.Add("p", price.Normalize());
            orderParameters.Add("s", quantity.Normalize());
            orderParameters.Add("r", reduceOnly ?? false);

            var orderTypeParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);            
            if (orderType == OrderType.Limit)
            {
                var limitParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                limitParameters.Add("tif", timeInForce ?? TimeInForce.GoodTillCanceled);
                orderTypeParameters.Add("limit", limitParameters);
            }
            else
            {
                if (triggerPrice == null)
                    throw new ArgumentNullException(nameof(triggerPrice), "Stop order should have a trigger price");

                if (tpSlType == null)
                    throw new ArgumentNullException(nameof(tpSlType), "Stop order should have a TpSlType");

                var triggerParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                triggerParameters.Add("isMarket", orderType == OrderType.StopMarket);
                triggerParameters.Add("triggerPx", triggerPrice.Value.Normalize());
                triggerParameters.Add("tpsl", tpSlType.Value);
                orderTypeParameters.Add("trigger", triggerParameters);
            }

            orderParameters.Add("t", orderTypeParameters);
            orderParameters.Add("c", newClientOrderId);

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "modify" }
            };
            actionParameters.Add("oid", orderId);
            actionParameters.Add("oid", clientOrderId);
            actionParameters.Add("order", orderParameters);
            if (alwaysPlace == true)
                actionParameters.Add("a", alwaysPlace.Value);
            parameters.Add("action", actionParameters);
            
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            return await _baseClient.SendAsync<HyperLiquidResponse>(request, parameters, ct).ConfigureAwait(false);
        }

        #endregion

        #region Edit Orders

        /// <inheritdoc />
        public async Task<HttpResult<CallResult<HyperLiquidOrderResult>[]>> EditOrdersAsync(
            IEnumerable<HyperLiquidEditOrderRequest> requests,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            bool? alwaysPlace = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var orderRequests = new List<Parameters>();
            foreach (var order in requests)
            {
                if ((order.OrderId == null) == (order.ClientOrderId == null))
                    throw new ArgumentException("Either OrderId or ClientOrderId should be provided per order");

                if (order.OrderType == OrderType.Market)
                    throw new ArgumentException("Order type can't be market");

                var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                if (!symbolId.Success)
                    return HttpResult.Fail<CallResult<HyperLiquidOrderResult>[]>(_baseClient.Exchange, symbolId.Error!);

                var modifyParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                modifyParameters.Add("oid", order.OrderId);
                modifyParameters.Add("oid", order.ClientOrderId);
                var orderParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                orderParameters.Add("a", symbolId.Data);
                orderParameters.Add("b", order.Side == OrderSide.Buy);
                orderParameters.Add("p", order.Price.Normalize());
                orderParameters.Add("s", order.Quantity.Normalize());
                orderParameters.Add("r", order.ReduceOnly ?? false);

                var orderTypeParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                if (order.OrderType is OrderType.Limit or OrderType.Market)
                {
                    var limitParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                    limitParameters.Add("tif", order.OrderType == OrderType.Market ? TimeInForce.ImmediateOrCancel : order.TimeInForce ?? TimeInForce.GoodTillCanceled);
                    orderTypeParameters.Add("limit", limitParameters);
                }
                else
                {
                    if (order.TriggerPrice == null)
                        throw new ArgumentNullException(nameof(order.TriggerPrice), "Stop order should have a trigger price");

                    if (order.TpSlType == null)
                        throw new ArgumentNullException(nameof(order.TpSlType), "Stop order should have a TpSlType");

                    var triggerParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
                    triggerParameters.Add("isMarket", order.OrderType == OrderType.StopMarket);
                    triggerParameters.Add("triggerPx", order.TriggerPrice.Value.Normalize());
                    triggerParameters.Add("tpsl", order.TpSlType.Value);
                    orderTypeParameters.Add("trigger", triggerParameters);
                }

                orderParameters.Add("t", orderTypeParameters);
                orderParameters.Add("c", order.ClientOrderId);

                modifyParameters.Add("order", orderParameters);
                orderRequests.Add(modifyParameters);
            }

            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "batchModify" },
                { "modifies", orderRequests }
            };
            if (alwaysPlace == true)
                actionParameters.Add("a", alwaysPlace.Value);
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "action", actionParameters }
            };

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var intResult = await _baseClient.SendAuthAsync<HyperLiquidOrderResultIntWrapper>(request, parameters, ct, weight).ConfigureAwait(false);
            if (!intResult.Success)
                return HttpResult.Fail<CallResult<HyperLiquidOrderResult>[]>(intResult);

            var result = new List<CallResult<HyperLiquidOrderResult>>();
            foreach (var order in intResult.Data.Statuses)
            {
                if (order.Error != null)
                    result.Add(CallResult.Fail<HyperLiquidOrderResult>(new ServerError(_baseClient.GetErrorInfo("Order", order.Error))));
                else if (order.ResultResting != null)
                    result.Add(CallResult.Ok<HyperLiquidOrderResult>(order.ResultResting with { Status = OrderStatus.Open }));
                else
                    result.Add(CallResult.Ok<HyperLiquidOrderResult>(order.ResultFilled! with { Status = OrderStatus.Filled }));
            }

            return HttpResult.Ok<CallResult<HyperLiquidOrderResult>[]>(intResult, result.ToArray());
        }

        #endregion

        #region Place TWAP order

        /// <inheritdoc />
        public async Task<HttpResult<HyperLiquidTwapOrderResult>> PlaceTwapOrderAsync(
            string symbol, 
            OrderSide orderSide, 
            decimal quantity, 
            bool reduceOnly, 
            int minutes, 
            bool randomize, 
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId.Success)
                return HttpResult.Fail<HyperLiquidTwapOrderResult>(_baseClient.Exchange, symbolId.Error);

            var orderParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings);
            orderParameters.Add("a", symbolId.Data);
            orderParameters.Add("b", orderSide == OrderSide.Buy);
            orderParameters.Add("s", quantity.Normalize());
            orderParameters.Add("r", reduceOnly);
            orderParameters.Add("m", minutes);
            orderParameters.Add("t", randomize);

            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "twapOrder" },
                { "twap", orderParameters }
            };
            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "action", actionParameters }
            };
            
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidTwapOrderResultIntWrapper>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<HyperLiquidTwapOrderResult>(result);

            if (result.Data.Status.Error != null)
                return HttpResult.Fail<HyperLiquidTwapOrderResult>(result, new ServerError(_baseClient.GetErrorInfo("Order", result.Data.Status.Error)));

            return HttpResult.Ok(result, result.Data.Status.ResultRunning!);
        }

        #endregion

        #region Cancel Twap Order

        /// <inheritdoc />
        public async Task<HttpResult> CancelTwapOrderAsync(
            string symbol, 
            long twapId,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId.Success)
                return HttpResult.Fail(_baseClient.Exchange, symbolId.Error!);

            var actionParameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "type", "twapCancel" },
                { "a", symbolId.Data },
                { "t", twapId }
            };

            var parameters = new Parameters(HyperLiquidExchange._parameterSerializationSettings)
            {
                { "action", actionParameters }
            };
            
            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var request = _definitions.GetOrCreate(HttpMethod.Post, _baseClient.BaseAddress, "exchange", HyperLiquidExchange.RateLimiter.HyperLiquidRest, 1, true);
            var result = await _baseClient.SendAuthAsync<HyperLiquidTwapCancelResult>(request, parameters, ct).ConfigureAwait(false);
            if (!result.Success)
                return result;

            if (result.Data.Status != "success")
                return HttpResult.Fail(result, new ServerError(_baseClient.GetErrorInfo("Order", result.Data.Status)));

            return result;
        }

        #endregion
    }
}
