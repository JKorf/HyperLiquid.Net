using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Objects.Options;
using Microsoft.Extensions.Options;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc />
    public class HyperLiquidSharedApiClient : SharedApiClientBase, IHyperLiquidSharedApiClient
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
            IHyperLiquidSocketClient socketClient,
            IOptions<HyperLiquidOptions> options)
            : base(options.Value.SharedApi.PreferredTransport,
                    restClient.SpotApi.SharedApi,
                    socketClient.SpotApi.SharedApi,
                    restClient.FuturesApi.SharedApi,
                    socketClient.FuturesApi.SharedApi
                  )
        {
            SpotRest = restClient.SpotApi.SharedApi;
            FuturesRest = restClient.FuturesApi.SharedApi;
            SpotSocket = socketClient.SpotApi.SharedApi;
            FuturesSocket = socketClient.FuturesApi.SharedApi;
        }
    }
}
