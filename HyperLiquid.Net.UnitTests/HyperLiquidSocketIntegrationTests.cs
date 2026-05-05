using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace HyperLiquid.Net.UnitTests
{
    [NonParallelizable]
    internal class HyperLiquidSocketIntegrationTests : SocketIntegrationTest<HyperLiquidSocketClient>
    {
        public override bool Run { get; set; } = false;

        public HyperLiquidSocketIntegrationTests()
        {
        }

        public override HyperLiquidSocketClient GetClient(ILoggerFactory loggerFactory)
        {
            var key = Environment.GetEnvironmentVariable("APIKEY");
            var sec = Environment.GetEnvironmentVariable("APISECRET");

            Authenticated = key != null && sec != null;
            return new HyperLiquidSocketClient(Options.Create(new HyperLiquidSocketOptions
            {
                OutputOriginalData = true,
                ApiCredentials = Authenticated ? new HyperLiquidCredentials(key, sec) : null
            }), loggerFactory);
        }

        [Test]
        public async Task TestSubscriptions()
        {
            await RunAndCheckUpdate<HyperLiquidUserUpdate>((client, updateHandler) => client.SpotApi.Account.SubscribeToUserUpdatesAsync(default, updateHandler, default), false, true);
            await RunAndCheckUpdate<HyperLiquidTicker>((client, updateHandler) => client.SpotApi.ExchangeData.SubscribeToSymbolUpdatesAsync("HYPE/USDC", updateHandler, default), true, false);

            await RunAndCheckUpdate<HyperLiquidUserTrade[]>((client, updateHandler) => client.FuturesApi.Trading.SubscribeToTwapTradeUpdatesAsync(default, updateHandler, default), false, true);
            await RunAndCheckUpdate<HyperLiquidTwapHistoryStatus[]>((client, updateHandler) => client.FuturesApi.Trading.SubscribeToTwapOrderUpdatesAsync(default, updateHandler, default), false, true);
        } 
    }
}
