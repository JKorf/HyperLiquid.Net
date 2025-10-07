using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Signing;
using HyperLiquid.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;

namespace HyperLiquid.Net
{
    internal class HyperLiquidAuthenticationProvider : AuthenticationProvider
    {
        private static readonly Dictionary<Type, string> _typeMapping = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(long), "uint64" },
            { typeof(bool), "bool" },
        };

        private static readonly List<string[]> _eip721Domain = new List<string[]>
        {
            new string[] { "name", "string" },
            new string[] { "version", "string" },
            new string[] { "chainId", "uint256" },
            new string[] { "verifyingContract", "address" },
            new string[] { "salt", "bytes32" }
        };

        private static readonly Dictionary<string, object> _domain = new Dictionary<string, object>()
        {
            { "chainId", 1337 },
            { "name", "Exchange" },
            { "verifyingContract", "0x0000000000000000000000000000000000000000" },
            { "version", "1" },
        };

        private static readonly Dictionary<string, object> _messageTypes = new Dictionary<string, object>()
        {
            { "Agent",
                new List<object>()
                {
                    new Dictionary<string, object>()
                    {
                        { "name", "source" },
                        { "type", "string" },
                    },
                    new Dictionary<string, object>() {
                        { "name", "connectionId" },
                        { "type", "bytes32" },
                    }
                }
            }
        };

        public HyperLiquidAuthenticationProvider(ApiCredentials credentials) : base(credentials)
        {
        }

        public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration request)
        {
            if (!request.Authenticated)
                return;

            var action = (Dictionary<string, object>)request.BodyParameters["action"];
            var nonce = action.TryGetValue("time", out var time) ? (long)time : action.TryGetValue("nonce", out var n) ? (long)n : GetMillisecondTimestampLong(apiClient);
            request.BodyParameters!.Add("nonce", nonce);
            if (action.TryGetValue("signatureChainId", out var chainId))
            {
                // User action
                var actionName = (string)action["type"];
                if (actionName == "withdraw3")
                    actionName = "withdraw";

                var types = GetSignatureTypes(actionName.Substring(0, 1).ToUpperInvariant() + actionName.Substring(1), action);
                var userActions = new Dictionary<string, object>()
                {
                    { "name", "HyperliquidSignTransaction" },
                    { "version", "1" },
                    { "chainId",  Convert.ToInt32((string)chainId, 16) },
                    { "verifyingContract", "0x0000000000000000000000000000000000000000" }
                };

                var msg = EncodeEip721(userActions, types, action);
                var keccakSigned = BytesToHexString(SignKeccak(msg));

                Dictionary<string, object> signature;
                if (HyperLiquidExchange.SignRequestDelegate != null)
                    signature = HyperLiquidExchange.SignRequestDelegate(keccakSigned, _credentials.Secret);
                else
                    signature = SignRequest(keccakSigned, _credentials.Secret);

                request.BodyParameters["signature"] = signature;
            }
            else
            {
                // Exchange action
                string? vaultAddress = null;
                if (request.BodyParameters.TryGetValue("vaultAddress", out var vaultAddressObj))
                {
                    vaultAddress = (string)vaultAddressObj;
                    vaultAddress = vaultAddress.StartsWith("0x") ? vaultAddress.Substring(2) : vaultAddress;
                }

                long? expiresAfter = null;
                if (request.BodyParameters.TryGetValue("expiresAfter", out var expiresAfterObj))
                    expiresAfter = (long)expiresAfterObj;

                var hash = GenerateActionHash(action, nonce, vaultAddress, expiresAfter);
                var phantomAgent = new Dictionary<string, object>()
                {
                    { "source", ((HyperLiquidRestClientApi)apiClient).ClientOptions.Environment.Name == TradeEnvironmentNames.Testnet ? "b" : "a" },
                    { "connectionId", hash },
                };

                var msg = EncodeEip721(_domain, _messageTypes, phantomAgent);
                var keccakSigned = BytesToHexString(SignKeccak(msg));

                Dictionary<string, object> signature;
                if (HyperLiquidExchange.SignRequestDelegate != null)
                    signature = HyperLiquidExchange.SignRequestDelegate(keccakSigned, _credentials.Secret);
                else
                    signature = SignRequest(keccakSigned, _credentials.Secret);

                request.BodyParameters["signature"] = signature;
            }
        }


        private Dictionary<string, object> GetSignatureTypes(string name, Dictionary<string, object> parameters)
        {
            var props = new List<object>();
            var result = new Dictionary<string, object>()
            {
                { "HyperliquidTransaction:" + name, props }
            };

            foreach (var item in parameters.Where(x => x.Key != "type" && x.Key != "signatureChainId"))
            {
                props.Add(new Dictionary<string, object>
                {
                    { "name", item.Key },
                    { "type", item.Key == "builder" ? "address" : _typeMapping[item.Value.GetType()] }
                });
            }

            return result;
        }

        public static Dictionary<string, object> SignRequest(string request, string secret)
        {
            var messageBytes = ConvertHexStringToByteArray(request);
            var bSecret = secret.HexToByteArray();

            ECParameters eCParameters = new ECParameters()
            {
                D = FixSize(bSecret, 32),
                Curve = ECCurve.CreateFromFriendlyName("secp256k1"),
                Q =
                {
                    X = null,
                    Y = null
                }
            };

            using (ECDsa dsa = ECDsa.Create(eCParameters))
            {
                var s = dsa.SignHash(messageBytes);
                var rs = NormalizeSignature(s);
                var parameters = dsa.ExportParameters(false);

                var c = new byte[33];

                rs.r.Reverse().ToArray().CopyTo(c, 0);
                BigInteger rValue = new BigInteger(c);
                c = new byte[33];
                rs.s.Reverse().ToArray().CopyTo(c, 0);
                BigInteger sValue = new BigInteger(c);

                var v = RecoverFromSignature(rValue, sValue, messageBytes, parameters.Q.X!, parameters.Q.Y!);

                return new Dictionary<string, object>()
                {
                    { "r", "0x" + BytesToHexString(rs.r).ToLowerInvariant() },
                    { "s", "0x" + BytesToHexString(rs.s).ToLowerInvariant() },
                    { "v", 27 + v}
                };
            }
        }

        private static byte[] FixSize(byte[] input, int expectedSize)
        {
            if (input.Length == expectedSize)
                return input;

            byte[] tmp;
            if (input.Length < expectedSize)
            {
                tmp = new byte[expectedSize];
                Buffer.BlockCopy(input, 0, tmp, expectedSize - input.Length, input.Length);
                return tmp;
            }

            if (input.Length > expectedSize + 1 || input[0] != 0)
                throw new InvalidOperationException();

            tmp = new byte[expectedSize];
            Buffer.BlockCopy(input, 1, tmp, 0, expectedSize);
            return tmp;
        }

        public static (byte[] r, byte[] s, bool flip) NormalizeSignature(byte[] signature)
        {
            // Ensure the signature is in the correct format (r, s)
            if (signature.Length != 64)
                throw new ArgumentException("Invalid signature length.");

            byte[] r = new byte[32];
            byte[] s = new byte[32];
            Array.Copy(signature, 0, r, 0, 32);
            Array.Copy(signature, 32, s, 0, 32);

            // Normalize the 's' value to be in the lower half of the curve order
            byte[] c = new byte[33];
            s.Reverse().ToArray().CopyTo(c, 0);
            BigInteger sValue = new BigInteger(c);
            byte[] normalizedS;
            var flip = false;
            if (sValue > Secp256k1PointCalculator._halfN)
            {
                sValue = Secp256k1PointCalculator._n - sValue;
                flip = true;
                normalizedS = sValue.ToByteArray().Reverse().ToArray();
                if (normalizedS.Length < 32)
                {
                    byte[] paddedS = new byte[32];
                    Array.Copy(normalizedS, 0, paddedS, 32 - normalizedS.Length, normalizedS.Length);
                    normalizedS = paddedS;
                }
            }
            else
            {
                normalizedS = s;
            }

            return (r, normalizedS, flip);
        }
 
        private static int RecoverFromSignature(BigInteger r, BigInteger s, byte[] message, byte[] publicKeyX, byte[] publicKeyY)
        {
            if (r < 0)
                throw new ArgumentException("r should be positive");
            if (s < 0)
                throw new ArgumentException("s should be positive");
            if (message == null)
                throw new ArgumentNullException("message");

            byte[] c = new byte[33];
            publicKeyX.Reverse().ToArray().CopyTo(c, 0);
            BigInteger publicKeyXValue = new BigInteger(c);

            c = new byte[33];
            publicKeyY.Reverse().ToArray().CopyTo(c, 0);
            BigInteger publicKeyYValue = new BigInteger(c);

            // Compute e from M using Steps 2 and 3 of ECDSA signature verification.
            c = new byte[33];
            message.Reverse().ToArray().CopyTo(c, 0);
            var e = new BigInteger(c);
            
            var eInv = (-e) % Secp256k1PointCalculator._n;
            if (eInv < 0)            
                eInv += Secp256k1PointCalculator._n;            

            var rInv = BigInteger.ModPow(r, Secp256k1PointCalculator._n - 2, Secp256k1PointCalculator._n);
            var srInv = (rInv * s) % Secp256k1PointCalculator._n;
            var eInvrInv = (rInv * eInv) % Secp256k1PointCalculator._n;

            var recId = -1;

            for (var i = 0; i < 4; i++)
            {
                recId = i;
                var intAdd = recId / 2;
                var x = r + (intAdd * Secp256k1PointCalculator._n);

                if (x < Secp256k1ZCalculator._q)
                {
                    var R = Secp256k1PointCalculator.DecompressPointSecp256k1(x, (recId & 1));
                    var tx = R.X.ToString("x");
                    var ty = R.Y.ToString("x");
                    var b = tx == ty;
                    if (R.MultiplyByN().IsInfinity())
                    {
                        var q = Secp256k1PointCalculator.SumOfTwoMultiplies(new Secp256k1PointPreCompCache(), Secp256k1PointCalculator._g, eInvrInv, R, srInv);
                        q = q.Normalize();
                        if (q.X == publicKeyXValue && q.Y == publicKeyYValue)
                        {
                            recId = i;
                            break;
                        }
                    }
                }
            }

            if (recId == -1)
                throw new Exception("Could not construct a recoverable key. This should never happen.");

            return recId;
        }

        public byte[] EncodeEip721(
            IEnumerable<KeyValuePair<string, object>> domain,
            IEnumerable<KeyValuePair<string, object>> messageTypes,
            IEnumerable<KeyValuePair<string, object>> messageData)
        {
            var domainValues = domain.Select(x => x.Value).ToArray();

            var typeRaw = new Signing.TypedDataRaw();
            var types = new Dictionary<string, Signing.MemberDescription[]>();

            // fill in domain types
            var domainTypesDescription = new List<Signing.MemberDescription>();
            var domainValuesArray = new List<Signing.MemberValue>();

            foreach (var d in _eip721Domain)
            {
                var key = d[0];
                var type = d[1];
                for (var i = 0; i < domain.Count(); i++)
                {
                    if (string.Equals(key, domain.Select(x => x.Key).ElementAt(i)))
                    {
                        var memberDescription = new Signing.MemberDescription
                        {
                            Name = key,
                            Type = type
                        };
                        domainTypesDescription.Add(memberDescription);

                        var memberValue = new Signing.MemberValue
                        {
                            TypeName = type,
                            Value = domainValues[i]
                        };
                        domainValuesArray.Add(memberValue);
                    }
                }
            }

            types["EIP712Domain"] = domainTypesDescription.ToArray();
            typeRaw.DomainRawValues = domainValuesArray.ToArray();

            // fill in message types
            var messageTypesDict = new Dictionary<string, string>();
            var typeName = messageTypes.Select(x => x.Key).First();
            var messageTypesContent = (IList<object>)messageTypes.Single(x => x.Key == typeName).Value;
            var messageTypesDescription = new List<Signing.MemberDescription> { };
            for (var i = 0; i < messageTypesContent.Count; i++)
            {
                var elem = (IDictionary<string, object>)messageTypesContent[i];
                var name = (string)elem["name"];
                var type = (string)elem["type"];
                messageTypesDict[name] = type;
                var member = new Signing.MemberDescription
                {
                    Name = name,
                    Type = type
                };
                messageTypesDescription.Add(member);
            }
            types[typeName] = messageTypesDescription.ToArray();

            // fill in message values
            var messageValues = new List<Signing.MemberValue> { };
            for (var i = 0; i < messageData.Count(); i++)
            {
                var kvp = messageData.ElementAt(i);
                if (messageTypesDict.TryGetValue(kvp.Key, out var msgVal))
                {
                    var member = new Signing.MemberValue
                    {
                        TypeName = msgVal,
                        Value = kvp.Value
                    };
                    messageValues.Add(member);
                }
            }

            typeRaw.Message = messageValues.ToArray();
            typeRaw.Types = types;
            typeRaw.PrimaryType = typeName;
            return LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
        }


        private byte[] GenerateActionHash(object action, long nonce, string? vaultAddress, long? expireAfter)
        {
            var packer = new PackConverter();
            var dataHex = BytesToHexString(packer.Pack(action));
            var nonceHex = nonce.ToString("x");
            var signHex = dataHex + "00000" + nonceHex;
            if (vaultAddress == null)
                signHex += "00";
            else
                signHex += "01" + vaultAddress;

            if (expireAfter != null)
                signHex += "00" + $"00000{(ulong)expireAfter:x}";

            var signBytes = ConvertHexStringToByteArray(signHex);
            return SignKeccak(signBytes);
        }

        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.StartsWith("0x"))
                hexString = hexString.Substring(2);

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string hexSubstring = hexString.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(hexSubstring, 16);
            }

            return bytes;
        }

        private static byte[] SignKeccak(byte[] data)
        {
            return InternalSha3Keccack.CalculateHash(data);
        }


        #region Nethereum signing method, here to use when we need to debug message signing issues

        // In Authenticate request method:
        //var msg = EncodeEip721Neth(typedData, Convert.ToInt32((string)chainId, 16), "HyperliquidTransaction:UsdClassTransfer");
        //var typedData = new UsdClassTransfer
        //{
        //    Amount = (string)action["amount"],
        //    HyperLiquidChain = (string)action["hyperliquidChain"],
        //    Nonce = (long)action["nonce"],
        //    ToPerp = (bool)action["toPerp"]
        //};

        //public byte[] EncodeEip721Neth(
        //    object msg,
        //    int chainId,
        //    string primaryType)
        //{
        //    var typeDef = GetMessageTypedDefinition(chainId, msg.GetType(), primaryType);

        //    var signer = new Eip712TypedDataSigner();
        //    var encodedData = signer.EncodeTypedData((UsdClassTransfer)msg, typeDef);
        //    return encodedData;
        //}

        //public static TypedData<Domain> GetMessageTypedDefinition(int chainId, Type messageType, string primaryType)
        //{
        //    return new TypedData<Domain>
        //    {
        //        Domain = new Domain
        //        {
        //            Name = "HyperliquidSignTransaction",
        //            Version = "1",
        //            ChainId = chainId,
        //            VerifyingContract = "0x0000000000000000000000000000000000000000",
        //        },
        //        Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), messageType),
        //        PrimaryType = primaryType,
        //    };
        //}
        #endregion
    }

    //[Struct("HyperliquidTransaction:UsdClassTransfer")]
    //public class UsdClassTransfer
    //{
    //    [Parameter("string", "hyperliquidChain", 1)]
    //    public string HyperLiquidChain { get; set; }
    //    [Parameter("string", "amount", 2)]
    //    public string Amount { get; set; }
    //    [Parameter("bool", "toPerp", 3)]
    //    public bool ToPerp { get; set; }
    //    [Parameter("uint64", "nonce", 4)]
    //    public long Nonce { get; set; }
    //}
}
