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
                restClient.SpotApi.SharedClient,
                null,
                restClient.SpotApi.SharedClient,
                socketClient.SpotApi.SharedClient,
                restClient.SpotApi.SharedClient,
                socketClient.SpotApi.SharedClient,
                socketClient.SpotApi.SharedClient,
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
                restClient.FuturesApi.SharedClient,
                null,
                restClient.FuturesApi.SharedClient,
                socketClient.FuturesApi.SharedClient,
                restClient.FuturesApi.SharedClient,
                socketClient.FuturesApi.SharedClient,
                socketClient.FuturesApi.SharedClient,
                socketClient.FuturesApi.SharedClient,
                userIdentifier,
                config ?? new FuturesUserDataTrackerConfig())
        {
        }
    }
}
