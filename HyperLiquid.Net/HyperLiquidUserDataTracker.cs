using CryptoExchange.Net.Trackers.UserData;
using CryptoExchange.Net.Trackers.UserData.Objects;
using HyperLiquid.Net.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace HyperLiquid.Net
{
    /// <inheritdoc />
    public class HyperLiquidUserSpotDataTracker : UserSpotDataTracker
    {
        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidUserSpotDataTracker(
            ILogger<HyperLiquidUserSpotDataTracker> logger,
            IHyperLiquidRestClient restClient,
            IHyperLiquidSocketClient socketClient,
            string? userIdentifier,
            SpotUserDataTrackerConfig? config) : base(
                logger,
                restClient.SpotApi.SharedApi,

                restClient.SpotApi.SharedApi,
                socketClient.SpotApi.SharedApi,

                restClient.SpotApi.SharedApi,
                restClient.SpotApi.SharedApi,
                socketClient.SpotApi.SharedApi,

                restClient.SpotApi.SharedApi,
                socketClient.SpotApi.SharedApi,
                userIdentifier,
                config ?? new SpotUserDataTrackerConfig())
        {
        }
    }

    /// <inheritdoc />
    public class HyperLiquidUserFuturesDataTracker : UserFuturesDataTracker
    {
        /// <inheritdoc />
        protected override bool WebsocketPositionUpdatesAreFullSnapshots => true;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidUserFuturesDataTracker(
            ILogger<HyperLiquidUserFuturesDataTracker> logger,
            IHyperLiquidRestClient restClient,
            IHyperLiquidSocketClient socketClient,
            string? userIdentifier,
            FuturesUserDataTrackerConfig? config) : base(logger,
                restClient.FuturesApi.SharedApi,

                restClient.FuturesApi.SharedApi,
                socketClient.FuturesApi.SharedApi,

                restClient.FuturesApi.SharedApi,
                restClient.FuturesApi.SharedApi,
                socketClient.FuturesApi.SharedApi,

                restClient.FuturesApi.SharedApi,
                socketClient.FuturesApi.SharedApi,

                restClient.FuturesApi.SharedApi,
                socketClient.FuturesApi.SharedApi,
                userIdentifier,
                config ?? new FuturesUserDataTrackerConfig())
        {
        }
    }
}
