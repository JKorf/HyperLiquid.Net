using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients.BaseApi;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using HyperLiquid.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal partial class HyperLiquidSocketClientApiTrading : IHyperLiquidSocketClientApiTrading
    {
        protected readonly HyperLiquidSocketClientApi _baseClient;
        protected readonly ILogger _logger;

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientApiTrading(ILogger logger, HyperLiquidSocketClientApi baseClient)
        {
            _logger = logger;
            _baseClient = baseClient;
        }
        #endregion


        #region Get Open Orders

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidOpenOrder[]>> GetOpenOrdersAsync(string? address = null, string? dex = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "openOrders" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key  }
            };
            parameters.AddOptional("dex", dex);
            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOpenOrder[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
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
        public async Task<CallResult<HyperLiquidOrder[]>> GetOpenOrdersExtendedAsync(string? address = null, string? dex = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "frontendOpenOrders" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.AddOptional("dex", dex);
            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOrder[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
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
        public async Task<CallResult<HyperLiquidUserTrade[]>> GetUserTradesAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userFills" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidUserTrade[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
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
        public async Task<CallResult<HyperLiquidUserTrade[]>> GetUserTradesByTimeAsync(
            DateTime startTime,
            DateTime? endTime = null,
            bool? aggregateByTime = null,
            string? address = null,
            CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "userFillsByTime" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);
            parameters.AddOptional("aggregateByTime", aggregateByTime);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidUserTrade[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
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
        public async Task<CallResult<HyperLiquidOrderStatus>> GetOrderAsync(long? orderId = null, string? clientOrderId = null, string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "orderStatus" },
                { "user", address ?? _baseClient.AuthenticationProvider!.Key }
            };

            parameters.AddOptional("oid", orderId);
            parameters.AddOptional("oid", clientOrderId);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOrderStatusResult>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            if (!result)
                return result.As<HyperLiquidOrderStatus>(default);

            if (result.Data.Status != "order")
                return result.AsError<HyperLiquidOrderStatus>(new ServerError(new ErrorInfo(ErrorType.Unknown, result.Data.Status)));

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
        public async Task<CallResult<HyperLiquidOrderStatus[]>> GetOrderHistoryAsync(string? address = null, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection()
            {
                { "type", "historicalOrders" },
                { "user",  address ?? _baseClient.AuthenticationProvider!.Key }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOrderStatus[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
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

        public async Task<CallResult<HyperLiquidOrderResult>> PlaceOrderAsync(
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
        public async Task<CallResult<CallResult<HyperLiquidOrderResult>[]>> PlaceMultipleOrdersAsync(
            IEnumerable<HyperLiquidOrderRequest> orders,
            TpSlGrouping? tpSlGrouping = null,
            string? vaultAddress = null,
            DateTime? expireAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

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
                    orderParameters.AddString("s", order.Quantity.Normalize());
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var limitParameters = new ParameterCollection();
                    limitParameters.AddEnum("tif", order.TimeInForce ?? TimeInForce.GoodTillCanceled);
                    orderTypeParameters.Add("limit", limitParameters);
                }
                else if (order.OrderType == OrderType.Market)
                {
                    var maxSlippage = order.MaxSlippage ?? 5;
                    var price = (order.Side == OrderSide.Buy ? order.Price * (1 + maxSlippage / 100m) : order.Price * (1 - maxSlippage / 100m)) ?? 0;
                    var quantityDecimals = await HyperLiquidUtils.GetQuantityDecimalPlacesForSymbolAsync(_baseClient.BaseClient, order.Symbol).ConfigureAwait(false);
                    if (!quantityDecimals)
                        return new WebCallResult<CallResult<HyperLiquidOrderResult>[]>(quantityDecimals.Error);

                    // Price can be a max of 5 significant figures
                    // but no more than (Spot:8, Futures:6) - quantityDecimals decimal places for the symbol
                    var decimalMax = symbolId.Data < 10000 ? 6 : 8; // Spot symbols have id >= 10000
                    price = ExchangeHelpers.RoundToSignificantDigits(price, 5, RoundingType.Closest);
                    price = ExchangeHelpers.RoundDown(price, decimalMax - quantityDecimals.Data);

                    orderParameters.AddString("p", price.Normalize());
                    orderParameters.AddString("s", order.Quantity.Normalize());
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var limitParameters = new ParameterCollection();
                    limitParameters.AddEnum("tif", order.TimeInForce ?? TimeInForce.ImmediateOrCancel);
                    orderTypeParameters.Add("limit", limitParameters);
                }
                else
                {
                    if (order.TriggerPrice == null)
                        throw new ArgumentNullException(nameof(order.TriggerPrice), "Stop order should have a trigger price");

                    if (order.TpSlType == null)
                        throw new ArgumentNullException(nameof(order.TpSlType), "Stop order should have a TpSlType");

                    orderParameters.AddString("p", order.Price?.Normalize() ?? 0);
                    orderParameters.AddString("s", order.Quantity.Normalize());
                    orderParameters.Add("r", order.ReduceOnly ?? false);
                    var triggerParameters = new ParameterCollection();
                    triggerParameters.Add("isMarket", order.OrderType == OrderType.StopMarket);
                    triggerParameters.AddString("triggerPx", order.TriggerPrice.Value.Normalize());
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

            if (_baseClient.ClientOptions.BuilderFeePercentage > 0 && _baseClient.ClientOptions.BuilderAddress != null)
            {
                // Convert from percentage to 1/10 basis point
                var tenthPoints = (int)(_baseClient.ClientOptions.BuilderFeePercentage * 1000);
                actionParameters.Add("builder",
                    new ParameterCollection
                    {
                        { "b", _baseClient.ClientOptions.BuilderAddress.ToLower() },
                        { "f", tenthPoints }
                    }
                );
            }

            var parameters = new ParameterCollection()
            {
                { "action", actionParameters }
            };

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expireAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);

            var intResult = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOrderResultIntWrapper>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            if (!intResult)
                return intResult.As<CallResult<HyperLiquidOrderResult>[]>(default);

            var result = new List<CallResult<HyperLiquidOrderResult>>();
            foreach (var order in intResult.Data.Statuses)
            {
                if (order.Error != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(new ServerError(_baseClient.GetErrorInfo("Order", order.Error))));
                else if (order.ResultResting != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.ResultResting with { Status = OrderStatus.Open }));
                else if (order.ResultFilled != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.ResultFilled! with { Status = OrderStatus.Filled }));
                else if (order.WaitingForFill != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.WaitingForFill! with { Status = OrderStatus.WaitingTrigger }));
                else
                    result.Add(new CallResult<HyperLiquidOrderResult>(order.WaitingForTrigger! with { Status = OrderStatus.WaitingTrigger }));
            }

            if (result.Count > 1 && result.All(x => !x.Success))
                return intResult.AsErrorWithData<CallResult<HyperLiquidOrderResult>[]>(new ServerError(new ErrorInfo(ErrorType.AllOrdersFailed, "All orders failed")), result.ToArray());

            return intResult.As<CallResult<HyperLiquidOrderResult>[]>(result.ToArray());
        }

        #endregion

        #region Cancel Order

        public async Task<CallResult> CancelOrderAsync(
            string symbol,
            long orderId,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var result = await CancelOrdersAsync([new HyperLiquidCancelRequest(symbol, orderId)], vaultAddress, expiresAfter, ct).ConfigureAwait(false);
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
        public async Task<CallResult<CallResult[]>> CancelOrdersAsync(
            IEnumerable<HyperLiquidCancelRequest> requests,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

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

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var resultInt = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidCancelResult>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            if (!resultInt)
                return resultInt.AsError<CallResult[]>(resultInt.Error!);

            var result = new List<CallResult>();
            foreach (var order in resultInt.Data.Statuses)
            {
                if (order.Equals("success"))
                    result.Add(CallResult.SuccessResult);
                else
                    result.Add(new CallResult(new ServerError(_baseClient.GetErrorInfo("Order", order))));
            }

            return resultInt.As<CallResult[]>(result.ToArray());
        }

        #endregion

        #region Cancel Order By Client Order Id

        public async Task<CallResult> CancelOrderByClientOrderIdAsync(
            string symbol,
            string clientOrderId,
            string? vaultAddress,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            var result = await CancelOrdersByClientOrderIdAsync([new HyperLiquidCancelByClientOrderIdRequest(symbol, clientOrderId)], vaultAddress, expiresAfter, ct).ConfigureAwait(false);
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
        public async Task<CallResult<CallResult[]>> CancelOrdersByClientOrderIdAsync(
            IEnumerable<HyperLiquidCancelByClientOrderIdRequest> requests,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

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

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var resultInt = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidCancelResult>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            if (!resultInt)
                return resultInt.AsError<CallResult[]>(resultInt.Error!);

            var result = new List<CallResult>();
            foreach (var order in resultInt.Data.Statuses)
            {
                if (order.Equals("success"))
                    result.Add(CallResult.SuccessResult);
                else
                    result.Add(new CallResult(new ServerError(_baseClient.GetErrorInfo("Order", order))));
            }

            return resultInt.As<CallResult[]>(result.ToArray());
        }

        #endregion

        #region Cancel after

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidOrderStatus[]>> CancelAfterAsync(
            TimeSpan? timeout,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var parameters = new ParameterCollection();
            var actionParameters = new ParameterCollection()
            {
                { "type", "scheduleCancel" }
            };
            actionParameters.AddOptionalMilliseconds("time", timeout == null ? null : DateTime.UtcNow + timeout);
            parameters.Add("action", actionParameters);

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOrderStatus[]>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result;
        }

        #endregion

        #region Edit Order

        /// <inheritdoc />
        public async Task<CallResult> EditOrderAsync(
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
            CancellationToken ct = default)
        {
            if ((orderId == null) == (clientOrderId == null))
                throw new ArgumentException("Either orderId or clientOrderId should be provided");

            if (orderType == OrderType.Market)
                throw new ArgumentException("Order type can't be market");

            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var symbolId = await HyperLiquidUtils.GetSymbolIdFromNameAsync(_baseClient.BaseClient, symbol).ConfigureAwait(false);
            if (!symbolId)
                return new WebCallResult(symbolId.Error!);

            var orderParameters = new ParameterCollection();
            orderParameters.Add("a", symbolId.Data);
            orderParameters.Add("b", side == OrderSide.Buy);
            orderParameters.AddString("p", price.Normalize());
            orderParameters.AddString("s", quantity.Normalize());
            orderParameters.Add("r", reduceOnly ?? false);

            var orderTypeParameters = new ParameterCollection();
            if (orderType == OrderType.Limit)
            {
                var limitParameters = new ParameterCollection();
                limitParameters.AddEnum("tif", timeInForce ?? TimeInForce.GoodTillCanceled);
                orderTypeParameters.Add("limit", limitParameters);
            }
            else
            {
                if (triggerPrice == null)
                    throw new ArgumentNullException(nameof(triggerPrice), "Stop order should have a trigger price");

                if (tpSlType == null)
                    throw new ArgumentNullException(nameof(tpSlType), "Stop order should have a TpSlType");

                var triggerParameters = new ParameterCollection();
                triggerParameters.Add("isMarket", orderType == OrderType.StopMarket);
                triggerParameters.AddString("triggerPx", triggerPrice.Value.Normalize());
                triggerParameters.AddEnum("tpsl", tpSlType.Value);
                orderTypeParameters.Add("trigger", triggerParameters);
            }

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

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<object>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            return result.AsDataless();
        }

        #endregion

        #region Edit Orders

        /// <inheritdoc />
        public async Task<CallResult<CallResult<HyperLiquidOrderResult>[]>> EditOrdersAsync(
            IEnumerable<HyperLiquidEditOrderRequest> requests,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

            var orderRequests = new List<ParameterCollection>();
            foreach (var order in requests)
            {
                if ((order.OrderId == null) == (order.ClientOrderId == null))
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
                orderParameters.AddString("p", order.Price.Normalize());
                orderParameters.AddString("s", order.Quantity.Normalize());
                orderParameters.Add("r", order.ReduceOnly ?? false);

                var orderTypeParameters = new ParameterCollection();
                if (order.OrderType is OrderType.Limit or OrderType.Market)
                {
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

                    var triggerParameters = new ParameterCollection();
                    triggerParameters.Add("isMarket", order.OrderType == OrderType.StopMarket);
                    triggerParameters.AddString("triggerPx", order.TriggerPrice.Value.Normalize());
                    triggerParameters.AddEnum("tpsl", order.TpSlType.Value);
                    orderTypeParameters.Add("trigger", triggerParameters);
                }

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

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var weight = 1 + (int)Math.Floor(orderRequests.Count / 40m);
            var intResult = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidOrderResultIntWrapper>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            if (!intResult)
                return intResult.As<CallResult<HyperLiquidOrderResult>[]>(default);

            var result = new List<CallResult<HyperLiquidOrderResult>>();
            foreach (var order in intResult.Data.Statuses)
            {
                if (order.Error != null)
                    result.Add(new CallResult<HyperLiquidOrderResult>(new ServerError(_baseClient.GetErrorInfo("Order", order.Error))));
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
        public async Task<CallResult<HyperLiquidTwapOrderResult>> PlaceTwapOrderAsync(
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
            if (!symbolId)
                return new WebCallResult<HyperLiquidTwapOrderResult>(symbolId.Error);

            var orderParameters = new ParameterCollection();
            orderParameters.Add("a", symbolId.Data);
            orderParameters.Add("b", orderSide == OrderSide.Buy);
            orderParameters.AddString("s", quantity.Normalize());
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

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidTwapOrderResultIntWrapper>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            if (!result)
                return result.As<HyperLiquidTwapOrderResult>(default);

            if (result.Data.Status.Error != null)
                return result.AsError<HyperLiquidTwapOrderResult>(new ServerError(_baseClient.GetErrorInfo("Order", result.Data.Status.Error)));

            return result.As(result.Data.Status.ResultRunning!);
        }

        #endregion

        #region Cancel Twap Order

        /// <inheritdoc />
        public async Task<CallResult> CancelTwapOrderAsync(
            string symbol,
            long twapId,
            string? vaultAddress = null,
            DateTime? expiresAfter = null,
            CancellationToken ct = default)
        {
            await HyperLiquidUtils.CheckBuilderFeeAsync(_baseClient.BaseClient).ConfigureAwait(false);

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

            if (vaultAddress != null)
                parameters.Add("vaultAddress", vaultAddress);

            _baseClient.AddExpiresAfter(parameters, expiresAfter);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidTwapCancelResult>(_baseClient, "post", "action", parameters, true), ct).ConfigureAwait(false);
            if (!result)
                return result.AsDataless();

            if (result.Data.Status != "success")
                return result.AsDatalessError(new ServerError(_baseClient.GetErrorInfo("Order", result.Data.Status)));

            return result.AsDataless();
        }

        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOrderUpdatesAsync(string? address, Action<DataEvent<HyperLiquidOrderStatus[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidOrderStatus[]>>((receiveTime, originalData, invocation, data) =>
            {
                var timestamp = data.Data.Max(x => x.Timestamp);
                if (invocation != 1)
                    _baseClient.UpdateTimeOffset(timestamp);

                foreach (var order in data.Data)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.Order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(_baseClient.ClientOptions.Environment.Name, order.Order.ExchangeSymbol);
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

                onMessage(
                    new DataEvent<HyperLiquidOrderStatus[]>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidOrderStatus[]>(_logger, _baseClient, "orderUpdates", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToOpenOrderUpdatesAsync(string? address, string? dex, Action<DataEvent<HyperLiquidOpenOrderUpdate>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidOpenOrderUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                DateTime? timestamp = null;
                if (data.Data.Orders.Length > 0)
                {
                    timestamp = data.Data.Orders.Max(x => x.Timestamp);
                    if (invocation != 1)
                        _baseClient.UpdateTimeOffset(timestamp.Value);
                }

                foreach (var order in data.Data.Orders)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(_baseClient.ClientOptions.Environment.Name, order.ExchangeSymbol);
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

                onMessage(
                    new DataEvent<HyperLiquidOpenOrderUpdate>(HyperLiquidExchange.ExchangeName, data.Data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidOpenOrderUpdate>(_logger, _baseClient, "openOrders", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
                { "dex", dex ?? "" },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToUserTradeUpdatesAsync(string? address, Action<DataEvent<HyperLiquidUserTrade[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidUserTradeUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                DateTime? timestamp = data.Data.Trades.Length != 0 ? data.Data.Trades.Max(x => x.Timestamp) : null;
                if (!data.Data.IsSnapshot && timestamp != null)
                    _baseClient.UpdateTimeOffset(timestamp!.Value);

                foreach (var order in data.Data.Trades)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(_baseClient.ClientOptions.Environment.Name, order.ExchangeSymbol);
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

                onMessage(
                    new DataEvent<HyperLiquidUserTrade[]>(HyperLiquidExchange.ExchangeName, data.Data.Trades, receiveTime, originalData)
                        .WithStreamId(data.Channel)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidUserTradeUpdate>(_logger, _baseClient, "userFills", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTwapTradeUpdatesAsync(string? address, Action<DataEvent<HyperLiquidTwapStatus[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTwapTradeUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                foreach (var order in data.Data.Trades)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(_baseClient.ClientOptions.Environment.Name, order.ExchangeSymbol);
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

                DateTime? timestamp = data.Data.Trades.Any() ? data.Data.Trades.Max(x => x.Timestamp) : null;
                if (timestamp != null)
                    _baseClient.UpdateTimeOffset(timestamp.Value);

                onMessage(
                    new DataEvent<HyperLiquidTwapStatus[]>(HyperLiquidExchange.ExchangeName, data.Data.Trades, receiveTime, originalData)
                        .WithStreamId(data.Channel)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidTwapTradeUpdate>(_logger, _baseClient, "userTwapSliceFills", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToTwapOrderUpdatesAsync(string? address, Action<DataEvent<HyperLiquidTwapOrderStatus[]>> onMessage, CancellationToken ct = default)
        {
            if (address == null && _baseClient.AuthenticationProvider == null)
                throw new ArgumentNullException(nameof(address), "Address needs to be provided if API credentials not set");

            _baseClient.ValidateAddress(address);

            var result = await HyperLiquidUtils.UpdateSpotSymbolInfoAsync(_baseClient.BaseClient).ConfigureAwait(false);
            if (!result)
                return new CallResult<UpdateSubscription>(result.Error!);

            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidTwapOrderUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                DateTime? timestamp = data.Data.History.Length != 0 ? data.Data.History.Max(x => x.Timestamp) : null;
                if (!data.Data.IsSnapshot && timestamp != null)
                    _baseClient.UpdateTimeOffset(timestamp!.Value);

                foreach (var order in data.Data.History)
                {
                    if (HyperLiquidUtils.ExchangeSymbolIsSpotSymbol(order.TwapInfo.ExchangeSymbol))
                    {
                        var symbolName = HyperLiquidUtils.GetSymbolNameFromExchangeName(_baseClient.ClientOptions.Environment.Name, order.TwapInfo.ExchangeSymbol);
                        if (symbolName == null)
                            continue;

                        order.TwapInfo.Symbol = symbolName.Data;
                        order.TwapInfo.SymbolType = SymbolType.Spot;
                    }
                    else
                    {
                        order.TwapInfo.Symbol = order.TwapInfo.ExchangeSymbol;
                        order.TwapInfo.SymbolType = SymbolType.Futures;
                    }
                }

                onMessage(
                    new DataEvent<HyperLiquidTwapOrderStatus[]>(HyperLiquidExchange.ExchangeName, data.Data.History, receiveTime, originalData)
                        .WithStreamId(data.Channel)
                        .WithUpdateType(data.Data.IsSnapshot ? SocketUpdateType.Snapshot : SocketUpdateType.Update)
                        .WithDataTimestamp(timestamp, _baseClient.GetTimeOffset())
                    );
            });

            var addressSub = address ?? _baseClient.AuthenticationProvider!.Key;
            var subscription = new HyperLiquidSubscription<HyperLiquidTwapOrderUpdate>(_logger, _baseClient, "userTwapHistory", null, new Dictionary<string, object>
            {
                { "user", addressSub.ToLowerInvariant() },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }

    }
}
