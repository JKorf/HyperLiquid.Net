using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.SpotApi
{
    internal partial class HyperLiquidSocketClientSpotSharedApi :
        SharedApiBase,
        IHyperLiquidSocketClientSpotApiShared,
        IHyperLiquidSocketClientSpotSharedApi
    {
        private readonly HyperLiquidSocketClientSpotApi _api;

        private const string _exchangeName = "HyperLiquid";
        private const string _topicId = "HyperLiquidSpot";

        public override SharedClientInfo Discover() => SharedUtils.GetClientInfo(HyperLiquidExchange.Metadata, this);

        public HyperLiquidSocketClientSpotSharedApi(HyperLiquidSocketClientSpotApi api)
            : base(
                  SharedTransport.Socket,
                  api,
                  [TradingMode.Spot],
                  () => api.Authenticated,
                  api.FormatSymbol)
        {
            _api = api;

            SetCapabilities(
                SubscribeTickerOptions,
                SubscribeTradeOptions,
                SubscribeKlineOptions,
                SubscribeOrderBookOptions,
                SubscribeSpotOrderOptions,
                SubscribeUserTradeOptions,
                SubscribeBalanceOptions,
                SubscribeBookTickerOptions,
                PlaceSpotOrderOptions,
                CancelSpotOrderOptions
                );
        }
    }
}
