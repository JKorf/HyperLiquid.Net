using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Interfaces.Clients
{
    /// <summary>
    /// Client for accessing the HyperLiquid Rest API. 
    /// </summary>
    public interface IHyperLiquidRestClient : IRestClient<HyperLiquidCredentials>
    {
        /// <summary>
        /// Spot API endpoints
        /// </summary>
        /// <see cref="IHyperLiquidRestClientSpotApi"/>
        public IHyperLiquidRestClientSpotApi SpotApi { get; }
        /// <summary>
        /// Futures API endpoints
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApi"/>
        public IHyperLiquidRestClientFuturesApi FuturesApi { get; }

    }
}
