using CryptoExchange.Net.Interfaces;
using System;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// HyperLiquid spot API endpoints
    /// </summary>
    public interface IHyperLiquidRestClientSpotApi : IRestApiClient, IDisposable
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
        /// Get the shared rest requests client. This interface is shared with other exchanges to allow for a common implementation for different exchanges.
        /// </summary>
        public IHyperLiquidRestClientSpotApiShared SharedClient { get; }
    }
}
