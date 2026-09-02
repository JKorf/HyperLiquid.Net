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
    internal partial class HyperLiquidRestClientFuturesSharedApi
    {
        #region Futures Symbol client

        public SharedSymbolCatalog? FuturesSymbolCatalog => ExchangeSymbolCache.GetSymbolCatalog(_exchangeName, _topicId, _api.EnvironmentName, null);

        public GetFuturesSymbolsOptions GetFuturesSymbolsOptions { get; } = new GetFuturesSymbolsOptions(_exchangeName, false)
        {
            OptionalExchangeParameters = new List<ParameterDescription>
            {
                new ParameterDescription("dex", typeof(string), "DEX to retrieve symbols for", "xyz")
            }
        };

        public async Task<HttpResult<SharedFuturesSymbol[]>> GetFuturesSymbolsAsync(GetSymbolsRequest request, CancellationToken ct)
        {
            var validationError = GetFuturesSymbolsOptions.ValidateRequest(request, this);
            if (validationError != null)
                return HttpResult.Fail<SharedFuturesSymbol[]>(Exchange, validationError);

            string? dex = ExchangeParameters.GetValue<string>(request.ExchangeParameters, Exchange, "dex");
            var annotationsTask = _api.ExchangeData.GetPerpConciseAnnotationsAsync(ct: ct);
            var exchangeInfoTask = _api.ExchangeData.GetExchangeInfoAllDexesAsync(ct: ct);
            await Task.WhenAll(annotationsTask, exchangeInfoTask).ConfigureAwait(false);
            var annotationsResult = annotationsTask.Result;
            var exchangeInfoResult = exchangeInfoTask.Result;
            if (!exchangeInfoResult.Success)
                return HttpResult.Fail<SharedFuturesSymbol[]>(exchangeInfoResult);
            if (!annotationsResult.Success)
                return HttpResult.Fail<SharedFuturesSymbol[]>(annotationsResult);

            var data = exchangeInfoResult.Data.SelectMany(x => x.Symbols);
            if (dex != null)
                data = exchangeInfoResult.Data.SingleOrDefault(x => x.Name == dex)?.Symbols ?? [];

            var resultData = data
               .Select(x => ParseSymbol(x, annotationsResult.Data))
               .ToArray();

            // Register both HYPE/USDC and HYPE as symbol names
            var symbolRegistrations = resultData
                .Concat(resultData.Select(x => new SharedSpotSymbol(x.BaseAsset, "USDC", x.BaseAsset, true, TradingMode.PerpetualLinear))).ToArray();

            ExchangeSymbolCache.UpdateSymbolInfo(_topicId, _api.EnvironmentName, dex, symbolRegistrations);
            return HttpResult.Ok(exchangeInfoResult, SharedUtils.ApplySymbolFilter(resultData, request));
        }

        private SharedFuturesSymbol ParseSymbol(HyperLiquidFuturesSymbol symbol, HyperLiquidPerpAnnotation[] annotations)
        {
            var result = new SharedFuturesSymbol(
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
                MaxShortLeverage = symbol.MaxLeverage,
                DisplayName = symbol.Name,
                QuoteAssetType = SharedAssetType.Crypto,
                QuoteAssetSubType = SharedAssetSubType.StableCoin
            };

            var annotation = annotations.SingleOrDefault(x => x.Symbol == symbol.Name);
            if (annotation == null)
            {
                if (symbol.Name.Contains(":"))
                {
                    // No annotation and on a dex, could be any type since annotations aren't always assigned
                    var baseAssetName = result.BaseAsset.Split(':')[1];
                    if (LibraryHelpers.IsEquity(baseAssetName))
                    {
                        result.BaseAssetType = SharedAssetType.TradFi;
                        result.BaseAssetSubType = SharedAssetSubType.Equity;
                    }
                    else if (LibraryHelpers.IsCommodity(baseAssetName))
                    {
                        result.BaseAssetType = SharedAssetType.TradFi;
                        result.BaseAssetSubType = SharedAssetSubType.Commodity;
                    }
                    else if (_exchangeKnownFiat.Contains(baseAssetName))
                    {
                        result.BaseAssetType = SharedAssetType.Fiat;
                    }
                    else
                    {
                        result.BaseAssetType = SharedAssetType.Crypto;
                    }
                }
                else
                {
                    // Main exchange only has crypto
                    result.BaseAssetType = SharedAssetType.Crypto; 
                }
            }
            else if (annotation.Annotations.Category.Equals("crypto", StringComparison.OrdinalIgnoreCase))
            {
                result.BaseAssetType = SharedAssetType.Crypto;
            }
            else if (annotation.Annotations.Category.Equals("indices", StringComparison.OrdinalIgnoreCase))
            {
                result.BaseAssetType = SharedAssetType.TradFi;
                result.BaseAssetSubType = SharedAssetSubType.Equity;
            }
            else if (annotation.Annotations.Category.Equals("fx", StringComparison.OrdinalIgnoreCase))
            {
                result.BaseAssetType = SharedAssetType.Fiat;
            }
            else if (annotation.Annotations.Category.Equals("stocks", StringComparison.OrdinalIgnoreCase))
            {
                result.BaseAssetType = SharedAssetType.TradFi;
                result.BaseAssetSubType = SharedAssetSubType.Equity;
            }
            else if (annotation.Annotations.Category.Equals("commodities", StringComparison.OrdinalIgnoreCase))
            {
                result.BaseAssetType = SharedAssetType.TradFi;
                result.BaseAssetSubType = SharedAssetSubType.Commodity;
            }
            else if (annotation.Annotations.Category.Equals("preipo", StringComparison.OrdinalIgnoreCase))
            {
                result.BaseAssetType = SharedAssetType.TradFi;
            }

            return result;
        }

        public async Task<ExchangeCallResult<SharedSymbol[]>> GetFuturesSymbolsForBaseAssetAsync(string baseAsset)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId, _api.EnvironmentName, null))
            {
                var symbols = await GetFuturesSymbolsAsync(new GetSymbolsRequest(), default).ConfigureAwait(false);
                if (!symbols.Success)
                    return ExchangeCallResult<SharedSymbol[]>.Fail(Exchange, symbols.Error!);
            }

            return ExchangeCallResult<SharedSymbol[]>.Ok(Exchange, ExchangeSymbolCache.GetSymbolsForBaseAsset(_topicId, _api.EnvironmentName, null, baseAsset));
        }

        public async Task<ExchangeCallResult<bool>> SupportsFuturesSymbolAsync(SharedSymbol symbol)
        {
            if (symbol.TradingMode == TradingMode.Spot)
                throw new ArgumentException(nameof(symbol), "Spot symbols not allowed");

            if (!ExchangeSymbolCache.HasCached(_topicId, _api.EnvironmentName, null))
            {
                var symbols = await GetFuturesSymbolsAsync(new GetSymbolsRequest(), default).ConfigureAwait(false);
                if (!symbols.Success)
                    return ExchangeCallResult<bool>.Fail(Exchange, symbols.Error!);
            }

            return ExchangeCallResult<bool>.Ok(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, _api.EnvironmentName, null, symbol));
        }

        public async Task<ExchangeCallResult<bool>> SupportsFuturesSymbolAsync(string symbolName)
        {
            if (!ExchangeSymbolCache.HasCached(_topicId, _api.EnvironmentName, null))
            {
                var symbols = await GetFuturesSymbolsAsync(new GetSymbolsRequest(), default).ConfigureAwait(false);
                if (!symbols.Success)
                    return ExchangeCallResult<bool>.Fail(Exchange, symbols.Error!);
            }

            return ExchangeCallResult<bool>.Ok(Exchange, ExchangeSymbolCache.SupportsSymbol(_topicId, _api.EnvironmentName, null, symbolName));
        }
        #endregion
    }
}
