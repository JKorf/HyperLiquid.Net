using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid API credentials
    /// </summary>
    public class HyperLiquidCredentials : ApiCredentials
    {
        /// <summary>
        /// </summary>
        [Obsolete("Parameterless constructor is only for deserialization purposes and should not be used directly. Use parameterized constructor instead.")]
        public HyperLiquidCredentials() { }

        /// <summary>
        /// Create credentials using an HMAC key, and secret
        /// </summary>
        /// <param name="apiKey">The API key</param>
        /// <param name="secret">The API secret</param>
        public HyperLiquidCredentials(string apiKey, string secret)
            : this(new ECDSACredential(apiKey, secret)) { }

        /// <summary>
        /// Create HyperLiquid credentials using HMAC credentials
        /// </summary>
        /// <param name="credential">The HMAC credentials</param>
        public HyperLiquidCredentials(ECDSACredential credential)
            : base(credential) { }

        /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
        public override ApiCredentials Copy() => new HyperLiquidCredentials { CredentialPairs = CredentialPairs };
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
