using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using HyperLiquid.Net.Clients.SpotApi;

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
        /// [V1] Get the shared socket requests client. For new implementations prefer <see cref="SharedApi"/>
        /// </summary>
        public IHyperLiquidSocketClientSpotApiShared SharedClient { get; }

        /// <summary>
        /// [V2] Gets the aggregate Shared API interface. Shared APIs provide a common,
        /// exchange-independent contract for accessing functionality across different
        /// exchange client libraries.
        /// </summary>
        public IHyperLiquidSocketClientSpotSharedApi SharedApi { get; }
    }
}
