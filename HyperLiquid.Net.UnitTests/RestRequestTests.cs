using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperLiquid.Net.Clients;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture]
    public class RestRequestTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateSpotExchangeDataCalls(bool useUpdatedDeserialization)
        {
            var client = new HyperLiquidRestClient(opts =>
            {
                opts.AutoTimestamp = false;
                opts.UseUpdatedDeserialization = useUpdatedDeserialization;
                opts.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("123", "456");
            });
            var tester = new RestRequestValidator<HyperLiquidRestClient>(client, "Endpoints/Spot/ExchangeData", "https://api.hyperliquid.xyz", IsAuthenticated);
            await tester.ValidateAsync(client => client.SpotApi.ExchangeData.GetPricesAsync(), "GetPrices");
            await tester.ValidateAsync(client => client.SpotApi.ExchangeData.GetOrderBookAsync("UnitTest"), "GetOrderBook");
            await tester.ValidateAsync(client => client.SpotApi.ExchangeData.GetKlinesAsync("UnitTest", Enums.KlineInterval.OneDay, DateTime.UtcNow, DateTime.UtcNow), "GetKlines");
            await tester.ValidateAsync(client => client.SpotApi.ExchangeData.GetExchangeInfoAsync(), "GetExchangeInfo");
            await tester.ValidateAsync(client => client.SpotApi.ExchangeData.GetExchangeInfoAndTickersAsync(), "GetExchangeInfoAndTickers");
            await tester.ValidateAsync(client => client.SpotApi.ExchangeData.GetAssetInfoAsync("123"), "GetAssetInfo");
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateSpotAccountCalls(bool useUpdatedDeserialization)
        {
            var client = new HyperLiquidRestClient(opts =>
            {
                opts.AutoTimestamp = false;
                opts.UseUpdatedDeserialization = useUpdatedDeserialization;
                opts.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("123", "456");
            });
            var tester = new RestRequestValidator<HyperLiquidRestClient>(client, "Endpoints/Spot/Account", "https://api.hyperliquid.xyz", IsAuthenticated);
            await tester.ValidateAsync(client => client.SpotApi.Account.GetBalancesAsync(), "GetBalances", nestedJsonProperty: "balances");
            await tester.ValidateAsync(client => client.SpotApi.Account.GetAccountLedgerAsync(DateTime.UtcNow), "GetAccountLedger");
            await tester.ValidateAsync(client => client.SpotApi.Account.GetRateLimitsAsync(), "GetRateLimits");
            await tester.ValidateAsync(client => client.SpotApi.Account.GetApprovedBuilderFeeAsync(), "GetApprovedBuilderFee");
            await tester.ValidateAsync(client => client.SpotApi.Account.GetSubAccountsAsync(), "GetSubAccounts");
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateTradingCalls(bool useUpdatedDeserialization)
        {
            var client = new HyperLiquidRestClient(opts =>
            {
                opts.AutoTimestamp = false;
                opts.UseUpdatedDeserialization = useUpdatedDeserialization;
                opts.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("123", "456");
            });
            var tester = new RestRequestValidator<HyperLiquidRestClient>(client, "Endpoints/Spot/Trading", "https://api.hyperliquid.xyz", IsAuthenticated);
            await tester.ValidateAsync(client => client.SpotApi.Trading.GetOpenOrdersAsync(), "GetOpenOrders");
            await tester.ValidateAsync(client => client.SpotApi.Trading.GetOpenOrdersExtendedAsync(), "GetOpenOrdersExtended", ignoreProperties: ["children"]);
            await tester.ValidateAsync(client => client.SpotApi.Trading.GetUserTradesAsync(), "GetUserTrades");
            await tester.ValidateAsync(client => client.SpotApi.Trading.PlaceOrderAsync("UnitTest", Enums.OrderSide.Buy, Enums.OrderType.Market, 1, 1), "PlaceOrder",  skipResponseValidation: true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateFuturesExchangeDataCalls(bool useUpdatedDeserialization)
        {
            var client = new HyperLiquidRestClient(opts =>
            {
                opts.AutoTimestamp = false;
                opts.UseUpdatedDeserialization = useUpdatedDeserialization;
                opts.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("123", "456");
            });
            var tester = new RestRequestValidator<HyperLiquidRestClient>(client, "Endpoints/Futures/ExchangeData", "https://api.hyperliquid.xyz", IsAuthenticated);
            await tester.ValidateAsync(client => client.FuturesApi.ExchangeData.GetExchangeInfoAsync(), "GetExchangeInfo", nestedJsonProperty: "universe");
            await tester.ValidateAsync(client => client.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync(), "GetExchangeInfoAndTickers");
            await tester.ValidateAsync(client => client.FuturesApi.ExchangeData.GetFundingRateHistoryAsync("ETH", DateTime.UtcNow), "GetFundingRateHistory");
            await tester.ValidateAsync(client => client.FuturesApi.ExchangeData.GetSymbolsAtMaxOpenInterestAsync(), "GetSymbolsAtMaxOpenInterest");
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateFuturesAccountCalls(bool useUpdatedDeserialization)
        {
            var client = new HyperLiquidRestClient(opts =>
            {
                opts.AutoTimestamp = false;
                opts.UseUpdatedDeserialization = useUpdatedDeserialization;
                opts.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("123", "456");
            });
            var tester = new RestRequestValidator<HyperLiquidRestClient>(client, "Endpoints/Futures/Account", "https://api.hyperliquid.xyz", IsAuthenticated);
            await tester.ValidateAsync(client => client.FuturesApi.Account.GetFundingHistoryAsync(DateTime.UtcNow), "GetFundingHistory", ignoreProperties: ["type", "nSamples"]);
            await tester.ValidateAsync(client => client.FuturesApi.Account.GetAccountInfoAsync(), "GetAccountInfo", ignoreProperties: []);
            
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateFuturesTradingCalls(bool useUpdatedDeserialization)
        {
            var client = new HyperLiquidRestClient(opts =>
            {
                opts.AutoTimestamp = false;
                opts.UseUpdatedDeserialization = useUpdatedDeserialization;
                opts.ApiCredentials = new CryptoExchange.Net.Authentication.ApiCredentials("123", "456");
            });
            var tester = new RestRequestValidator<HyperLiquidRestClient>(client, "Endpoints/Futures/Trading", "https://api.hyperliquid.xyz", IsAuthenticated);
            await tester.ValidateAsync(client => client.FuturesApi.Trading.SetLeverageAsync("UnitTest", 1, Enums.MarginType.Isolated), "SetLeverage");

        }

        private bool IsAuthenticated(WebCallResult result)
        {
            return result.RequestBody?.Contains("signature") == true;
        }
    }
}
