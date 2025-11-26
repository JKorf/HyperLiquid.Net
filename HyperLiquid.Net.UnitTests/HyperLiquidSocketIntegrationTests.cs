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
        public override bool Run { get; set; } = true;

        public HyperLiquidSocketIntegrationTests()
        {
        }

        public override HyperLiquidSocketClient GetClient(ILoggerFactory loggerFactory, bool useUpdatedDeserialization)
        {
            var key = Environment.GetEnvironmentVariable("APIKEY");
            var sec = Environment.GetEnvironmentVariable("APISECRET");

            Authenticated = key != null && sec != null;
            return new HyperLiquidSocketClient(Options.Create(new HyperLiquidSocketOptions
            {
                OutputOriginalData = true,
                UseUpdatedDeserialization = useUpdatedDeserialization,
                ApiCredentials = Authenticated ? new CryptoExchange.Net.Authentication.ApiCredentials(key, sec) : null
            }), loggerFactory);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestSubscriptions(bool useUpdatedDeserialization)
        {
            await RunAndCheckUpdate<HyperLiquidTicker>(useUpdatedDeserialization, (client, updateHandler) => client.SpotApi.SubscribeToUserUpdatesAsync(default , default, default), false, true);
            await RunAndCheckUpdate<HyperLiquidTicker>(useUpdatedDeserialization, (client, updateHandler) => client.SpotApi.SubscribeToSymbolUpdatesAsync("HYPE/USDC", updateHandler, default), true, false);
        } 
    }
}
