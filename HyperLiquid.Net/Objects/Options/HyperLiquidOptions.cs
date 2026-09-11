using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;

namespace HyperLiquid.Net.Objects.Options
{
    /// <summary>
    /// HyperLiquid options
    /// </summary>
    public class HyperLiquidOptions : LibraryOptions<HyperLiquidRestOptions, HyperLiquidSocketOptions, HyperLiquidCredentials, HyperLiquidEnvironment>
    {
        /// <summary>
        /// Options for Shared API usage
        /// </summary>
        public SharedApiOptions SharedApi { get; set; } = new();
    }
}
