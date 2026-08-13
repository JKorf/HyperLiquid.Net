using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture]
    internal class SharedRestIntegrationTests
    {
        private bool ManualRun { get; } = false;
        private static SharedSymbol _spotSymbol = new SharedSymbol(TradingMode.Spot, "ETH", "USDC");
        private static SharedSymbol _futuresSymbol = new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDC");



        private bool ShouldRun()
        {
            var integrationTests = Environment.GetEnvironmentVariable("INTEGRATION");
            if (!ManualRun && integrationTests != "1")
                return false;

            return true;
        }

        private IHyperLiquidRestClientSpotApiShared GetSpotRestClient()
        {
            var collection = new ServiceCollection();
            collection.AddHyperLiquid(x => x.Rest.OutputOriginalData = true);
            collection.AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Trace);
                x.AddProvider(new TraceLoggerProvider());
            });
            var sp = collection.BuildServiceProvider();
            return sp.GetRequiredService<IHyperLiquidRestClient>().SpotApi.SharedClient;
        }

        private IHyperLiquidRestClientFuturesApiShared GetFuturesRestClient()
        {
            var collection = new ServiceCollection();
            collection.AddHyperLiquid(x => x.Rest.OutputOriginalData = true);
            collection.AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Trace);
                x.AddProvider(new TraceLoggerProvider());
            });
            var sp = collection.BuildServiceProvider();
            return sp.GetRequiredService<IHyperLiquidRestClient>().FuturesApi.SharedClient;
        }

        [Test]
        public async Task TestFuturesKlinesRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetKlinesAsync(new GetKlinesRequest(_futuresSymbol, SharedKlineInterval.OneDay));
            var result3 = await client.GetKlinesAsync(new GetKlinesRequest(_futuresSymbol, SharedKlineInterval.OneDay, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow));
            CheckResults([
                ("FuturesKlines", result1),
                ("FuturesKlinesTimed", result3),
                ]);
        }

        [Test]
        public async Task TestSpotKlinesRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetSpotRestClient();
            var result1 = await client.GetKlinesAsync(new GetKlinesRequest(_spotSymbol, SharedKlineInterval.OneDay));
            var result3 = await client.GetKlinesAsync(new GetKlinesRequest(_spotSymbol, SharedKlineInterval.OneDay, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow));
            CheckResults([
                ("SpotKlines", result1),
                ("SpotKlinesTimed", result3),
                ]);
        }

        [Test]
        public async Task TestFuturesBookTickersRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetBookTickerAsync(new GetBookTickerRequest(_futuresSymbol));
            CheckResults("FuturesBookTicker", result1);
        }

        [Test]
        public async Task TestSpotBookTickersRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetSpotRestClient();
            var result1 = await client.GetBookTickerAsync(new GetBookTickerRequest(_spotSymbol));
            CheckResults("SpotBookTicker", result1);
        }

        [Test]
        public async Task TestFuturesOrderBookRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetOrderBookAsync(new GetOrderBookRequest(_futuresSymbol));
            CheckResults("FuturesOrderBook", result1);
        }

        [Test]
        public async Task TestSpotOrderBookRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetSpotRestClient();
            var result1 = await client.GetOrderBookAsync(new GetOrderBookRequest(_spotSymbol));
            CheckResults("SpotOrderBook", result1);
        }

        [Test]
        public async Task TestFuturesTickerRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetFuturesTickerAsync(new GetTickerRequest(_futuresSymbol));
            var result2 = await client.GetFuturesTickersAsync(new GetTickersRequest());
            CheckResults([
                ("FuturesTicker", result1),
                ("FuturesTickers", result2)
                ]);
        }

        [Test]
        public async Task TestSpotTickerRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetSpotRestClient();
            var result1 = await client.GetSpotTickerAsync(new GetTickerRequest(_spotSymbol));
            var result2 = await client.GetSpotTickersAsync(new GetTickersRequest());
            CheckResults([
                ("SpotTicker", result1),
                ("SpotTickers", result2)
                ]);
        }

        [Test]
        public async Task TestFuturesSymbolRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetFuturesSymbolsAsync(new GetSymbolsRequest());
            CheckResults([
                ("FuturesSymbols", result1)
                ]);
        }

        [Test]
        public async Task TestSpotSymbolRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetSpotRestClient();
            var result1 = await client.GetSpotSymbolsAsync(new GetSymbolsRequest());
            CheckResults([
                ("SpotSymbols", result1)
                ]);
        }

        [Test]
        public async Task TestAssetsRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetSpotRestClient();
            var result1 = await client.GetAssetAsync(new GetAssetRequest("ETH"));
            var result2 = await client.GetAssetsAsync(new GetAssetsRequest());
            CheckResults([
                ("SpotAsset", result1),
                ("SpotAssets", result2)
                ]);
        }

        [Test]
        public async Task TestFundingRateRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetFundingRateHistoryAsync(new GetFundingRateHistoryRequest(_futuresSymbol));
            var result2 = await client.GetFundingRateHistoryAsync(new GetFundingRateHistoryRequest(_futuresSymbol, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow));
            CheckResults([
                ("FuturesFundingRateHistory", result1),
                ("FuturesFundingRateHistory", result2)
                ]);
        }

        [Test]
        public async Task TestOpenInterestRequests()
        {
            if (!ShouldRun())
                return;

            var client = GetFuturesRestClient();
            var result1 = await client.GetOpenInterestAsync(new GetOpenInterestRequest(_futuresSymbol));
            CheckResults([
                ("FuturesOpenInterest", result1)
                ]);
        }

        private void CheckResults(string name, ICallResult result)
            => CheckResults([(name, result)]);
        private void CheckResults((string, ICallResult result)[] results)
        {
            foreach(var item in results)
            {
                if (!item.result.Success)
                    throw new Exception($"Failed to get {item.Item1}: " + item.result.Error);
            }
        }

        private void CheckResults<T>(string name, ICallResult<T[]> result)
            => CheckResults([(name, result)]);
        private void CheckResults<T>((string, ICallResult<T[]> result)[] results)
        {
            foreach (var item in results)
            {
                if (!item.result.Success)
                    throw new Exception($"Failed to get {item.Item1}: " + item.result.Error);

                if (item.result.Data.Length == 0)
                    throw new Exception($"No response data for {item.Item1}");
            }
        }
    }
}
