using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Clients.SpotApi;
using HyperLiquid.Net.Clients.FuturesApi;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc cref="IHyperLiquidSocketClient" />
    public class HyperLiquidSocketClient : BaseSocketClient<HyperLiquidEnvironment, HyperLiquidCredentials>, IHyperLiquidSocketClient
    {
        #region fields
        internal new HyperLiquidSocketOptions ClientOptions => (HyperLiquidSocketOptions)base.ClientOptions;
        #endregion

        #region Api clients

        /// <inheritdoc />
        public IHyperLiquidSocketClientSpotApi SpotApi { get; }
         /// <inheritdoc />
        public IHyperLiquidSocketClientFuturesApi FuturesApi { get; }

        #endregion

        #region constructor/destructor

        /// <summary>
        /// Create a new instance of HyperLiquidSocketClient
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public HyperLiquidSocketClient(Action<HyperLiquidSocketOptions>? optionsDelegate = null)
            : this(Options.Create(ApplyOptionsDelegate(optionsDelegate)), null)
        {
        }

        /// <summary>
        /// Create a new instance of HyperLiquidSocketClient
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="options">Option configuration</param>
        public HyperLiquidSocketClient(IOptions<HyperLiquidSocketOptions> options, ILoggerFactory? loggerFactory = null) : base(loggerFactory, "HyperLiquid")
        {
            Initialize(options.Value);

            SpotApi = AddApiClient(new HyperLiquidSocketClientSpotApi(_logger, this, options.Value));
            FuturesApi = AddApiClient(new HyperLiquidSocketClientFuturesApi(_logger, this, options.Value));
        }
        #endregion

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<HyperLiquidSocketOptions> optionsDelegate)
        {
            HyperLiquidSocketOptions.Default = ApplyOptionsDelegate(optionsDelegate);
        }
    }
}
