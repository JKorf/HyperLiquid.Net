using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using CryptoExchange.Net.Authentication;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Clients;
using Microsoft.Extensions.Options;
using CryptoExchange.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Clients.SpotApi;
using HyperLiquid.Net.Clients.FuturesApi;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc cref="IHyperLiquidRestClient" />
    public class HyperLiquidRestClient : BaseRestClient, IHyperLiquidRestClient
    {
        #region Api clients
                
         /// <inheritdoc />
        public IHyperLiquidRestClientSpotApi SpotApi { get; }
         /// <inheritdoc />
        public IHyperLiquidRestClientFuturesApi FuturesApi { get; }

        #endregion

        #region constructor/destructor

        /// <summary>
        /// Create a new instance of the HyperLiquidRestClient using provided options
        /// </summary>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public HyperLiquidRestClient(Action<HyperLiquidRestOptions>? optionsDelegate = null)
            : this(null, null, Options.Create(ApplyOptionsDelegate(optionsDelegate)))
        {
        }

        /// <summary>
        /// Create a new instance of the HyperLiquidRestClient using provided options
        /// </summary>
        /// <param name="options">Option configuration</param>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="httpClient">Http client for this client</param>
        public HyperLiquidRestClient(HttpClient? httpClient, ILoggerFactory? loggerFactory, IOptions<HyperLiquidRestOptions> options) : base(loggerFactory, "HyperLiquid")
        {
            Initialize(options.Value);
                        
            SpotApi = AddApiClient(new HyperLiquidRestClientSpotApi(_logger, this, httpClient, options.Value));
            FuturesApi = AddApiClient(new HyperLiquidRestClientFuturesApi(_logger, this, httpClient, options.Value));
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
        public static void SetDefaultOptions(Action<HyperLiquidRestOptions> optionsDelegate)
        {
            HyperLiquidRestOptions.Default = ApplyOptionsDelegate(optionsDelegate);
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {            
            SpotApi.SetApiCredentials(credentials);
            FuturesApi.SetApiCredentials(credentials);
        }
    }
}
