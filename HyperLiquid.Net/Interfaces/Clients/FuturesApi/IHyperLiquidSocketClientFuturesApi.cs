using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using HyperLiquid.Net.Clients.FuturesApi;

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
        /// [V1] Get the shared socket requests client. For new implementations prefer <see cref="SharedApi"/>
        /// </summary>
        public IHyperLiquidSocketClientFuturesApiShared SharedClient { get; }
        /// <summary>
        /// [V2] Gets the aggregate Shared API interface. Shared APIs provide a common,
        /// exchange-independent contract for accessing functionality across different
        /// exchange client libraries.
        /// </summary>
        public IHyperLiquidSocketClientFuturesSharedApi SharedApi { get; }
    }
}
