using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Objects.Options;

namespace HyperLiquid.Net.UnitTests
{
    [NonParallelizable]
    public class HyperLiquidRestIntegrationTests : RestIntergrationTest<HyperLiquidRestClient>
    {
        public override bool Run { get; set; } = true;

        public override HyperLiquidRestClient GetClient(ILoggerFactory loggerFactory)
        {
            var key = Environment.GetEnvironmentVariable("APIKEY");
            var sec = Environment.GetEnvironmentVariable("APISECRET");

            Authenticated = key != null && sec != null;
            return new HyperLiquidRestClient(null, loggerFactory, Options.Create(new HyperLiquidRestOptions
            {
                AutoTimestamp = false,
                OutputOriginalData = true,
                ApiCredentials = Authenticated ? new CryptoExchange.Net.Authentication.ApiCredentials(key, sec) : null
            }));
        }

        [Test]
        public async Task TestErrorResponseParsing()
        {
            if (!ShouldRun())
                return;

            var result = await CreateClient().SpotApi.ExchangeData.GetOrderBookAsync("TSTTST", default);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(500));
        }

        [Test]
        public async Task TestSpotAccount()
        {
            await RunAndCheckResult(client => client.SpotApi.Account.GetBalancesAsync(default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Account.GetAccountLedgerAsync(DateTime.UtcNow.AddDays(-30), default, default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Account.GetRateLimitsAsync(default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Account.GetApprovedBuilderFeeAsync(default, default, default), false);
        }

        [Test]
        public async Task TestSpotExchangeData()
        {
            await RunAndCheckResult(client => client.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync(default), false);
            await RunAndCheckResult(client => client.SpotApi.ExchangeData.GetExchangeInfoAsync(default), false);
            await RunAndCheckResult(client => client.SpotApi.ExchangeData.GetAssetInfoAsync("0x6d1e7cde53ba9467b783cb7c530ce054", default), false);
            await RunAndCheckResult(client => client.SpotApi.ExchangeData.GetPricesAsync(default), false);
            await RunAndCheckResult(client => client.SpotApi.ExchangeData.GetOrderBookAsync("HYPE/USDC", default, default, default), false);
            await RunAndCheckResult(client => client.SpotApi.ExchangeData.GetKlinesAsync("HYPE/USDC", Enums.KlineInterval.OneDay, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, default), false);
        }

        [Test]
        public async Task TestSpotTrading()
        {
            await RunAndCheckResult(client => client.SpotApi.Trading.GetOpenOrdersAsync(default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Trading.GetOpenOrdersExtendedAsync(default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Trading.GetUserTradesAsync(default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Trading.GetUserTradesByTimeAsync(DateTime.UtcNow.AddDays(-30), default, default, default, default), false);
            await RunAndCheckResult(client => client.SpotApi.Trading.GetOrderHistoryAsync(default, default), false);
        }

        [Test]
        public async Task TestFuturesAccount()
        {
            await RunAndCheckResult(client => client.FuturesApi.Account.GetAccountInfoAsync(default, default), false);
            await RunAndCheckResult(client => client.FuturesApi.Account.GetFundingHistoryAsync(DateTime.UtcNow.AddDays(-10), default, default, default), false);
        }

        [Test]
        public async Task TestFuturesExchangeData()
        {
            await RunAndCheckResult(client => client.FuturesApi.ExchangeData.GetExchangeInfoAsync(default), false);
            await RunAndCheckResult(client => client.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync(default), false);
            await RunAndCheckResult(client => client.FuturesApi.ExchangeData.GetFundingRateHistoryAsync("ETH", DateTime.UtcNow.AddDays(-3), default, default), false);
            await RunAndCheckResult(client => client.FuturesApi.ExchangeData.GetSymbolsAtMaxOpenInterestAsync(default), false);
        }

        [Test]
        public Task TestFuturesTrading()
        {
            // All already tested by Spot calls
            return Task.CompletedTask;
        }
    }
}
