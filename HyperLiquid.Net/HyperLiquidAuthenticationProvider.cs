using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Authentication.Signing;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Utils;
using Secp256k1Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HyperLiquid.Net
{
    internal class HyperLiquidAuthenticationProvider : AuthenticationProvider<HyperLiquidCredentials, HyperLiquidCredentials>
    {
        private static readonly object _nonceLock = new object();
        private static long? _lastNonce;

        private static IEnumerable<(string Name, string Type, object Value)> GetDomainFields(
            string action,
            string version,
            int chainId,
            string verifyingAddress)
        {
            return [
                ("name", "string", action),
                ("version", "string", version),
                ("chainId", "uint256", chainId),
                ("verifyingContract", "address", verifyingAddress),
                ];
        }

        private static readonly Dictionary<Type, string> _typeMapping = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(long), "uint64" },
            { typeof(bool), "bool" },
            { typeof(byte[]), "bytes32" }
        };

        public HyperLiquidAuthenticationProvider(HyperLiquidCredentials credentials) : base(credentials, credentials)
        {
        }

        public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration request)
        {
            if (!request.Authenticated)
                return;

            var action = (Dictionary<string, object>)request.BodyParameters!["action"];
            var nonce = GetNonce(action);
            request.BodyParameters!.Add("nonce", nonce);

            var signature = GenerateSignature(
                ((HyperLiquidRestOptions)apiClient.ClientOptions).Environment,
                action,
                nonce,
                request.BodyParameters.TryGetValue("vaultAddress", out var vaultAddressObj) ? (string)vaultAddressObj : null,
                request.BodyParameters.TryGetValue("expiresAfter", out var expiresAfterObj) ? (long)expiresAfterObj : null);
            request.BodyParameters["signature"] = signature;
        }

        public void ProcessRequest(SocketApiClient apiClient, ParameterCollection request)
        {
            var action = (Dictionary<string, object>)request["action"];
            var nonce = GetNonce(action);
            request.Add("nonce", nonce);

            var signature = GenerateSignature(
                ((HyperLiquidSocketOptions)apiClient.ClientOptions).Environment,
                action,
                nonce,
                request.TryGetValue("vaultAddress", out var vaultAddressObj) ? (string)vaultAddressObj : null,
                request.TryGetValue("expiresAfter", out var expiresAfterObj) ? (long)expiresAfterObj : null);
            request["signature"] = signature;
        }

        private long GetNonce(Dictionary<string, object> actionParameters)
        {
            if (actionParameters.TryGetValue("time", out var time))
                return (long)time;

            if (actionParameters.TryGetValue("nonce", out var n))
                return (long)n;

            var nonce = DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow).Value;
            lock (_nonceLock)
            {
                if (nonce <= _lastNonce)
                    nonce = _lastNonce.Value + 1;

                _lastNonce = nonce;
            }

            return nonce;
        }

        internal Dictionary<string, object> GenerateSignature(
            HyperLiquidEnvironment environment,
            Dictionary<string, object> action,
            long nonce,
            string? vaultAddress,
            long? expiresAfter)
        {
            byte[] messageBytes;
            if (action.TryGetValue("signatureChainId", out var chainId))
            {
                // User action
                var actionName = (string)action["type"];
                if (actionName == "withdraw3")
                    actionName = "withdraw";

                actionName = actionName.Substring(0, 1).ToUpperInvariant() + actionName.Substring(1);
                var primary = "HyperliquidTransaction:" + actionName;
                var messageInnerFields = GetMessageFields(action.Where(x => x.Key != "type" && x.Key != "signatureChainId").ToDictionary(x => x.Key, x => x.Value));

                var domainFields = GetDomainFields("HyperliquidSignTransaction", "1", Convert.ToInt32((string)chainId, 16), "0x0000000000000000000000000000000000000000");

                if (HyperLiquidExchange.AsyncTypedDataSignRequestDelegate != null)
                    return HyperLiquidExchange.AsyncTypedDataSignRequestDelegate(primary, domainFields, messageInnerFields);

                messageBytes = CeEip712TypedDataEncoder.EncodeEip721(primary, domainFields, messageInnerFields);
            }
            else
            {
                // Exchange action
                if (vaultAddress != null)
                    vaultAddress = vaultAddress.StartsWith("0x") ? vaultAddress.Substring(2) : vaultAddress;

                var hash = GenerateActionHash(action, nonce, vaultAddress, expiresAfter);
                var phantomAgent = new Dictionary<string, object>()
                {
                    { "source", environment.Name == TradeEnvironmentNames.Testnet ? "b" : "a" },
                    { "connectionId", hash },
                };

                var messageFields = GetMessageFields(phantomAgent);
                var domainFields = GetDomainFields("Exchange", "1", 1337, "0x0000000000000000000000000000000000000000");

                if (HyperLiquidExchange.AsyncTypedDataSignRequestDelegate != null)
                    return HyperLiquidExchange.AsyncTypedDataSignRequestDelegate("Agent", domainFields, messageFields);

                messageBytes = CeEip712TypedDataEncoder.EncodeEip721("Agent", domainFields, messageFields);
            }

            var keccakSigned = CeSha3Keccack.CalculateHash(messageBytes);

            Dictionary<string, object> signature;
            var effectiveDelegate = HyperLiquidExchange.SignRequestDelegate;
            if (effectiveDelegate != null)
                signature = effectiveDelegate(BytesToHexString(keccakSigned), Credential.PrivateKey);
            else
                signature = SignRequest(keccakSigned, Credential.PrivateKey);

            return signature;
        }

        private List<(string Name, string Type, object Value)> GetMessageFields(Dictionary<string, object> parameters)
        {
            var result = new List<(string, string, object)>();

            foreach (var parameter in parameters)
            {
                result.Add(
                    (parameter.Key,
                    ((parameter.Key == "builder" || parameter.Key == "user") ? "address" : _typeMapping[parameter.Value.GetType()]),
                    parameter.Key == "type" ? ((string)parameter.Value).Substring(0, 1).ToUpper() + ((string)parameter.Value).Substring(1) : parameter.Value));
            }

            return result;
        }

        public static Dictionary<string, object> SignRequest(byte[] request, string secret)
        {
            (var signature, var recover) = Secp256k1.SignRecoverable(request, HexToBytesString(secret));
            var hexCompactR = BytesToHexString(new ArraySegment<byte>(signature, 0, 32));
            var hexCompactS = BytesToHexString(new ArraySegment<byte>(signature, 32, 32));
            var hexCompactV = recover + 27;

            return new Dictionary<string, object>
            {
                { "r", "0x" + hexCompactR },
                { "s", "0x" + hexCompactS },
                { "v", hexCompactV },
            };
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

            var signBytes = signHex.HexToByteArray();
            return CeSha3Keccack.CalculateHash(signBytes);
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
