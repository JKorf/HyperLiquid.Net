using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientFuturesApiExchangeData : HyperLiquidSocketClientApiExchangeData, IHyperLiquidSocketClientFuturesApiExchangeData
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientFuturesApiExchangeData(ILogger logger, HyperLiquidSocketClientApi baseClient)
            : base(logger, baseClient)
        {
        }
        #endregion

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidPerpDex[]>> GetPerpDexesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpDexs" }
            };

            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidPerpDex[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }


        public async Task<CallResult<HyperLiquidFuturesDexInfo[]>> GetExchangeInfoAllDexesAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "allPerpMetas" }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFuturesExchangeInfo[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);

            for (var j = 0; j < result.Data.Length; j++)
            {
                var data = result.Data[j];
                for (var i = 0; i < data.Symbols.Length; i++)
                {
                    var symbol = data.Symbols.ElementAt(i);
                    symbol.Index = i;
                    if (symbol.MarginTableId < 50)
                    {
                        symbol.MarginTable = new HyperLiquidMarginTableEntry
                        {
                            MarginTiers = [
                                new HyperLiquidMarginTableTier {
                                LowerBound = 0,
                                MaxLeverage = symbol.MarginTableId
                            }
                            ]
                        };
                    }
                    else
                    {
                        symbol.MarginTable = data.MarginTables.Single(x => x.Id == symbol.MarginTableId).Table;
                    }
                }
            }

            int index = 0;
            return result.As(result.Data.Select(x =>
            {
                var symbolName = x.Symbols.FirstOrDefault()?.Name;
                return new HyperLiquidFuturesDexInfo
                {
                    Name = symbolName?.Contains(':') == true ? symbolName.Split(':')[0] : "default",
                    Symbols = x.Symbols,
                    Index = index++
                };
            }).ToArray());
        }

        #region Get Futures Exchange Info

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidFuturesSymbol[]>> GetExchangeInfoAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "meta" }
            };
            parameters.AddOptional("dex", dex);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFuturesExchangeInfo>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            if (!result)
                return result.As<HyperLiquidFuturesSymbol[]>(default);

            for (var i = 0; i < result.Data.Symbols.Count(); i++)
            {
                var symbol = result.Data.Symbols.ElementAt(i);
                symbol.Index = i;
                if (symbol.MarginTableId < 50)
                {
                    symbol.MarginTable = new HyperLiquidMarginTableEntry
                    {
                        MarginTiers = [
                            new HyperLiquidMarginTableTier {
                                LowerBound = 0,
                                MaxLeverage = symbol.MarginTableId
                            }
                        ]
                    };
                }
                else
                {
                    symbol.MarginTable = result.Data.MarginTables.Single(x => x.Id == symbol.MarginTableId).Table;
                }
            }

            return result.As<HyperLiquidFuturesSymbol[]>(result.Data?.Symbols);
        }

        #endregion

        #region Get Futures Exchange Info And Tickers

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidFuturesExchangeInfoAndTickers>> GetExchangeInfoAndTickersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "metaAndAssetCtxs" }
            };

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFuturesExchangeInfoAndTickers>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            if (!result)
                return result;

            for (var i = 0; i < result.Data.ExchangeInfo.Symbols.Count(); i++)
                result.Data.ExchangeInfo.Symbols.ElementAt(i).Index = i;

            for (var i = 0; i < result.Data.Tickers.Count(); i++)
                result.Data.Tickers.ElementAt(i).Symbol = result.Data.ExchangeInfo.Symbols.ElementAt(i).Name;

            return result;
        }

        #endregion

        #region Get Funding Rate History

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidFundingRate[]>> GetFundingRateHistoryAsync(string symbol, DateTime startTime, DateTime? endTime = null, CancellationToken ct = default)
        {
            var innerParameters = new ParameterCollection();
            var parameters = new ParameterCollection()
            {
                { "type", "fundingHistory" },
            };
            parameters.Add("coin", symbol);
            parameters.AddMilliseconds("startTime", startTime);
            parameters.AddOptionalMilliseconds("endTime", endTime);

            var result = await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidFundingRate[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
            //if (result.ResponseStatusCode == (HttpStatusCode)500 && result.Error?.ErrorType == ErrorType.Unknown)
            //    return result.AsError<HyperLiquidFundingRate[]>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return result;
        }

        #endregion

        #region Get Futures Symbols At Max Open Interest

        /// <inheritdoc />
        public async Task<CallResult<string[]>> GetSymbolsAtMaxOpenInterestAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpsAtOpenInterestCap" },
            };
            parameters.AddOptional("dex", dex);
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<string[]>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Perp DEX Market Limits

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidPerpDexLimit>> GetPerpDexMarketLimitsAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpDexLimits" },
            };
            parameters.AddOptional("dex", dex);

            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidPerpDexLimit>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        #region Get Perp Market Status

        /// <inheritdoc />
        public async Task<CallResult<HyperLiquidPerpDexStatus>> GetPerpDexMarketStatusAsync(string? dex = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection()
            {
                { "type", "perpDexStatus" },
            };
            parameters.AddOptional("dex", dex);
            return await _baseClient.QueryInternalAsync(
                new HyperLiquidRequestQuery<HyperLiquidPerpDexStatus>(_baseClient, "post", "info", parameters, false), ct).ConfigureAwait(false);
        }

        #endregion

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToSymbolUpdatesAsync(string symbol, Action<DataEvent<HyperLiquidFuturesTicker>> onMessage, CancellationToken ct = default)
        {
            var internalHandler = new Action<DateTime, string?, int, HyperLiquidSocketUpdate<HyperLiquidFuturesTickerUpdate>>((receiveTime, originalData, invocation, data) =>
            {
                data.Data.Ticker.Symbol = symbol;
                onMessage(
                    new DataEvent<HyperLiquidFuturesTicker>(HyperLiquidExchange.ExchangeName, data.Data.Ticker, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(data.Channel)
                        .WithSymbol(symbol)
                    );
            });

            var subscription = new HyperLiquidSubscription<HyperLiquidFuturesTickerUpdate>(_logger, _baseClient, "activeAssetCtx", symbol, new Dictionary<string, object>
            {
                { "coin", symbol },
            },
            internalHandler, false);
            return await _baseClient.SubscribeInternalAsync(subscription, ct).ConfigureAwait(false);
        }
    }
}
