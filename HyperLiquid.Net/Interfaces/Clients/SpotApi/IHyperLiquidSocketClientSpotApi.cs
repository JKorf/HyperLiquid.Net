using CryptoExchange.Net.Interfaces.Clients;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot WebSocket endpoints and streams
    /// </summary>
    public interface IHyperLiquidSocketClientSpotApi : ISocketApiClient<HyperLiquidCredentials>
    {
        /// <summary>
        /// Endpoints and streams related to account settings, info or actions
        /// </summary>
        /// <see cref="IHyperLiquidSocketClientSpotApiAccount"/>
        public IHyperLiquidSocketClientSpotApiAccount Account { get; }

        /// <summary>
        /// Endpoints and streams related to retrieving market and system data
        /// </summary>
        /// <see cref="IHyperLiquidSocketClientSpotApiExchangeData"/>
        public IHyperLiquidSocketClientSpotApiExchangeData ExchangeData { get; }

        /// <summary>
        /// Endpoints and streams related to orders and trades
        /// </summary>
        /// <see cref="IHyperLiquidSocketClientSpotApiTrading"/>
        public IHyperLiquidSocketClientSpotApiTrading Trading { get; }

        /// <summary>
        /// Get the shared socket requests client. This interface is shared with other exchanges to allow for a common implementation for different exchanges.
        /// </summary>
        public IHyperLiquidSocketClientSpotApiShared SharedClient { get; }
    }
}
