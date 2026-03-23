using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures WebSocket endpoints and streams
    /// </summary>
    public interface IHyperLiquidSocketClientFuturesApi : ISocketApiClient<HyperLiquidCredentials>
    {
        /// <summary>
        /// Endpoints and streams related to account settings, info or actions
        /// </summary>
        /// <see cref="IHyperLiquidSocketClientFuturesApiAccount"/>
        public IHyperLiquidSocketClientFuturesApiAccount Account { get; }

        /// <summary>
        /// Endpoints and streams related to retrieving market and system data
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiExchangeData"/>
        public IHyperLiquidSocketClientFuturesApiExchangeData ExchangeData { get; }

        /// <summary>
        /// Endpoints and streams related to orders and trades
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiTrading"/>
        public IHyperLiquidSocketClientFuturesApiTrading Trading { get; }
        
        /// <summary>
        /// Get the shared socket requests client. This interface is shared with other exchanges to allow for a common implementation for different exchanges.
        /// </summary>
        public IHyperLiquidSocketClientFuturesApiShared SharedClient { get; }
    }
}
