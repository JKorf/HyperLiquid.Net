using CryptoExchange.Net.Authentication;
using System;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid API credentials
    /// </summary>
    public class HyperLiquidCredentials : ECDsaCredential
    {
        /// <summary>
        /// Create new credentials
        /// </summary>
        public HyperLiquidCredentials() { }


        /// <summary>
        /// Create new credentials providing only credentials in ECDsa format
        /// </summary>
        /// <param name="key">API key</param>
        /// <param name="privateKey">Private key</param>
        public HyperLiquidCredentials(string key, string privateKey) : base(key, privateKey)
        {
        }

        /// <summary>
        /// Create new credentials providing ECDsa credentials
        /// </summary>
        /// <param name="credential">ECDsa credentials</param>
        public HyperLiquidCredentials(ECDsaCredential credential) : base(credential.Key, credential.PrivateKey)
        {
        }

        /// <summary>
        /// Specify the ECDsa credentials
        /// </summary>
        /// <param name="key">API key</param>
        /// <param name="privateKey">Private key</param>
        public HyperLiquidCredentials WithECDsa(string key, string privateKey)
        {
            if (!string.IsNullOrEmpty(Key)) throw new InvalidOperationException("Credentials already set");

            Key = key;
            PrivateKey = privateKey;
            return this;
        }
    }
}
