using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using System;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures API endpoints
    /// </summary>
    public interface IHyperLiquidRestClientFuturesApi : IRestApiClient<HyperLiquidCredentials>, IDisposable
    {
        /// <summary>
        /// Endpoints related to account settings, info or actions
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiAccount"/>
        public IHyperLiquidRestClientFuturesApiAccount Account { get; }

        /// <summary>
        /// Endpoints related to retrieving market and system data
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiExchangeData"/>
        public IHyperLiquidRestClientFuturesApiExchangeData ExchangeData { get; }

        /// <summary>
        /// Endpoints related to orders and trades
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiTrading"/>
        public IHyperLiquidRestClientFuturesApiTrading Trading { get; }

        /// <summary>
        /// [V1] Get the shared rest requests client. For new implementations prefer <see cref="SharedApi"/>
        /// </summary>
        public IHyperLiquidRestClientFuturesApiShared SharedClient { get; }

        /// <summary>
        /// [V2] Gets the aggregate Shared API interface. Shared APIs provide a common,
        /// exchange-independent contract for accessing functionality across different
        /// exchange client libraries.
        /// </summary>
        public IHyperLiquidRestClientFuturesSharedApi SharedApi { get; }
    }
}
