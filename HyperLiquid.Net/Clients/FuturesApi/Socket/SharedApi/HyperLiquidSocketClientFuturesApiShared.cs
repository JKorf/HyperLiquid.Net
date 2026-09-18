using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    internal partial class HyperLiquidSocketClientFuturesSharedApi :
        SharedApiBase,
        IHyperLiquidSocketClientFuturesApiShared,
        IHyperLiquidSocketClientFuturesSharedApi
    {
        private readonly HyperLiquidSocketClientFuturesApi _api;

        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidFutures";

        public override SharedClientInfo Discover() => SharedUtils.GetClientInfo(HyperLiquidExchange.Metadata, this);

        public HyperLiquidSocketClientFuturesSharedApi(HyperLiquidSocketClientFuturesApi api)
            : base(
                  SharedTransport.Socket,
                  api,
                  [TradingMode.PerpetualLinear],
                  () => api.Authenticated,
                  api.FormatSymbol)
        {
            _api = api;

            SetCapabilities(
                SubscribeTickerOptions,
                SubscribeTradeOptions,
                SubscribeKlineOptions,
                SubscribeOrderBookOptions,
                SubscribeUserTradeOptions,
                SubscribeFuturesOrderOptions,
                SubscribeBalanceOptions,
                SubscribePositionOptions,
                SubscribeBookTickerOptions,
                PlaceFuturesOrderOptions,
                CancelFuturesOrderOptions
                );
        }
    }
}
