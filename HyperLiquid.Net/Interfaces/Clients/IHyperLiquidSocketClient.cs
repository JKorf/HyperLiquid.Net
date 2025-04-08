using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Interfaces.Clients
{
    /// <summary>
    /// Client for accessing the HyperLiquid websocket API
    /// </summary>
    public interface IHyperLiquidSocketClient : ISocketClient
    {
        /// <summary>
        /// Futures API endpoints
        /// </summary>
        /// <see cref="IHyperLiquidSocketClientFuturesApi"/>
        public IHyperLiquidSocketClientFuturesApi FuturesApi { get; }
        /// <summary>
        /// Spot API endpoints
        /// </summary>
        /// <see cref="IHyperLiquidSocketClientSpotApi"/>
        public IHyperLiquidSocketClientSpotApi SpotApi { get; }

        /// <summary>
        /// Update specific options
        /// </summary>
        /// <param name="options">Options to update. Only specific options are changeable after the client has been created</param>
        void SetOptions(UpdateOptions options);

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        void SetApiCredentials(ApiCredentials credentials);
    }
}
