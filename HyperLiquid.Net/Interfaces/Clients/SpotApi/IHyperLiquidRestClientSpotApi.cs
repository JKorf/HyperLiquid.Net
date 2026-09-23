using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using System;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot API endpoints
    /// </summary>
    public interface IHyperLiquidRestClientSpotApi : IRestApiClient<HyperLiquidCredentials>, IDisposable
    {
        /// <summary>
        /// Endpoints related to account settings, info or actions
        /// </summary>
        /// <see cref="IHyperLiquidRestClientSpotApiAccount"/>
        public IHyperLiquidRestClientSpotApiAccount Account { get; }

        /// <summary>
        /// Endpoints related to retrieving market and system data
        /// </summary>
        /// <see cref="IHyperLiquidRestClientSpotApiExchangeData"/>
        public IHyperLiquidRestClientSpotApiExchangeData ExchangeData { get; }

        /// <summary>
        /// Endpoints related to orders and trades
        /// </summary>
        /// <see cref="IHyperLiquidRestClientSpotApiTrading"/>
        public IHyperLiquidRestClientSpotApiTrading Trading { get; }

        /// <summary>
        /// [V1] Get the shared rest requests client. For new implementations prefer <see cref="SharedApi"/>
        /// </summary>
        public IHyperLiquidRestClientSpotApiShared SharedClient { get; }

        /// <summary>
        /// [V2] Gets the aggregate Shared API interface. Shared APIs provide a common,
        /// exchange-independent contract for accessing functionality across different
        /// exchange client libraries.
        /// </summary>
        public IHyperLiquidRestClientSpotSharedApi SharedApi { get; }
    }
}
