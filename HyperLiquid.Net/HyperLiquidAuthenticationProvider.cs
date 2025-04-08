using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Utils;
using System.Security.Cryptography;
using System.Numerics;

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

        private static readonly Dictionary<string, object> _userActionDomain = new Dictionary<string, object>()
        {
            { "chainId", 2748 },
            { "name", "HyperliquidSignTransaction" },
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

        public override void AuthenticateRequest(
            RestApiClient apiClient,
            Uri uri,
            HttpMethod method,
            ref IDictionary<string, object>? uriParameters,
            ref IDictionary<string, object>? bodyParameters,
            ref Dictionary<string, string>? headers,
            bool auth,
            ArrayParametersSerialization arraySerialization,
            HttpMethodParameterPosition parameterPosition,
            RequestBodyFormat requestBodyFormat)
        {
            headers = new Dictionary<string, string>() { };

            if (!auth)
                return;

            var action = (Dictionary<string, object>)bodyParameters!["action"];
            var nonce = action.TryGetValue("time", out var time) ? (long)time : action.TryGetValue("nonce", out var n) ? (long)n : GetMillisecondTimestampLong(apiClient);
            bodyParameters!.Add("nonce", nonce);
            if (action.TryGetValue("signatureChainId", out var chainId))
            {
                // User action
                var actionName = (string)action["type"];
                if (actionName == "withdraw3")
                    actionName = "withdraw";

                var types = GetSignatureTypes(actionName.Substring(0, 1).ToUpperInvariant() + actionName.Substring(1), action);
                var msg = EncodeEip721(_userActionDomain, types, action.Where(x => x.Key != "type" && x.Key != "signatureChainId").ToDictionary(x => x.Key, x => x.Value));
                var keccakSigned = BytesToHexString(SignKeccak(msg));
                var signature = SignRequest(keccakSigned, _credentials.Secret);

                bodyParameters["signature"] = signature;
            }
            else
            {
                // Exchange action
                var hash = GenerateActionHash(action, nonce);
                var phantomAgent = new Dictionary<string, object>()
                {
                    { "source", "a" },
                    { "connectionId", hash },
                };

                var msg = EncodeEip721(_domain, _messageTypes, phantomAgent);
                var keccakSigned = BytesToHexString(SignKeccak(msg));
                var signature = SignRequest(keccakSigned, _credentials.Secret);

                bodyParameters["signature"] = signature;
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
            var t = new byte[32];
            bSecret.CopyTo(t, Math.Max(0, t.Length - bSecret.Length));
            ECParameters eCParameters = new ECParameters()
            {
                D = t,
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

        public static (byte[] r, byte[] s, bool flip) NormalizeSignature(byte[] signature)
        {
            // Ensure the signature is in the correct format (r, s)
            if (signature.Length != 64)
            {
                throw new ArgumentException("Invalid signature length.");
            }

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
            if (sValue > Secp256k1PointCalculator.HalfN)
            {
                sValue = Secp256k1PointCalculator.N - sValue;
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

            //var curve = Secp256k1;
            //var pubToHex = uncompressedPublicKey.ToHex();

            // 1.0 For j from 0 to h   (h == recId here and the loop is outside this function)
            //   1.1 Let x = r + jn
            //var n = curve.N;

            //   1.5. Compute e from M using Steps 2 and 3 of ECDSA signature verification.
            c = new byte[33];
            message.Reverse().ToArray().CopyTo(c, 0);
            var e = new BigInteger(c);
            //   1.6. For k from 1 to 2 do the following.   (loop is outside this function via iterating recId)
            //   1.6.1. Compute a candidate public key as:
            //               Q = mi(r) * (sR - eG)
            //
            // Where mi(x) is the modular multiplicative inverse. We transform this into the following:
            //               Q = (mi(r) * s ** R) + (mi(r) * -e ** G)
            // Where -e is the modular additive inverse of e, that is z such that z + e = 0 (mod n). In the above equation
            // ** is point multiplication and + is point addition (the EC group operator).
            //
            // We can find the additive inverse by subtracting e from zero then taking the mod. For example the additive
            // inverse of 3 modulo 11 is 8 because 3 + 8 mod 11 = 0, and -3 mod 11 = 8.

            var eInv = (-e) % Secp256k1PointCalculator.N;
            if (eInv < 0)
            {
                eInv += Secp256k1PointCalculator.N;
            }
            var rInv = BigInteger.ModPow(r, Secp256k1PointCalculator.N - 2, Secp256k1PointCalculator.N);
            //            var rInv = r.ModInverse(s_curveOrder);
            var srInv = (rInv * s) % Secp256k1PointCalculator.N;
            var eInvrInv = (rInv * eInv) % Secp256k1PointCalculator.N;

            var recId = -1;

            for (var i = 0; i < 4; i++)
            {
                recId = i;
                var intAdd = recId / 2;
                var x = r + (intAdd * Secp256k1PointCalculator.N);

                //   1.2. Convert the integer x to an octet string X of length mlen using the conversion routine
                //        specified in Section 2.3.7, where mlen = ⌈(log2 p)/8⌉ or mlen = ⌈m/8⌉.
                //   1.3. Convert the octet string (16 set binary digits)||X to an elliptic curve point R using the
                //        conversion routine specified in Section 2.3.4. If this conversion routine outputs “invalid”, then
                //        do another iteration of Step 1.
                //
                // More concisely, what these points mean is to use X as a compressed public key.

                //using bouncy and Q value of Point

                if (x < Secp256k1ZCalculator.Q)
                {
                    // So it's encoded in the recId.
                    var R = Secp256k1PointCalculator.DecompressPointSecp256k1(x, (recId & 1));
                    var tx = R.X.ToString("x");
                    var ty = R.Y.ToString("x");
                    var b = tx == ty;
                    //   1.4. If nR != point at infinity, then do another iteration of Step 1 (callers responsibility).

                    if (R.MultiplyByN().IsInfinity())
                    {
                        var q = Secp256k1PointCalculator.SumOfTwoMultiplies(new Secp256k1PointPreCompCache(), Secp256k1PointCalculator.G, eInvrInv, R, srInv);
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
            Dictionary<string, object> domain,
            Dictionary<string, object> messageTypes,
            Dictionary<string, object> messageData)
        {
            var domainValues = domain.Values.ToArray();

            var typeRaw = new TypedDataRaw();
            var types = new Dictionary<string, MemberDescription[]>();

            // fill in domain types
            var domainTypesDescription = new List<MemberDescription>();
            var domainValuesArray = new List<MemberValue>();

            foreach (var d in _eip721Domain)
            {
                var key = d[0];
                var type = d[1];
                for (var i = 0; i < domain.Count; i++)
                {
                    if (string.Equals(key, domain.Keys.ElementAt(i)))
                    {
                        var memberDescription = new MemberDescription
                        {
                            Name = key,
                            Type = type
                        };
                        domainTypesDescription.Add(memberDescription);

                        var memberValue = new MemberValue
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
            var typeName = messageTypes.Keys.First();
            var messageTypesContent = (IList<object>)messageTypes[typeName];
            var messageTypesDescription = new List<MemberDescription> { };
            for (var i = 0; i < messageTypesContent.Count; i++)
            {
                var elem = (IDictionary<string, object>)messageTypesContent[i];
                var name = (string)elem["name"];
                var type = (string)elem["type"];
                messageTypesDict[name] = type;
                var member = new MemberDescription
                {
                    Name = name,
                    Type = type
                };
                messageTypesDescription.Add(member);
            }
            types[typeName] = messageTypesDescription.ToArray();

            // fill in message values
            var messageValues = new List<MemberValue> { };
            for (var i = 0; i < messageData.Count; i++)
            {
                var kvp = messageData.ElementAt(i);
                var member = new MemberValue
                {
                    TypeName = messageTypesDict[kvp.Key],
                    Value = kvp.Value
                };
                messageValues.Add(member);
            }

            typeRaw.Message = messageValues.ToArray();
            typeRaw.Types = types;
            typeRaw.PrimaryType = typeName;
            return LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
            //            return Eip712TypedDataSigner.Current.EncodeTypedDataRaw(Converter(typeRaw));
        }

        private byte[] GenerateActionHash(object action, long nonce)
        {
            var packer = new PackConverter();
            var dataHex = BytesToHexString(packer.Pack(action));
            var nonceHex = nonce.ToString("x");
            var signHex = dataHex + "00000" + nonceHex + "00";
            var signBytes = ConvertHexStringToByteArray(signHex);
            return SignKeccak(signBytes);
        }

        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
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
            //var keccack = new Sha3Keccack();
            return InternalSha3Keccack.CalculateHash(data);
        }
    }
}
