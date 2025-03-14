using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using HyperLiquid.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Converters.SystemTextJson;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture()]
    public class HyperLiquidRestClientTests
    {
        [Test]
        public void CheckSignatureExample1()
        {
            var authProvider = new HyperLiquidAuthenticationProvider(new ApiCredentials("0xaa", "0xbb"));
            var client = (RestApiClient)new HyperLiquidRestClient().SpotApi;

            CryptoExchange.Net.Testing.TestHelpers.CheckSignature(
                client,
                authProvider,
                HttpMethod.Post,
                "/exchange",
                (uriParams, bodyParams, headers) =>
                {
                    var signature = (Dictionary<string, object>)bodyParams["signature"];

                    return signature["r"].ToString() + "-" + signature["s"].ToString();
                },
                "0x9b6ce90642ed560d93419b17c8b02f6b953b773c5d53c8d6d0d1f12b05e18d01-0x76044596dd1032e3663dc6b198e66f328aaeae11ac11504770682de3f414c861",
                new Dictionary<string, object>
                {
                    { "action", new ParameterCollection
                        {
                            { "type", "order" },
                            { "orders", new [] {
                                new Dictionary<string, object>{
                                    { "symbol", 105 },
                                    { "a", "123" }
                                }

                            } }
                        }
                    },
                },
                DateTimeConverter.ParseFromDouble(1499827319559),
                true,
                false);
        }

        [Test]
        public void CheckInterfaces()
        {
            CryptoExchange.Net.Testing.TestHelpers.CheckForMissingRestInterfaces<HyperLiquidRestClient>(["IHyperLiquidRestClientAccount", "IHyperLiquidRestClientExchangeData", "IHyperLiquidRestClientTrading"]);
            CryptoExchange.Net.Testing.TestHelpers.CheckForMissingSocketInterfaces<HyperLiquidSocketClient>(["IHyperLiquidSocketClientApi"]);
        }
    }
}
