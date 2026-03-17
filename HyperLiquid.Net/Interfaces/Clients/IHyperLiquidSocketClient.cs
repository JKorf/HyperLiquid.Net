using CryptoExchange.Net.Interfaces.Clients;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Interfaces.Clients
{
    /// <summary>
    /// Client for accessing the HyperLiquid websocket API
    /// </summary>
    public interface IHyperLiquidSocketClient : ISocketClient<HyperLiquidCredentials>
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

    }
}
