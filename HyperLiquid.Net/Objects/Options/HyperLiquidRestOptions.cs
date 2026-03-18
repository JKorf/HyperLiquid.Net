using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Options;
using System;

namespace HyperLiquid.Net.Objects.Options
{
    /// <summary>
    /// Options for the HyperLiquidRestClient
    /// </summary>
    public class HyperLiquidRestOptions : RestExchangeOptions<HyperLiquidEnvironment, HyperLiquidCredentials>
    {
        /// <summary>
        /// Default options for new clients
        /// </summary>
        internal static HyperLiquidRestOptions Default { get; set; } = new HyperLiquidRestOptions()
        {
            Environment = HyperLiquidEnvironment.Live
        };

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidRestOptions()
        {
            Default?.Set(this);
        }

        /// <summary>
        /// The builder fee percentage to apply to orders. This refers to a fee percentage being paid to the developer to support development. Defaults to 1bps/0.01%, but can be set to 0/null. Value be between 0.001% and 0.1%.
        /// </summary>
        public decimal? BuilderFeePercentage { get; set; } = 0.01m;

        /// <summary>
        /// Address of the builder
        /// </summary>
        public string BuilderAddress { get; set; } = "0x64134a9577A857BcC5dAfa42E1647E1439e5F8E7".ToLower();

        /// <summary>
        /// If set requests will only be accepted by the server if they're received within (RequestTime + ExpiresAfter) timespan
        /// </summary>
        public TimeSpan? ExpiresAfter { get; set; }

        /// <summary>
        /// Spot API options
        /// </summary>
        public RestApiOptions SpotOptions { get; private set; } = new RestApiOptions();
        /// <summary>
        /// Futures API options
        /// </summary>
        public RestApiOptions FuturesOptions { get; private set; } = new RestApiOptions();

        internal HyperLiquidRestOptions Set(HyperLiquidRestOptions targetOptions)
        {
            targetOptions = base.Set<HyperLiquidRestOptions>(targetOptions);
            targetOptions.BuilderFeePercentage = BuilderFeePercentage;
            targetOptions.BuilderAddress = BuilderAddress;
            targetOptions.ExpiresAfter = ExpiresAfter;
            targetOptions.SpotOptions = SpotOptions.Set(targetOptions.SpotOptions);
            targetOptions.FuturesOptions = FuturesOptions.Set(targetOptions.FuturesOptions);
            return targetOptions;
        }
    }
}
