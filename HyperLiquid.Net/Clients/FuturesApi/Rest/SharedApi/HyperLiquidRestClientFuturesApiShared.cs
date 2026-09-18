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
    internal partial class HyperLiquidRestClientFuturesSharedApi :
        SharedApiBase,
        IHyperLiquidRestClientFuturesApiShared,
        IHyperLiquidRestClientFuturesSharedApi
    {
        private readonly HyperLiquidRestClientFuturesApi _api;

        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidFutures";
        public override SharedClientInfo Discover() => SharedUtils.GetClientInfo(HyperLiquidExchange.Metadata, this);

        private static readonly HashSet<string> _exchangeKnownFiat = ["EUR", "USD"];

        public HyperLiquidRestClientFuturesSharedApi(HyperLiquidRestClientFuturesApi api)
           : base(
                 SharedTransport.Rest,
                 api,
                 [TradingMode.PerpetualLinear],
                 () => api.Authenticated,
                 api.FormatSymbol)
        {
            _api = api;

            SetCapabilities(
                GetBalancesOptions,
                GetKlinesOptions,
                GetOrderBookOptions,
                GetTickerOptions,
                GetAllTickersOptions,
                GetBookTickerOptions,
                GetFuturesSymbolsOptions,
                GetFeeOptions,
                GetFundingRateHistoryOptions,
                GetLeverageOptions,
                SetLeverageOptions,
                GetOpenInterestOptions,
                PlaceFuturesOrderOptions,
                GetFuturesOrderOptions,
                GetOpenFuturesOrdersOptions,
                GetClosedFuturesOrdersOptions,
                GetFuturesOrderTradesOptions,
                GetFuturesUserTradeHistoryOptions,
                CancelFuturesOrderOptions,
                GetPositionsOptions,
                GetFuturesOrderByClientOrderIdOptions,
                CancelFuturesOrderByClientOrderIdOptions,
                SetFuturesTpSlOptions,
                CancelFuturesTpSlOptions
                );
        }
    }
}
