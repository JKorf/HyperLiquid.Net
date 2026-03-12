using CryptoExchange.Net.Objects.Options;

namespace HyperLiquid.Net.Objects.Options
{
    /// <summary>
    /// Options for the HyperLiquidSocketClient
    /// </summary>
    public class HyperLiquidSocketOptions : SocketExchangeOptions<HyperLiquidEnvironment, HyperLiquidCredentials>
    {
        /// <summary>
        /// Default options for new clients
        /// </summary>
        internal static HyperLiquidSocketOptions Default { get; set; } = new HyperLiquidSocketOptions()
        {
            Environment = HyperLiquidEnvironment.Live,
            SocketSubscriptionsCombineTarget = 10
        };

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidSocketOptions()
        {
            Default?.Set(this);
        }

        /// <summary>
        /// Spot API options
        /// </summary>
        public SocketApiOptions<HyperLiquidCredentials> SpotOptions { get; private set; } = new SocketApiOptions<HyperLiquidCredentials>()
        {
            MaxSocketConnections = 100
        };

        /// <summary>
        /// Spot API options
        /// </summary>
        public SocketApiOptions<HyperLiquidCredentials> FuturesOptions { get; private set; } = new SocketApiOptions<HyperLiquidCredentials>()
        {
            MaxSocketConnections = 100
        };

        internal HyperLiquidSocketOptions Set(HyperLiquidSocketOptions targetOptions)
        {
            targetOptions = base.Set<HyperLiquidSocketOptions>(targetOptions);            
            targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
            targetOptions.FuturesOptions = FuturesOptions.Set(targetOptions.FuturesOptions);
            return targetOptions;
        }
    }
}
