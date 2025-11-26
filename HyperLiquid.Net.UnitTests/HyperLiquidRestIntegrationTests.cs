using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.SymbolOrderBooks;
using CryptoExchange.Net.Objects.Errors;

namespace HyperLiquid.Net.UnitTests
{
    [NonParallelizable]
    public class HyperLiquidRestIntegrationTests : RestIntegrationTest<HyperLiquidRestClient>
    {
        public override bool Run { get; set; } = true;

        public override HyperLiquidRestClient GetClient(ILoggerFactory loggerFactory, bool useUpdatedDeserialization)
        {
            var key = Environment.GetEnvironmentVariable("APIKEY");
            var sec = Environment.GetEnvironmentVariable("APISECRET");

            Authenticated = key != null && sec != null;
            return new HyperLiquidRestClient(null, loggerFactory, Options.Create(new HyperLiquidRestOptions
            {
                AutoTimestamp = false,
                UseUpdatedDeserialization = useUpdatedDeserialization,
                OutputOriginalData = true,
                ApiCredentials = Authenticated ? new CryptoExchange.Net.Authentication.ApiCredentials(key, sec) : null
            }));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestErrorResponseParsing(bool useUpdatedDeserialization)
        {
            if (!ShouldRun())
                return;

            var result = await CreateClient(useUpdatedDeserialization).SpotApi.ExchangeData.GetOrderBookAsync("TSTTST", -1);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.ErrorType, Is.EqualTo(ErrorType.UnknownSymbol));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestSpotAccount(bool useUpdatedDeserialization)
        {
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Account.GetBalancesAsync(default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Account.GetAccountLedgerAsync(DateTime.UtcNow.AddDays(-30), default, default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Account.GetRateLimitsAsync(default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Account.GetApprovedBuilderFeeAsync(default, default, default), true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestSpotExchangeData(bool useUpdatedDeserialization)
        {
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync(default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.ExchangeData.GetExchangeInfoAsync(default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.ExchangeData.GetAssetInfoAsync("0x6d1e7cde53ba9467b783cb7c530ce054", default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.ExchangeData.GetPricesAsync(default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.ExchangeData.GetOrderBookAsync("HYPE/USDC", default, default, default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.ExchangeData.GetKlinesAsync("HYPE/USDC", Enums.KlineInterval.OneDay, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, default), false);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestSpotTrading(bool useUpdatedDeserialization)
        {
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Trading.GetOpenOrdersAsync(default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Trading.GetOpenOrdersExtendedAsync(default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Trading.GetUserTradesAsync(default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Trading.GetUserTradesByTimeAsync(DateTime.UtcNow.AddDays(-30), default, default, default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.SpotApi.Trading.GetOrderHistoryAsync(default, default), true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestFuturesAccount(bool useUpdatedDeserialization)
        {
            await RunAndCheckResult(useUpdatedDeserialization, client => client.FuturesApi.Account.GetAccountInfoAsync(default, default), true);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.FuturesApi.Account.GetFundingHistoryAsync(DateTime.UtcNow.AddDays(-10), default, default, default), true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task TestFuturesExchangeData(bool useUpdatedDeserialization)
        {
            await RunAndCheckResult(useUpdatedDeserialization, client => client.FuturesApi.ExchangeData.GetExchangeInfoAsync(default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync(default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.FuturesApi.ExchangeData.GetFundingRateHistoryAsync("ETH", DateTime.UtcNow.AddDays(-3), default, default), false);
            await RunAndCheckResult(useUpdatedDeserialization, client => client.FuturesApi.ExchangeData.GetSymbolsAtMaxOpenInterestAsync(default), false);
        }

        [Test]
        public Task TestFuturesTrading()
        {
            // All already tested by Spot calls
            return Task.CompletedTask;
        }

        [Test]
        public async Task TestOrderBooks()
        {
            await TestOrderBook(new HyperLiquidSymbolOrderBook("HYPE/USDC"));
        }
    }
}
