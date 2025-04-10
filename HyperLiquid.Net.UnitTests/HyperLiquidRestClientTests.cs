using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using HyperLiquid.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Converters.SystemTextJson;
using Nethereum.Signer;
using System.Net.Sockets;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.Crypto;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture()]
    public class HyperLiquidRestClientTests
    {
        [Test]
        public void CheckNonDeterministSignatureExample1()
        {
            const string hash = "FACE28327892D757909E0DB4B499EC67D51AD9127BFBC53B96CCB173155D7B94";
            const string secret = "0xbb";
            var authProvider = new HyperLiquidAuthenticationProvider(new ApiCredentials("0xaa", secret));
            var client = (RestApiClient)new HyperLiquidRestClient().SpotApi;

            CryptoExchange.Net.Testing.TestHelpers.CheckSignature(
                client,
                authProvider,
                HttpMethod.Post,
                "/exchange",
                (uriParams, bodyParams, headers) =>
                {
                    var signature = (Dictionary<string, object>)bodyParams["signature"];
                    var privateKey = new ECKey(secret.HexToByteArray(), true);
                    var r = signature["r"].ToString();
                    var s = signature["s"].ToString();
                    if (!r.StartsWith("0x"))
                    {
                        return $"bad start for r :{r}";
                    }
                    if (!s.StartsWith("0x"))
                    {
                        return $"bad start for s :{s}";
                    }
                    if (!int.TryParse(signature["v"].ToString(), out var v))
                    {
                        return $"v is not an int : {signature["v"]}";
                    }
                    ECDSASignature eCDSASignature = new ECDSASignature(
                            new Org.BouncyCastle.Math.BigInteger(r.Substring(2), 16),
                            new Org.BouncyCastle.Math.BigInteger(s.Substring(2), 16)
                            );
                    var binaryHash = hash.HexToByteArray();
                    var verifyResult = privateKey.Verify(binaryHash, eCDSASignature);
                    if (!verifyResult)
                    {
                        return "Bad Signature";
                    }
                    var recId = ECKey.RecoverFromSignature(eCDSASignature, binaryHash, true, privateKey.GetPubKey(false));
                    if (recId + 27 != v)
                    {
                        return $"Bad v. Expecting {recId + 27}, got {v}";
                    }
                    return "Good";
                },
                "Good",
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
