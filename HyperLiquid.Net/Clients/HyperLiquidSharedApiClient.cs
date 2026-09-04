using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc />
    public class HyperLiquidSharedApiClient : IHyperLiquidSharedApiClient
    {
        /// <inheritdoc />
        public IHyperLiquidRestClientSpotSharedApi SpotRest { get; }
        /// <inheritdoc />
        public IHyperLiquidRestClientFuturesSharedApi FuturesRest { get; }
        /// <inheritdoc />
        public IHyperLiquidSocketClientSpotSharedApi SpotSocket { get; }
        /// <inheritdoc />
        public IHyperLiquidSocketClientFuturesSharedApi FuturesSocket { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidSharedApiClient(
            IHyperLiquidRestClient restClient,
            IHyperLiquidSocketClient socketClient)
        {
            SpotRest = restClient.SpotApi.SharedApi;
            FuturesRest = restClient.FuturesApi.SharedApi;
            SpotSocket = socketClient.SpotApi.SharedApi;
            FuturesSocket = socketClient.FuturesApi.SharedApi;
        }
    }
}
