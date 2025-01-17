using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Clients.SpotApi;
using HyperLiquid.Net.Clients.FuturesApi;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc cref="IHyperLiquidSocketClient" />
    public class HyperLiquidSocketClient : BaseSocketClient, IHyperLiquidSocketClient
    {
        #region fields
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

            SpotApi = AddApiClient(new HyperLiquidSocketClientSpotApi(_logger, options.Value));
            FuturesApi = AddApiClient(new HyperLiquidSocketClientFuturesApi(_logger, options.Value));
        }
        #endregion

        /// <inheritdoc />
        public void SetOptions(UpdateOptions options)
        {
            SpotApi.SetOptions(options);
            FuturesApi.SetOptions(options);
        }

        /// <summary>
        /// Set the default options to be used when creating new clients
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public static void SetDefaultOptions(Action<HyperLiquidSocketOptions> optionsDelegate)
        {
            HyperLiquidSocketOptions.Default = ApplyOptionsDelegate(optionsDelegate);
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            SpotApi.SetApiCredentials(credentials);
            FuturesApi.SetApiCredentials(credentials);
        }
    }
}
