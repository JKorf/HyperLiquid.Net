using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture]
    public class SocketSubscriptionTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public async Task ValidateSpotSubscriptions(bool useUpdatedDeserialization)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TraceLoggerProvider());
            var client = new HyperLiquidSocketClient(Options.Create(new HyperLiquidSocketOptions
            {
                OutputOriginalData = true,
                Environment = HyperLiquidEnvironment.CreateCustom("UnitTest", "https://api.hyperliquid.xyz", "wss://api.hyperliquid.xyz"),
                UseUpdatedDeserialization = useUpdatedDeserialization,
                ApiCredentials = new ApiCredentials("MTIz", "MTIz")
            }), loggerFactory);
            var tester = new SocketSubscriptionValidator<HyperLiquidSocketClient>(client, "Subscriptions", "wss://api.hyperliquid.xyz", "data");
            await tester.ValidateAsync<HyperLiquidTrade[]>((client, handler) => client.SpotApi.SubscribeToTradeUpdatesAsync("HYPE", handler), "Trades");
            await tester.ValidateAsync<Dictionary<string, decimal>>((client, handler) => client.SpotApi.SubscribeToPriceUpdatesAsync(handler), "Prices", nestedJsonProperty: "data.mids");
            await tester.ValidateAsync<HyperLiquidKline>((client, handler) => client.SpotApi.SubscribeToKlineUpdatesAsync("HYPE", Enums.KlineInterval.OneDay, handler), "Klines");
            await tester.ValidateAsync<HyperLiquidOrderBook>((client, handler) => client.SpotApi.SubscribeToOrderBookUpdatesAsync("HYPE", handler), "OrderBook");
            await tester.ValidateAsync<HyperLiquidOrderStatus[]>((client, handler) => client.SpotApi.SubscribeToOrderUpdatesAsync(null, handler), "Order");
            await tester.ValidateAsync<HyperLiquidUserTrade[]>((client, handler) => client.SpotApi.SubscribeToUserEventUpdatesAsync(null, handler), "UserEventTrade", nestedJsonProperty: "data.fills");
        }
    }
}
