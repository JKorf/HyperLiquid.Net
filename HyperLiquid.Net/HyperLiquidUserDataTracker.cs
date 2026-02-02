using CryptoExchange.Net.Trackers.UserData;
using HyperLiquid.Net.Interfaces.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    public class HyperLiquidUserSpotDataTracker : UserSpotDataTracker
    {
        public HyperLiquidUserSpotDataTracker(
            ILogger<HyperLiquidUserSpotDataTracker> logger,
            IHyperLiquidRestClient restClient,
            IHyperLiquidSocketClient socketClient,
            string? userIdentifier,
            UserDataTrackerConfig config) : base(logger, restClient.SpotApi.SharedClient, socketClient.SpotApi.SharedClient, userIdentifier, config)
        {

        }
    }

    public class HyperLiquidUserFuturesDataTracker : UserFuturesDataTracker
    {
        protected override bool WebsocketPositionUpdatesAreFullSnapshots => true;

        public HyperLiquidUserFuturesDataTracker(
            ILogger<HyperLiquidUserFuturesDataTracker> logger,
            IHyperLiquidRestClient restClient,
            IHyperLiquidSocketClient socketClient,
            string? userIdentifier,
            UserDataTrackerConfig config) : base(logger, restClient.FuturesApi.SharedClient, socketClient.FuturesApi.SharedClient, userIdentifier, config)
        {

        }
    }
}
