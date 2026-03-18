using CryptoExchange.Net.Authentication;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid API credentials
    /// </summary>
    public class HyperLiquidCredentials : ECDsaCredential
    {
        /// <summary>
        /// Create new credentials providing only credentials in ECDsa format
        /// </summary>
        /// <param name="key">API key</param>
        /// <param name="privateKey">Private key</param>
        public HyperLiquidCredentials(string key, string privateKey) : base(key, privateKey)
        {
        }
    }
}
